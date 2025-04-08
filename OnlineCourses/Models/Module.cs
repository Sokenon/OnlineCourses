using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCourses.Models

{
    public class Module
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public int CourseId { get; set; }
    }
}
