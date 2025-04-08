using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using System.Xml.Linq;

namespace OnlineCourses.Models
{
    [Index("StudentId", "CourseId", IsUnique = true, Name = "Index")]
    public class User_Course
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public bool Completed { get; set; }
    }
}
