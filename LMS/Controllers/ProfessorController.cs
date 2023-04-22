using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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

        private readonly LMSContext _db;

        public ProfessorController(LMSContext _db)
        {
            this._db = _db;
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
            var students = from cour in _db.Courses.Where(c => c.CNum == num && c.Subject == subject)
                           join clas in _db.Classes.Where(c => c.Season == season && c.Year == year)
                           on cour.CId equals clas.CId into cour_clas
                           from cour_clas_ in cour_clas.DefaultIfEmpty()
                           join e in _db.Enrolleds
                           on cour_clas_.ClassId equals e.Class into cour_clas_e
                           from cour_clas_e_ in cour_clas_e.DefaultIfEmpty()
                           join s in _db.Students
                           on cour_clas_e_.Student equals s.UId into cour_clas_e_s
                           from cour_clas_e_s_ in cour_clas_e_s.DefaultIfEmpty()
                           select new
                           {
                               fname = cour_clas_e_s_.Fname,
                               lname = cour_clas_e_s_.Lname,
                               uid = cour_clas_e_s_.UId,
                               dob = cour_clas_e_s_.Dob,
                               grade = cour_clas_e_.Grade
                           };
            return Json(students.ToArray());
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
            var assignments = from asgn in _db.Assignments
                              join asgn_asgnCat in _db.AssignmentCategories on asgn.CategId equals asgn_asgnCat.CategId
                              join asgn_asgnCat_class in _db.Classes on asgn_asgnCat.ClassId equals asgn_asgnCat_class.ClassId
                              join asgn_asgnCat_class_course in _db.Courses on asgn_asgnCat_class.CId equals asgn_asgnCat_class_course.CId
                              where asgn_asgnCat_class_course.Subject == subject &&
                                    asgn_asgnCat_class_course.CNum == num &&
                                    asgn_asgnCat_class.Season == season &&
                                    asgn_asgnCat_class.Year == year &&
                                    (category == null || asgn_asgnCat.Name == category)
                              select new
                              {
                                  aname = asgn.Name,
                                  cname = asgn_asgnCat.Name,
                                  due = asgn.Due,
                                  submissions = asgn.Submissions.Count
                              };
            return Json(assignments.ToArray());
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
            var asgnCategories = from cour in _db.Courses.Where(c => c.CNum == num && c.Subject == subject)
                                 join clas in _db.Classes.Where(c => c.Season == season && c.Year == year)
                                 on cour.CId equals clas.CId into cour_clas
                                 from cour_clas_ in cour_clas.DefaultIfEmpty()
                                 join ac in _db.AssignmentCategories
                                 on cour_clas_.ClassId equals ac.ClassId into cour_clas_ac
                                 from cour_clas_ac_ in cour_clas_ac.DefaultIfEmpty()
                                 select new
                                 {
                                     name = cour_clas_ac_.Name,
                                     weight = cour_clas_ac_.GradingWeight
                                 };
            return Json(asgnCategories.ToArray());
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
            // check if the category already exists for that class
            bool categoryTaken = (from cl in _db.Classes
                                  join co in _db.Courses on cl.CId equals co.CId
                                  join ac in _db.AssignmentCategories on cl.ClassId equals ac.ClassId
                                  where cl.Season == season && cl.Year == year &&
                                     co.CNum == num && co.Subject == subject &&
                                     ac.Name == category
                                  select new
                                  {
                                      tmp = ac.CategId
                                  }).Any();
            if (categoryTaken)
            {
                return Json(new { success = false});
            }

            // Find class for the new category
            var targetClass = (from cl in _db.Classes
                              join co in _db.Courses on cl.CId equals co.CId
                              where co.Subject == subject && co.CNum == num &&
                                cl.Season == season && cl.Year == year
                              select cl).SingleOrDefault();
            if (targetClass == null)
            {
                return Json(new { success = false });
            }
            // Create new assignment category for the target class
            var newCategory = new AssignmentCategory
            {
                Name = category,
                GradingWeight = Convert.ToUInt32(catweight),
                ClassId = targetClass.ClassId
            };
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
            // check if the assignment already exists
            var asgnExists = from asgn in _db.Assignments.Where(a =>
                                    a.Name == asgname &&
                                    a.MaxPoints == asgpoints &&
                                    a.Due == asgdue &&
                                    a.Contents == asgcontents)
                             join asgnCat in _db.AssignmentCategories
                             on asgn.CategId equals asgnCat.CategId into asgn_asgnCat
                             from asgn_asgnCat_ in asgn_asgnCat.DefaultIfEmpty()
                             select new
                             {
                                 tmp = asgn_asgnCat_.CategId
                             };
            if (asgnExists.Any())
            {
                return Json(new { success = false });
            }

            // check if the category exists for that class
            var existingCategory = from cour in _db.Courses.Where(c => c.CNum == num && c.Subject == subject)
                                   join clas in _db.Classes.Where(c => c.Season == season && c.Year == year)
                                   on cour.CId equals clas.CId into cour_clas
                                   from cour_clas_ in cour_clas.DefaultIfEmpty()
                                   join ac in _db.AssignmentCategories.Where(ac => ac.Name == category)
                                   on cour_clas_.ClassId equals ac.ClassId into cour_clas_ac
                                   from cour_clas_ac_ in cour_clas_ac.DefaultIfEmpty()
                                   select new
                                   {
                                       categId = cour_clas_ac_.CategId
                                   };
            if (!existingCategory.Any())
            {
                return Json(new { success = false });
            }

            // create the new assignment
            var newAssignment = new Assignment
            {
                Name = asgname,
                MaxPoints = Convert.ToUInt32(asgpoints),
                Due = asgdue,
                Contents = asgcontents,
                CategId = existingCategory.First().categId
            };
            // Add new assignment to the database
            _db.Assignments.Add(newAssignment);

            // Fetch the list of students enrolled in the class
            var enrolledStudents = (from enrollment in _db.Enrolleds
                                    where enrollment.Class == num
                                    join student in _db.Students
                                    on enrollment.Student equals student.UId
                                    select student).ToList();

            // Recalculate and update the grades for all students enrolled in the class
            foreach (var student in enrolledStudents)
            {
                string updatedLetterGrade = CalculateStudentGrade(student.UId);
                // Update the student's grade in the database
                var studentEnrollment = _db.Enrolleds.FirstOrDefault(e => e.Student == student.UId && e.Class == num);
                if (studentEnrollment != null)
                {
                    studentEnrollment.Grade = updatedLetterGrade;
                }
            }


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
            var submissions = from courses in _db.Courses.Where(c =>
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
                              join sub in _db.Submissions
                                 on courses_classes_aCateg_asg_.AId equals sub.Assignment into courses_classes_aCateg_asg_sub
                              from courses_classes_aCateg_asg_sub_ in courses_classes_aCateg_asg_sub.DefaultIfEmpty()
                              join stud in _db.Students
                                 on courses_classes_aCateg_asg_sub_.Student equals stud.UId into courses_classes_aCateg_asg_sub_stud
                              from courses_classes_aCateg_asg_sbu_stud_ in courses_classes_aCateg_asg_sub_stud.DefaultIfEmpty()
                              select new
                              {
                                  fname = courses_classes_aCateg_asg_sbu_stud_.Fname,
                                  lname = courses_classes_aCateg_asg_sbu_stud_.Lname,
                                  uid = courses_classes_aCateg_asg_sbu_stud_.UId,
                                  time = courses_classes_aCateg_asg_sub_.Time,
                                  score = courses_classes_aCateg_asg_sub_.Score
                              };
            return Json(submissions.ToArray());
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
            // Find the submission
            var submission = (from courses in _db.Courses.Where(c =>
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
                              join sub in _db.Submissions.Where(s => s.Student == uid)
                                 on courses_classes_aCateg_asg_.AId equals sub.Assignment into courses_classes_aCateg_asg_sub
                              from courses_classes_aCateg_asg_sub_ in courses_classes_aCateg_asg_sub.DefaultIfEmpty()
                              select courses_classes_aCateg_asg_sub_).SingleOrDefault();
            if (submission == null)
            {
                return Json(new { success = false });
            }

            // Change the submission's score in the database
            submission.Score = Convert.ToUInt32(score);
            string updatedLetterGrade = CalculateStudentGrade(uid);
            var enrollment = (from e in _db.Enrolleds
                             join cl in _db.Classes on e.Class equals cl.ClassId
                             join co in _db.Courses on cl.CId equals co.CId
                             where co.Subject == subject && co.CNum == num &&
                                cl.Season == season && cl.Year == year &&
                                e.Student == uid
                             select e).SingleOrDefault();
            if (enrollment != null)
            {
                enrollment.Grade = updatedLetterGrade;
            }
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
            var myClasses = from cl in _db.Classes
                            join co in _db.Courses on cl.CId equals co.CId
                            where cl.TaughtBy == uid
                            select new
                            {
                                subject = co.Subject,
                                number = co.CNum,
                                name = co.Name,
                                season = cl.Season,
                                year = cl.Year
                            };
            return Json(myClasses.ToArray());
        }

        /// <summary>
        /// Helpder method to calculate the grade.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="classId"></param>
        private string CalculateStudentGrade(string studentId)
        {
            // Get all of the assginment categories for that class
            var assignmentCategories = (from ac in _db.AssignmentCategories
                                       join cl in _db.Classes on ac.ClassId equals cl.ClassId
                                       select ac).ToArray();

            // Calculate grading weights for each assignment category
            double totalScaledScore = 0;
            double totalCategoryWeights = 0;
            foreach (var category in assignmentCategories)
            {
                var assignments = (from assgn in _db.Assignments
                                  where assgn.CategId == category.CategId
                                  select assgn).ToArray();

                if (assignments.Length > 0)
                {
                    double totalPointsEarned = 0;
                    double totalMaxPoints = 0;

                    foreach (var assignment in assignments)
                    {
                        var submission = (from s in _db.Submissions
                                         where s.Assignment == assignment.AId
                                         select s).SingleOrDefault();
                        totalPointsEarned += submission?.Score ?? 0;
                        totalMaxPoints += (double)(assignment.MaxPoints ?? 0);
                    }

                    double categoryPercentage = totalPointsEarned / totalMaxPoints;
                    double scaledCategoryTotal = (categoryPercentage * (category.GradingWeight ?? 0));
                    totalScaledScore += scaledCategoryTotal;
                    totalCategoryWeights += (double)(category.GradingWeight ?? 0);
                }
            }

            double scalingFactor = 100 / totalCategoryWeights;
            double finalPercentage = totalScaledScore * scalingFactor;

            // Convert the final percentage to a letter grade based on the scale found in your class syllabus
            string letterGrade = ConvertPercentageToLetterGrade(finalPercentage);
            return letterGrade;
        }

        /// <summary>
        /// Helpder method to give a Letter grade for grade percentage. 
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private string ConvertPercentageToLetterGrade(double percentage)
        {
            if (percentage >= 93)
            {
                return "A";
            }
            else if (percentage >= 90)
            {
                return "A-";
            }
            else if (percentage >= 87)
            {
                return "B+";
            }
            else if (percentage >= 83)
            {
                return "B";
            }
            else if (percentage >= 80)
            {
                return "B-";
            }
            else if (percentage >= 77)
            {
                return "C+";
            }
            else if (percentage >= 73)
            {
                return "C";
            }
            else if (percentage >= 70)
            {
                return "C-";
            }
            else if (percentage >= 67)
            {
                return "D+";
            }
            else if (percentage >= 63)
            {
                return "D";
            }
            else if (percentage >= 60)
            {
                return "D-";
            }
            else
            {
                return "E";
            }
        }


        /*******End code to modify********/
    }
}

