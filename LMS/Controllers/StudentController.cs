using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Class
                        join co in db.Course on c.CID equals co.CID
                        join e in db.Enrolled on c.CID equals e.CID
                        where e.UId == uid
                        select new
                        {
                            subject = co.Department,
                            number = co.CourseNum,
                            name = co.Name,
                            season = c.Semester.Substring(0, c.Semester.Length - 4),
                            year = Int32.Parse(c.Semester.Substring(c.Semester.Length - 4)),
                            grade = e.Grade ?? "--"
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from a in db.Assignment
                        join c in db.Category on a.Category equals c.Name
                        join cl in db.Class on c.CID equals cl.CID
                        join co in db.Course on cl.CID equals co.CID
                        join s in db.Submission on a.AID equals s.AID into ps
                        from sub in ps.DefaultIfEmpty()
                        where co.Department == subject && co.CourseNum == num && cl.Semester == season + year.ToString() && sub.UId == uid
                        select new
                        {
                            aname = a.Name,
                            cname = c.Name,
                            due = a.Due,
                            score = sub == null ? (int?)null : sub.Score
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = from a in db.Assignment
                        join c in db.Category on a.Category equals c.Name
                        join cl in db.Class on c.CID equals cl.CID
                        join co in db.Course on cl.CID equals co.CID
                        where co.Department == subject && co.CourseNum == num && cl.Semester == season + year.ToString() && a.Name == asgname && c.Name == category
                        select a.AID;

            int aid = query.Single();

            var submission = db.Submission.SingleOrDefault(s => s.AID == aid && s.UId == uid);
            if (submission == null)
            {
                submission = new Submission
                {
                    AID = aid,
                    UId = uid,
                    Time = DateTime.Now,
                    Score = 0,
                    Contents = contents
                };
                db.Submission.Add(submission);
            }
            else
            {
                submission.Time = DateTime.Now;
                submission.Contents = contents;
            }

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from cl in db.Class
                        join co in db.Course on cl.CID equals co.CID
                        where co.Department == subject && co.CourseNum == num && cl.Semester == season + year.ToString()
                        select cl.CID;

            int cid = query.Single();

            var enrollment = db.Enrolled.SingleOrDefault(e => e.CID == cid && e.UId == uid);
            if (enrollment == null)
            {
                enrollment = new Enrolled
                {
                    CID = cid,
                    UId = uid,
                    Grade = null
                };
                db.Enrolled.Add(enrollment);

                try
                {
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }
            }
            else
            {
                return Json(new { success = false });
            }
        }




        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from e in db.Enrolled
                        where e.UId == uid && e.Grade != null
                        select e.Grade;

            var grades = query.ToArray();
            double totalPoints = 0;
            int count = 0;

            foreach (var grade in grades)
            {
                if (grade == "A")
                    totalPoints += 4;
                else if (grade == "A-")
                    totalPoints += 3.7;
                else if (grade == "B+")
                    totalPoints += 3.3;
                else if (grade == "B")
                    totalPoints += 3;
                else if (grade == "B-")
                    totalPoints += 2.7;
                else if (grade == "C+")
                    totalPoints += 2.3;
                else if (grade == "C")
                    totalPoints += 2;
                else if (grade == "C-")
                    totalPoints += 1.7;
                else if (grade == "D+")
                    totalPoints += 1.3;
                else if (grade == "D")
                    totalPoints += 1;
                else if (grade == "D-")
                    totalPoints += 0.7;
                else if (grade == "E")
                    totalPoints += 0;
                count++;
            }

            double gpa = count == 0 ? 0 : totalPoints / count;
            return Json(new { gpa });
        }

        /*******End code to modify********/

    }
}

