using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCourses.Models
{
    [Index("StudentId", "LessonId", IsUnique = true, Name = "Index")]
    public class User_Lesson
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public bool Completed { get; set; }
    }
}
