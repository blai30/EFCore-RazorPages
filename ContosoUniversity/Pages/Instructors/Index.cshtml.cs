using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models.SchoolViewModels;

namespace ContosoUniversity.Pages.Instructors
{
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;

        public IndexModel(SchoolContext context)
        {
            _context = context;
        }

        public InstructorIndexData InstructorData { get; set; }
        public int InstructorId { get; set; }
        public int CourseId { get; set; }

        public async Task OnGetAsync(int? id, int? courseId)
        {
            InstructorData = new InstructorIndexData
            {
                Instructors = await _context.Instructors
                    .Include(i => i.OfficeAssignment)
                    .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                    .ThenInclude(i => i.Department)
                    // .Include(i => i.CourseAssignments)
                    // .ThenInclude(i => i.Course)
                    // .ThenInclude(i => i.Enrollments)
                    // .ThenInclude(i => i.Student)
                    // .AsNoTracking()
                    .OrderBy(i => i.LastName)
                    .ToListAsync()
            };

            if (id != null)
            {
                InstructorId = id.Value;
                var instructor = InstructorData.Instructors
                    .Single(i => i.Id == id.Value);

                InstructorData.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            if (courseId != null)
            {
                CourseId = courseId.Value;
                var selectedCourse = InstructorData.Courses
                    .Single(x => x.CourseId == courseId);

                await _context.Entry(selectedCourse)
                    .Collection(x => x.Enrollments)
                    .LoadAsync();

                foreach (var enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment)
                        .Reference(x => x.Student)
                        .LoadAsync();
                }

                InstructorData.Enrollments = selectedCourse.Enrollments;
            }
        }
    }
}
