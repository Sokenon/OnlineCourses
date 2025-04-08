namespace OnlineCourses.Models.ViewsModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List <ModuleViewModel>? Modules { get; set; }
        public bool Complited { get; set; } = false;
        public int TeacherId { get; set; }
    }
}
