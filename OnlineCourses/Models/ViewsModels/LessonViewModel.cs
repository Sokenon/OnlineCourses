namespace OnlineCourses.Models.ViewsModels
{
    public class LessonViewModel
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool Completed { get; set; } = false;
        public Module? module { get; set; }
        public Course? course { get; set; }
    }
}
