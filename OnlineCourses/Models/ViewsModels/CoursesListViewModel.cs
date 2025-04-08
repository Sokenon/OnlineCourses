using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OnlineCourses.Models.ViewsModels
{
    public class CoursesListViewModel
    {
        public Course Course { get; set; } = new Course();
        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
        public IEnumerable<User> Teachers { get; set; } = new List<User>();
        public IndexViewModel Page { get; set; } = new IndexViewModel(new List<Course>(), new PageViewModel(0, 1, 10));
        public IEnumerable<Module> Modules { get; set; } = new List<Module>();
        public IEnumerable<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
