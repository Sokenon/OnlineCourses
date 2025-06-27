using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Models.ViewsModels;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OnlineCourses.Services.Classes
{
    public class CourseService : ICourseService
    {
        ApplicationContext db;
        ITokenService tokenService;

        public CourseService(ApplicationContext DB, ITokenService token) 
        {
            db = DB;
            tokenService = token;
        }
        public List<Course> StudentsCourses(int idStudent)
        {
            List <Course> result = db.User_Courses.Where(uc => uc.StudentId == idStudent).Join(db.Courses, uc => uc.CourseId, c => c.Id, (uc, c) => new { c.Id, c.Title, c.Description, c.TeacherId }).Select(x => new Course { Id = x.Id, Title = x.Title, Description = x.Description, TeacherId = x.TeacherId}).ToList<Course>();
            if (result.Count() != 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        public List<User_Course> UserCorses (int id)
        {
            return db.User_Courses.Where(s => s.StudentId == id).ToList();
        }
        public List<Course> CreatedCourses(int idTeacher)
        {
            List<Course> courses = db.Courses.Where(c => c.TeacherId == idTeacher).ToList<Course>();
            return courses;
        }
        public CourseViewModel GetCourse(int id, int userId)
        {
            CourseViewModel course = new CourseViewModel();

            var result = db.Courses
                .Where(course => course.Id == id)
                .GroupJoin(db.Modules,
                    course => course.Id,
                    module => module.CourseId,
                    (course, modules) => new { Course = course, Modules = modules })
                .SelectMany(
                    x => x.Modules.DefaultIfEmpty(), // LEFT JOIN
                    (course, module) => new { course.Course, Module = module })
                .GroupJoin(db.Lessons,
                    x => x.Module != null ? x.Module.Id : (int?)null,
                    lesson => lesson.ModuleId,
                    (x, lessons) => new { x.Course, x.Module, Lessons = lessons })
                .SelectMany(
                    x => x.Lessons.DefaultIfEmpty(), // LEFT JOIN
                    (x, lesson) => new
                    {
                        CourseId = x.Course.Id,
                        CourseTitle = x.Course.Title,
                        CourseDescription = x.Course.Description,
                        x.Course.TeacherId,
                        ModuleId = x.Module != null ? x.Module.Id : (int?)null,
                        ModuleTitle = x.Module != null ? x.Module.Title : null,
                        LessonTitle = lesson != null ? lesson.Title : null,
                        LessonId = lesson != null ? lesson.Id : (int?)null,
                        Completed = lesson != null && db.User_Lessons
                            .Any(ul => ul.LessonId == lesson.Id && ul.StudentId == userId)
                    })
                .OrderBy(end => end.ModuleId).ToList();
            if (result.Count <= 0)
            {
                return course;
            }
            course.Id = id;
            course.Title = result[0].CourseTitle;
            course.Description = result[0].CourseDescription;
            course.TeacherId = result[0].TeacherId;
            course.Modules = new List<ModuleViewModel>();
            if (result[0].ModuleTitle != "")
            {
                for (int i = 0; i < result.Count(); i++)
                {
                    if (course.Modules.Count() == 0 || !course.Modules.Any(m => m.Title == result[i].ModuleTitle))
                    {
                        course.Modules!.Add(new ModuleViewModel() { Title = result[i].ModuleTitle, Lessons = new List<LessonViewModel>(), Id = result[i].ModuleId });
                        course.Modules.FirstOrDefault(m => m.Title == result[i].ModuleTitle)!.Lessons!.Add(new LessonViewModel() { Id = result[i].LessonId, Title = result[i].LessonTitle, Completed = result[i].Completed });
                    }
                    else if (course.Modules.Any(m => m.Title == result[i].ModuleTitle))
                    {
                        course.Modules.FirstOrDefault(m => m.Title == result[i].ModuleTitle)!.Lessons!.Add(new LessonViewModel() { Id = result[i].LessonId, Title = result[i].LessonTitle, Completed = result[i].Completed });
                    }
                }
                course.Complited = IsCompleted(course.Modules!);
                foreach (var module in course.Modules)
                {
                    module.Lessons = module.Lessons!.OrderBy(l => l.Id).ToList();
                }   
            }
            else
            {
                course.Complited = false;
            }
            return course;
        }
        public bool CheckCourse(int CourseId, int StudentId)
        {
            List<User_Course> whqat = db.User_Courses.Where(uc => uc.CourseId == CourseId && uc.StudentId == StudentId).ToList();
            bool check = whqat.Count > 0 ? true : false;
            return check;
        }
        public bool IsCompleted(List<ModuleViewModel> modules)
        {
            if (modules.Count > 0)
            {
                return modules.All(ls => ls.Lessons!.All(l => l.Completed == true));
            }
            else
            {
                return false;
            }
        }
        public bool Purchase(int courseId, int studentId)
        {
            if (CheckCourse(courseId, studentId))
            {
                return false;
            }
            else
            {
                User_Course uc = new User_Course { StudentId = studentId, CourseId = courseId, Completed = false };
                db.Add(uc);
                db.SaveChanges();
                return true;
            }
        }
        public bool Edit(CourseDTO course)
        {
            var updated = db.Courses.Where(c => c.Id == course.Id).ExecuteUpdate(p => p.SetProperty(c => c.Title, c => course.Title ?? c.Title).SetProperty(c => c.Description, c => course.Description ?? c.Description));
            return updated > 0;
        }
        public bool EditModule(ModuleDTO module)
        {
            if (module.Title != null)
            {
                db.Modules.Where(m => m.Id == module.Id).ExecuteUpdate(p => p.SetProperty(m => m.Title, module.Title));
                db.SaveChanges();
                return true;
            }
            return false;
        }
        public bool IsLearning(int id)
        {
            if (db.User_Courses.Where(uc => uc.CourseId == id).Count() > 0)
            {
                return true;
            }
            return false;
        }
        public bool EditLesson(LessonIDDTO lesson)
        {
            if (lesson.Title != null && lesson.Content != null)
            {
                db.Lessons.Where(l => l.Id == lesson.LessonId).ExecuteUpdate(p => p.SetProperty(l => l.Title, lesson.Title).SetProperty(l => l.Content, lesson.Content));
                db.SaveChanges();
                return true;
            }
            else
            {
                if (lesson.Title != null)
                {
                    db.Lessons.Where(l => l.Id == lesson.LessonId).ExecuteUpdate(p => p.SetProperty(l => l.Title, lesson.Title));
                    db.SaveChanges();
                    return true;
                }
                if (lesson.Content != null)
                {
                    db.Lessons.Where(l => l.Id == lesson.LessonId).ExecuteUpdate(p => p.SetProperty(l => l.Content, lesson.Content));
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public Course Create(string title, string description, int teacherId)
        {
            Course newCourse = new Course { Title = title, Description = description, TeacherId = teacherId };
            db.Courses.AddAsync(newCourse);
            db.SaveChanges();
            return newCourse;
        }
        public LessonViewModel GetLesson(int id, int studentId)
        {
            LessonViewModel lesson = new LessonViewModel { module = new Module(), course = new Course() };

            var result = db.Lessons.Where(l => l.Id == id).Join(db.Modules, l => l.ModuleId, module => module.Id, (l, module) => new { Lesson = l, Module = module }).Join(db.Courses, combined => combined.Module.CourseId, course => course.Id, (combined, course) => new { combined.Lesson.Id, combined.Lesson.Title, combined.Lesson.Content, ModuleId = combined.Module.Id, ModuleTitle = combined.Module.Title, CourseId = course.Id, CourseTitle = course.Title, course.TeacherId, Completed = db.User_Lessons.Any(us => us.LessonId == combined.Lesson.Id && us.StudentId == studentId) ? db.User_Lessons.FirstOrDefault(us => us.LessonId == combined.Lesson.Id && us.StudentId == studentId)!.Completed : false }).ToList();

            if (result.Count <= 0)
            {
                return lesson;
            }

            lesson.Id = result[0].Id;
            lesson.Title = result[0].Title;
            lesson.Content = result[0].Content;
            lesson.Completed = result[0].Completed;
            lesson.module.Id = result[0].ModuleId;
            lesson.module.Title = result[0].ModuleTitle;
            lesson.course.Id = result[0].CourseId;
            lesson.course.Title = result[0].CourseTitle;
            lesson.course.TeacherId = result[0].TeacherId;

            return lesson;
        }
        public Lesson AddLesson(LessonDTO lesson)
        {
            Lesson newLesson = new Lesson { Title = lesson.Title, Content = lesson.Content, ModuleId = lesson.ModuleId };

            db.Lessons.AddAsync(newLesson);
            db.SaveChanges();

            return newLesson;
        }
        public ModuleViewModel GetModule(int id)
        {   
            var result = db.Lessons.Where(l => l.ModuleId == id).Join(db.Modules, l => l.ModuleId, module => module.Id, (l, module) => new { Lesson = l, Module = module }).Join(db.Courses, combined => combined.Module.CourseId, course => course.Id, (combined, course) => new { combined.Lesson.Id, combined.Lesson.Title, combined.Lesson.Content, ModuleId = combined.Module.Id, ModuleTitle = combined.Module.Title, CourseId = course.Id, CourseTitle = course.Title, CourseCreator = course.TeacherId }).OrderBy(end => end.Id).ToList();
            if (result.Count <= 0)
            {
                var resultForNull = db.Modules.Where(m => m.Id == id).Join(db.Courses, m => m.CourseId, course => course.Id, (m, course) => new { ModuleId = m.Id, ModuleTitle = m.Title, CourseId = course.Id, CourseTitle = course.Title, CourseCreator = course.TeacherId }).ToList();
                return new ModuleViewModel { Id = resultForNull[0].ModuleId, Title = resultForNull[0].ModuleTitle, CourseId = resultForNull[0].CourseId, CourseTitle = resultForNull[0].CourseTitle, CreatorId = resultForNull[0].CourseCreator };
            }
            List<LessonViewModel> lessons = new List<LessonViewModel>();
            for (int i = 0; i < result.Count(); i++)
            {
                lessons.Add(new LessonViewModel() { Id = result[i].Id, Title = result[i].Title, Content = result[i].Content });
            }
            ModuleViewModel module = new ModuleViewModel { Id = id, Title = result[0].ModuleTitle, CourseId = result[0].CourseId, CourseTitle = result[0].CourseTitle, CreatorId = result[0].CourseCreator, Lessons = lessons };
            return module;
        }
        public bool FinalLesson(int id, int idStudent)
        {
            if (db.User_Lessons.Where(ul => ul.LessonId == id && ul.StudentId == idStudent).Count() > 0)
            {
                db.User_Lessons.Where(ul => ul.LessonId == id && ul.StudentId == idStudent).ExecuteUpdate(p => p.SetProperty(l => l.Completed, true));
                db.SaveChanges();
            }
            else
            {
                User_Lesson ul = new User_Lesson { LessonId = id, StudentId = idStudent, Completed = true };
                db.User_Lessons.AddAsync(ul);
                db.SaveChanges();
            }
            return true; 
        }


    }
}
