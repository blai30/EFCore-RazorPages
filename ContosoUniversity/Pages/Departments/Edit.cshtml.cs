using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments
{
    public class EditModel : PageModel
    {
        private readonly SchoolContext _context;

        public EditModel(SchoolContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Department Department { get; set; }

        // Replace ViewData["InstructorId"]
        public SelectList InstructorNameSl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Department = await _context.Departments
                .Include(d => d.Administrator) // eager loading
                .AsNoTracking() // tracking not required
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (Department == null)
            {
                return NotFound();
            }

            // Use strongly typed data rather than ViewData.
            InstructorNameSl = new SelectList(
                _context.Instructors,
                "Id",
                "FirstMidName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var departmentToUpdate = await _context.Departments
                .Include(i => i.Administrator)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (departmentToUpdate == null)
            {
                return HandleDeletedDepartment();
            }

            _context.Entry(departmentToUpdate)
                .Property("RowVersion").OriginalValue = Department.RowVersion;

            if (await TryUpdateModelAsync(
                departmentToUpdate,
                "Department",
                department => department.Name,
                department => department.StartDate,
                department => department.Budget,
                department => department.InstructorId))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException e)
                {
                    var exceptionEntry = e.Entries.Single();
                    var clientValues = exceptionEntry.Entity as Department;
                    var databaseEntry = await exceptionEntry.GetDatabaseValuesAsync();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Unable to save. The department was deleted by another user.");
                        return Page();
                    }

                    var dbValues = databaseEntry.ToObject() as Department;
                    await SetDbErrorMessage(dbValues, clientValues, _context);

                    // Save the current RowVersion so next postback
                    // matches unless an new concurrency issue happens.
                    Department.RowVersion = dbValues?.RowVersion;
                    // Clear the model error for the next postback.
                    ModelState.Remove("Department.RowVersion");
                }
            }

            InstructorNameSl = new SelectList(
                _context.Instructors,
                "Id",
                "FullName",
                departmentToUpdate.InstructorId);

            return Page();
        }

        private IActionResult HandleDeletedDepartment()
        {
            var deletedDepartment = new Department();
            // ModelState contains the posted data because of the deletion error
            // and will override the Department instance values when displaying Page().
            ModelState.AddModelError(
                string.Empty,
                "Unable to save. The department was deleted by another user.");

            InstructorNameSl = new SelectList(
                _context.Instructors,
                "Id",
                "FullName",
                Department.InstructorId);

            return Page();
        }

        private async Task SetDbErrorMessage(
            Department dbValues,
            Department clientValues,
            SchoolContext context)
        {
            if (dbValues.Name != clientValues.Name)
            {
                ModelState.AddModelError("Department.Name",
                    $"Current value: {dbValues.Name}");
            }

            if (dbValues.Budget != clientValues.Budget)
            {
                ModelState.AddModelError("Department.Budget",
                    $"Current value: {dbValues.Budget:c}");
            }

            if (dbValues.StartDate != clientValues.StartDate)
            {
                ModelState.AddModelError("Department.StartDate",
                    $"Current value: {dbValues.StartDate:d}");
            }

            if (dbValues.InstructorId != clientValues.InstructorId)
            {
                var dbInstructor = await _context.Instructors
                    .FindAsync(dbValues.InstructorId);
                ModelState.AddModelError("Department.InstructorId",
                    $"Current value: {dbInstructor?.FullName}");
            }

            ModelState.AddModelError(
                string.Empty,
                "The record you attempted to edit "
                + "was modified by another user after you. The "
                + "edit operation was canceled and the current values in the database "
                + "have been displayed. If you still want to edit this record, click "
                + "the Save button again.");
        }
    }
}
