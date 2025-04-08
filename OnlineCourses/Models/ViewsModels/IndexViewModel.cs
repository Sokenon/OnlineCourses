namespace OnlineCourses.Models.ViewsModels
{
    public class IndexViewModel
    {
        public IEnumerable<Course> Courses { get; }
        public PageViewModel PageViewModel { get; }
        public IndexViewModel(IEnumerable<Course> courses, PageViewModel viewModel)
        {
            Courses = courses;
            PageViewModel = viewModel;
        }
    }
}
