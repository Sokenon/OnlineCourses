namespace OnlineCourses.Models.ViewsModels
{
    public class ModuleViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<LessonViewModel>? Lessons { get; set; }
    }
}
