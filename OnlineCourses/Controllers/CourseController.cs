using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Models.Types;
using OnlineCourses.Models.ViewsModels;
using OnlineCourses.Servers.Interfaces;
using OnlineCourses.Services.Classes;
using OnlineCourses.Services.Interfaces;

namespace OnlineCourses.Controllers
{
    public class CourseController : Controller
    {
        // TODO [архитектура][1] лучшие практики по архитектуре (например чистая архитектура и др)
        // предполагают то, что приложение делится на слои, и каждый слой отвечает за свою часть приложения
        // контроллеры нужны только для обработки запросов и передачи данных между слоями
        // и не должны содержать бизнес-логику. Вобще почти всё,
        // кроме проверки ролей и валидации входящих данных - это бизнес-логика (работа с бд, агрегация данных)
        // это во всех контроллерах сейчас встречается
        ApplicationContext db;
        ITokenService tokenService;
        ICourseService courseService;
        IAuthentificationService authService;
        public CourseController(ApplicationContext context, ITokenService token, ICourseService course, IAuthentificationService AuthService) 
        {
            db = context;
            tokenService = token;
            courseService = course;
            authService = AuthService;
        }

        [HttpGet]
        async public Task<IActionResult> Main(int page = 1)
        {
            int pageSize = 10;
            var source = db.Courses;

            var count = await source.CountAsync();
            var courses = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync<Course>();
            
            ViewBag.Teachers = await db.Users.Where(s => s.Role == UsersRoles.Teacher).ToListAsync<User>();
            IndexViewModel viewPage = new IndexViewModel(courses, new PageViewModel(count, page, pageSize));

            return View("Main", viewPage);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] CourseIdDTO id)
        {
            if (authService.CheckTeacher(HttpContext.Request.Cookies[AuthService.cookie]!.ToString()))
            {
                if (!courseService.IsLearning(id.CourseId))
                {
                    db.Courses.Where(c => c.Id == id.CourseId).ExecuteDelete();
                    await db.SaveChangesAsync();
                    return StatusCode(200);
                }
            }
            return StatusCode(403);
        }

        [HttpGet]
        public ActionResult Edit([FromQuery] int id)
        {
            // TODO [удобство\DRY принцип][1\2] заметил, что часто идёт работа с юзером 
            // обычно такое оборачивают в отдельный декоратор
            // (в c# это Model Binder, у чата спрашивал здесь https://chat.deepseek.com/a/chat/s/b1565521-8b17-4c98-b129-9e2a1bf4d058)
            // получится один новый класс для того, чтобы достать юзера из httpContext'а 
            // и получать его по аналогии с [FromQuery] int id, что-то вроде
            /*
             * Edit([FromQuery] int id,[FromUser] User user)
             * {
             *   var course = courseService.GetCourse(id, user.Id);
             *   ....
             * }
             */
            int userId = int.Parse(tokenService.GetClaim(HttpContext.Request.Cookies[AuthService.cookie]!.ToString(), ClaimsTypes.ID));
            var course = courseService.GetCourse(id, userId);
            if (course == null)
            {
                return StatusCode(404);
            }
            return View(course);
        }

        [HttpPut]
        public ActionResult EditCourse([FromBody] CourseDTO course)
        {
            if (courseService.Edit(course))
            {
                return StatusCode(200);
            }
            return StatusCode(404);
        }

        [HttpPut]
        public ActionResult EditModule([FromBody] ModuleDTO module)
        {
            if (courseService.EditModule(module))
            {
                return StatusCode(200);
            }
            return StatusCode(404);
        }

        [HttpGet]
        public ActionResult Course([FromQuery] int id)
        {
            int userId = int.Parse(tokenService.GetClaim(HttpContext.Request.Cookies[AuthService.cookie]!.ToString(), ClaimsTypes.ID));
            var course = courseService.GetCourse(id, userId);

            if (course == null)
            {
                return StatusCode(400);
            }

            ViewBag.Purchased = courseService.CheckCourse(id, userId);
            if (userId == course.TeacherId)
            {
                ViewBag.IsCreator = true;
            }
            else
            {
                ViewBag.IsCreator = false;
            }
            return View(course);
        }

