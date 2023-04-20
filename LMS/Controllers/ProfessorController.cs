using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students = _db.Enrollments
       .Include(e => e.Student)
       .Include(e => e.Class.Course)
       .Where(e => e.Class.Course.Department.Subject == subject &&
                   e.Class.Course.Number == num &&
                   e.Class.Semester == season &&
                   e.Class.Year == year)
       .Select(e => new
       {
           fname = e.Student.FirstName,
           lname = e.Student.LastName,
           uid = e.Student.UId,
           dob = e.Student.DOB,
           grade = e.Grade
       })
       .ToList();

            return Json(students);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var assignments = _db.Assignments
        .Include(a => a.Category.Class.Course)
        .Where(a => a.Category.Class.Course.Department.Subject == subject &&
                    a.Category.Class.Course.Number == num &&
                    a.Category.Class.Semester == season &&
                    a.Category.Class.Year == year &&
                    (category == null || a.Category.Name == category))
        .Select(a => new
        {
            aname = a.Name,
            cname = a.Category.Name,
            due = a.DueDate,
            submissions = a.Submissions.Count
        })
        .ToList();

            return Json(assignments);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var categories = _db.AssignmentCategories
       .Include(c => c.Class.Course)
       .Where(c => c.Class.Course.Department.Subject == subject &&
                   c.Class.Course.Number == num &&
                   c.Class.Semester == season &&
                   c.Class.Year == year)
       .Select(c => new
       {
           name = c.Name,
           weight = c.Weight
       })
       .ToList();

            return Json(categories);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var existingCategory = _db.AssignmentCategories
         .Include(c => c.Class.Course)
         .FirstOrDefault(c => c.Class.Course.Department.Subject == subject &&
                              c.Class.Course.Number == num &&
                              c.Class.Semester == season &&
                              c.Class.Year == year &&
                              c.Name == category);

            if (existingCategory != null)
            {
                return Json(new { success = false });
            }

            var targetClass = _db.Classes
                .Include(c => c.Course)
                .FirstOrDefault(c => c.Course.Department.Subject == subject &&
                                     c.Course.Number == num &&
                                     c.Semester == season &&
                                     c.Year == year);

            if (targetClass == null)
            {
                return Json(new { success = false });
            }

            var newCategory = new AssignmentCategory { Name = category, Weight = catweight, Class = targetClass };
            _db.AssignmentCategories.Add(newCategory);
            _db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var existingAssignment = _db.Assignments
        .Include(a => a.Category.Class.Course)
        .FirstOrDefault(a => a.Category.Class.Course.Department.Subject == subject &&
                             a.Category.Class.Course.Number == num &&
                             a.Category.Class.Semester == season &&
                             a.Category.Class.Year == year &&
                             a.Category.Name == category &&
                             a.Name == asgname);

            if (existingAssignment != null)
            {
                return Json(new { success = false });
            }

            var targetCategory = _db.AssignmentCategories
                .Include(c => c.Class.Course)
                .FirstOrDefault(c => c.Class.Course.Department.Subject == subject &&
                                     c.Class.Course.Number == num &&
                                     c.Class.Semester == season &&
                                     c.Class.Year == year &&
                                     c.Name == category);

            if (targetCategory == null)
            {
                return Json(new { success = false });
            }

            var newAssignment = new Assignment { Name = asgname, MaxPoints = asgpoints, DueDate = asgdue, Contents = asgcontents, Category = targetCategory };
            _db.Assignments.Add(newAssignment);
            _db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var submissions = _db.Submissions
        .Include(s => s.Student)
        .Include(s => s.Assignment.Category.Class.Course)
        .Where(s => s.Assignment.Category.Class.Course.Department.Subject == subject &&
                    s.Assignment.Category.Class.Course.Number == num &&
                    s.Assignment.Category.Class.Semester == season &&
                    s.Assignment.Category.Class.Year == year &&
                    s.Assignment.Category.Name == category &&
                    s.Assignment.Name == asgname)
        .Select(s => new
        {
            fname = s.Student.FirstName,
            lname = s.Student.LastName,
            uid = s.Student.UId,
            time = s.Time,
            score = s.Score
        })
        .ToList();

            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var submission = _db.Submissions
        .Include(s => s.Student)
        .Include(s => s.Assignment.Category.Class.Course)
        .FirstOrDefault(s => s.Assignment.Category.Class.Course.Department.Subject == subject &&
                             s.Assignment.Category.Class.Course.Number == num &&
                             s.Assignment.Category.Class.Semester == season &&
                             s.Assignment.Category.Class.Year == year &&
                             s.Assignment.Category.Name == category &&
                             s.Assignment.Name == asgname &&
                             s.Student.UId == uid);

            if (submission == null)
            {
                return Json(new { success = false });
            }

            submission.Score = score;
            _db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = _db.Classes
        .Include(c => c.Course)
        .Include(c => c.Professor)
        .Where(c => c.Professor.UId == uid)
        .Select(c => new
        {
            subject = c.Course.Department.Subject,
            number = c.Course.Number,
            name = c.Course.Name,
            season = c.Semester,
            year = c.Year
        })
        .ToList();

            return Json(classes);
        }



        /*******End code to modify********/
    }
}

