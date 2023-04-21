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
    public class CommonController : Controller
    {
        private readonly LMSContext _db;

        public CommonController(LMSContext _db)
        {
            this._db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var departments = _db.Departments.Select(d => new
            {
                name = d.Name,
                subject = d.Subject
            }).ToList();

            return Json(departments);
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog = _db.Departments.Select(d => new
            {
                subject = d.Subject,
                dname = d.Name,
                courses = d.Courses.Select(c => new
                {
                    number = c.CNum,
                    cname = c.Name
                }).ToList()
            }).ToList();

            return Json(catalog);
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var classOfferings = from courses in _db.Courses.
                                 Where(c => c.CNum == number && subject == c.Subject)
                                 join classes in _db.Classes
                                 on courses.CId equals classes.CId into courses_classes
                                 from courses_classes_ in courses_classes.DefaultIfEmpty()
                                 join courses_classes_professors in _db.Professors
                                 on courses_classes_.TaughtBy equals courses_classes_professors.UId into courses_classes_professors
                                 from courses_classes_professors_ in courses_classes_professors.DefaultIfEmpty()
                                 select new
                                 {
                                     season = courses_classes_.Season,
                                     year = courses_classes_.Year,
                                     location = courses_classes_.Location,
                                     start = $"{courses_classes_.StartTime:hh\\:mm\\:ss}",
                                     end = $"{courses_classes_.EndTime:hh\\:mm\\:ss}",
                                     fname = courses_classes_professors_.FName,
                                     lname = courses_classes_professors_.LName
                                 };

            return Json(classOfferings);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var assignment = from courses in _db.Courses.Where(c =>
                                c.CNum == num && c.Subject == subject)
                             join classes in _db.Classes.Where(c =>
                                c.Season == season && c.Year == year)
                             on courses.CId equals classes.CId into courses_classes
                             from courses_classes_ in courses_classes.DefaultIfEmpty()
                             join aCateg in _db.AssignmentCategories.Where(ac => ac.Name == category)
                             on courses_classes_.ClassId equals aCateg.ClassId into courses_classes_aCateg
                             from courses_classes_aCateg_ in courses_classes_aCateg.DefaultIfEmpty()
                             join asg in _db.Assignments.Where(asg => asg.Name == asgname)
                             on courses_classes_aCateg_.CategId equals asg.CategId into courses_classes_aCateg_asg
                             from courses_classes_aCateg_asg_ in courses_classes_aCateg_asg.DefaultIfEmpty()
                             select new
                             {
                                 contents = courses_classes_aCateg_asg_.Contents
                             };
            return Content(assignment.First().contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {

            var submission = from courses in _db.Courses.Where(c =>
                                c.CNum == num && c.Subject == subject)
                             join classes in _db.Classes.Where(c =>
                                c.Season == season && c.Year == year)
                             on courses.CId equals classes.CId into courses_classes
                             from courses_classes_ in courses_classes.DefaultIfEmpty()
                             join aCateg in _db.AssignmentCategories.Where(ac => ac.Name == category)
                             on courses_classes_.ClassId equals aCateg.ClassId into courses_classes_aCateg
                             from courses_classes_aCateg_ in courses_classes_aCateg.DefaultIfEmpty()
                             join asg in _db.Assignments.Where(asg => asg.Name == asgname)
                             on courses_classes_aCateg_.CategId equals asg.CategId into courses_classes_aCateg_asg
                             from courses_classes_aCateg_asg_ in courses_classes_aCateg_asg.DefaultIfEmpty()
                             join s in _db.Submissions.Where(s => s.Student == uid)
                             on courses_classes_aCateg_asg_.AId equals s.Assignment into courses_classes_aCateg_asg_s
                             from courses_classes_aCaeg_asg_s_ in courses_classes_aCateg_asg_s.DefaultIfEmpty()
                             select new
                             {
                                 submissionText = courses_classes_aCaeg_asg_s_.SubmissionContents
                             };

            return Content(submission.First().submissionText);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            //Create a professor instance to test for professor. 
            var professor = from p in _db.Professors.Where(p => p.UId == uid)
                            select new
                            {
                                fname = p.FName,
                                lname = p.LName,
                                uid = p.UId,
                                department = p.WorksIn
                            };
            if (professor.Any())
            {
                return Json(professor.FirstOrDefault());
            }

            //Create a student instance to test for student.
            var student = from s in _db.Students.Where(s => s.UId == uid)
                            select new
                            {
                                fname = s.Fname,
                                lname = s.Lname,
                                uid = s.UId,
                                department = s.Major
                            };
            if (student.Any())
            {
                return Json(student.FirstOrDefault());
            }

            //Create a administrator instance to test for administrator.
            var administrator = from a in _db.Administrators.Where(a => a.UId == uid)
                                select new
                                {
                                    fname = a.Fname,
                                    lname = a.Lname,
                                    uid = a.UId
                                };
            if (administrator.Any())
            {
                return Json(administrator.FirstOrDefault());
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

