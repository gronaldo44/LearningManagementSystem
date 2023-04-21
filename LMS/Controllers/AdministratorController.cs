using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext _db;

        public AdministratorController(LMSContext _db)
        {
            this._db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {

            var existingDepartment = _db.Departments.FirstOrDefault(d => d.Subject == subject);

            if (existingDepartment != null)
            {
                return Json(new { success = false });
            }

            var newDepartment = new Department { Subject = subject, Name = name };
            _db.Departments.Add(newDepartment);
            _db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {

            var courses = _db.Courses
       .Where(c => c.Subject == subject)
       .Select(c => new
       {
           number = c.CNum,
           name = c.Name
       })
       .ToList();

            return Json(courses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {

            var professors = _db.Professors
         .Where(p => p.WorksIn == subject)
         .Select(p => new
         {
             lname = p.LName,
             fname = p.FName,
             uid = p.UId
         })
         .ToList();

            return Json(professors);

        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Check if the course already exists
            var existingCourse = _db.Courses.FirstOrDefault(c => c.Subject == subject && c.CNum == number);
            if (existingCourse != null)
            {
                return Json(new { success = false });
            }

            // Check if the department exists
            var department = _db.Departments.FirstOrDefault(d => d.Subject == subject);
            if (department == null)
            {
                return Json(new { success = false });
            }

            // Create the course
            var newCourse = new Course { Subject = subject, CNum = Convert.ToUInt32(number), Name = name };
            _db.Courses.Add(newCourse);
            _db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            // Check if the timeslot is occupied for that location
            var timeslotIsOccupied = _db.Classes.
                Where(c => c.Season == season &&
                    c.Location == location &&
                    c.Year == year &&
                    (c.StartTime >= TimeOnly.FromTimeSpan(start.TimeOfDay) && c.StartTime <= TimeOnly.FromTimeSpan(end.TimeOfDay) ||
                    c.EndTime >= TimeOnly.FromTimeSpan(start.TimeOfDay) && c.EndTime <= TimeOnly.FromTimeSpan(end.TimeOfDay))).Any();
            if (timeslotIsOccupied)
            {
                return Json(new { success = false });
            }
            // Check if another professor is already teaching the course that semester
            var isAlreadyTaught = from cour in _db.Courses.Where(c => c.CNum == number)
                                     join clas in _db.Classes.
                    Where(c => c.Season == season &&
                        c.Year == year)
                    on cour.CId equals clas.CId into cour_clas
                                     from cour_clas_ in cour_clas.DefaultIfEmpty()
                                     select new
                                     {
                                         tmp = cour_clas_.ClassId
                                     };
            if (isAlreadyTaught.Any())
            {
                return Json(new {success = false});
            }

            // Create the Class with the given course and professor
            var courseListing = from c in _db.Courses.Where(c => c.CNum == number)
                                select new
                                {
                                    listing = c.CId
                                };
            var newClass = new Class
            {
                Year = Convert.ToUInt32(year),
                Season = season,
                Location = location,
                StartTime = TimeOnly.FromTimeSpan(start.TimeOfDay),
                EndTime = TimeOnly.FromTimeSpan(end.TimeOfDay),
                CId = courseListing.First().listing,
                TaughtBy = instructor
            };
            // Add the class to the database
            _db.Classes.Add(newClass);
            _db.SaveChanges();
            return Json(new { success = true });
        }


        /*******End code to modify********/

    }
}