        [HttpPost]
        public ActionResult Purchase([FromBody] CourseIdDTO Id)
        {
            int studentId = int.Parse(tokenService.GetClaim(HttpContext.Request.Cookies[AuthService.cookie]!.ToString(), ClaimsTypes.ID));
            if (courseService.Purchase(Id.CourseId, studentId))
            {
                return StatusCode(200);
            }
            else
            {
                return Content("Вы уже записаны");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteModule([FromBody] ModuleDTO id)
        {
            if (authService.CheckTeacher(HttpContext.Request.Cookies[AuthService.cookie]!.ToString()))
            {
                if (!courseService.IsLearning(id.Id))
                {
                    db.Modules.Where(m => m.Id == id.Id).ExecuteDelete();
                    await db.SaveChangesAsync();
                    return StatusCode(200);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public ActionResult CreateModule([FromBody] Module module)
        {
            Module newModule = new Module { Title = module.Title, CourseId = module.CourseId };
            db.Modules.Add(newModule);
            db.SaveChanges();
            return StatusCode(200);
        }

        [HttpGet]
        public ActionResult Module([FromQuery] int idCourse, int idModule)
        {
            List<Lesson> lessons = db.Lessons.Where(l => l.ModuleId == idModule).ToList();
            ViewBag.Course = idCourse;
            ViewBag.Module = db.Modules.FirstOrDefault(m => m.Id == idModule);
            return View(lessons);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLesson([FromBody] LessonIDDTO id)
        {
            if (authService.CheckTeacher(HttpContext.Request.Cookies[AuthService.cookie]!.ToString()))
            {
                if (!courseService.IsLearning(id.LessonId))
                {
                    db.Modules.Where(m => m.Id == id.LessonId).ExecuteDelete();
                    await db.SaveChangesAsync();
                    return StatusCode(200);
                }
            }
            return NotFound();
        }

        [HttpPut]
        public ActionResult EditLesson([FromBody] LessonIDDTO lesson)
        {
            if (courseService.EditLesson(lesson))
            {
                return StatusCode(200);
            }
            return StatusCode(404);
        }

        [HttpGet]
        public ActionResult CreatedCourses(int page = 1)
        {
            if (authService.CheckTeacher(HttpContext.Request.Cookies[AuthService.cookie]!.ToString()))
            {
                int id = int.Parse(tokenService.GetClaim(Request.Cookies[AuthService.cookie]!, ClaimsTypes.ID));
                int pageSize = 10;
                var source = courseService.CreatedCourses(id);

                if (source != null)
                {
                    var count = source.Count();
                    var courses = source.Skip((page - 1) * pageSize).Take(pageSize).ToList<Course>();

                    IndexViewModel viewPage = new IndexViewModel(courses, new PageViewModel(count, page, pageSize));
                    return View(viewPage);
                }
                else
                {
                    return View(null);
                }

            }
            return StatusCode(404);
        }

        [HttpPost]
        public ActionResult Create([FromBody] CourseDTO course)
        {
            Course newCourse = courseService.Create(course.Title ?? "" , course.Description ?? "", int.Parse(tokenService.GetClaim(Request.Cookies[AuthService.cookie]!, ClaimsTypes.ID)));
            return Ok(new { id = newCourse.Id });
        }

        [HttpGet]
        public ActionResult Lesson([FromQuery] int id)
        {
            int userId = int.Parse(tokenService.GetClaim(HttpContext.Request.Cookies[AuthService.cookie]!.ToString(), ClaimsTypes.ID));

            LessonViewModel lesson = courseService.GetLesson(id, userId);
            if (lesson.Id <= 0)
            {
                return StatusCode(404);
            }
            bool userCreator = false;
            if (userId == lesson.course!.TeacherId)
            {
                userCreator = true;
            }
            ViewBag.IsCreator = userCreator;

            return View(lesson);
        }

        [HttpPost]
        public ActionResult CreateLesson([FromBody] LessonDTO lesson)
        {
            Lesson newLesson = courseService.AddLesson(lesson);
            return Ok();
        }

        [HttpGet]
        public ActionResult Lessons([FromQuery] int id)
        {
            ModuleViewModel module = courseService.GetModule(id);
            if (module == null)
            {
                return StatusCode(404);
            }
            return View("EditModule", module);
        }

        [HttpPost]
        public ActionResult FinalLesson([FromBody] LessonIDDTO lesson)
        {
            int userId = int.Parse(tokenService.GetClaim(HttpContext.Request.Cookies[AuthService.cookie]!.ToString(), ClaimsTypes.ID));
            bool got = courseService.FinalLesson(lesson.LessonId, userId);
            if (!got)
            {
                return StatusCode(404);
            }
            else
            {
                return StatusCode(200);
            }
        }
    }
}
