using OnlineCourses.Models;
using OnlineCourses.Models.DTO;
using OnlineCourses.Models.ViewsModels;

namespace OnlineCourses.Services.Interfaces
{
    public interface ICourseService
    {
        public List<Course> StudentsCourses(int idStudent);
        public List<Course> CreatedCourses(int idTeacher);
        public CourseViewModel GetCourse(int id, int idStudent);
        public List<User_Course> UserCorses(int id);
        public bool CheckCourse (int CourseId, int StudentId);
        public bool IsCompleted(List<ModuleViewModel> modules);
        public bool Purchase(int CourseId, int StudentId);
        public bool Edit(CourseDTO course);
        public bool EditModule(ModuleDTO module);
        public bool IsLearning(int id);
        public bool EditLesson(LessonIDDTO lesson);
        public Course Create(string title, string descrption, int teacherId);
        public LessonViewModel GetLesson(int id, int studentID);
        public Lesson AddLesson(LessonDTO lesson);
        public ModuleViewModel GetModule(int id);
        public bool FinalLesson(int id, int idStudent);

    }
}
