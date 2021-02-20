using System.Linq;
using ContosoUniversity.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses
{
    public class DepartmentNamePageModel : PageModel
    {
        public SelectList DepartmentNameSL { get; set; }

        public void PopulateDepartmentsDropdownList(
            SchoolContext context,
            object selectedDepartment = null)
        {
            var departmentsQuery =
                from d in context.Departments
                orderby d.Name
                select d;

            DepartmentNameSL = new SelectList(
                departmentsQuery.AsNoTracking(),
                "DepartmentId",
                "Name",
                selectedDepartment);
        }
    }
}
