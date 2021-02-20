using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Pages.Courses
{
    public class CreateModel : DepartmentNamePageModel
    {
        private readonly SchoolContext _context;

        public CreateModel(SchoolContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            PopulateDepartmentsDropdownList(_context);
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var emptyCourse = new Course();

            if (await TryUpdateModelAsync(
                emptyCourse,
                // Prefix for form value.
                "course",
                course => course.CourseId,
                course => course.DepartmentId,
                course => course.Title,
                course => course.Credits))
            {
                await _context.Courses.AddAsync(emptyCourse);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            // Select DepartmentId if TryUpdateModelAsync fails.
            PopulateDepartmentsDropdownList(_context, emptyCourse.DepartmentId);
            return Page();
        }
    }
}
