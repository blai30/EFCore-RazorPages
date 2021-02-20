using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Pages.Students
{
    /// <summary>
    /// For example, the .NET Framework implementation of Contains performs a case-sensitive
    /// comparison by default. In SQL Server, Contains case-sensitivity is determined by the
    /// collation setting of the SQL Server instance. SQL Server defaults to case-insensitive.
    /// SQLite defaults to case-sensitive. ToUpper could be called to make the test
    /// explicitly case-insensitive:
    ///
    /// When Contains is called on an IEnumerable collection, the .NET Core implementation is used.
    /// When Contains is called on an IQueryable object, the database implementation is used.
    ///
    /// Calling Contains on an IQueryable is usually preferable for performance reasons.
    /// With IQueryable, the filtering is done by the database server.
    /// If an IEnumerable is created first, all the rows have to be returned
    /// from the database server.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;

        public IndexModel(SchoolContext context)
        {
            _context = context;
        }

        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Student> Students { get;set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Student> students = _context.Students.Select(s => s);

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s =>
                    s.LastName.ToUpper().Contains(searchString.ToUpper()) ||
                    s.FirstMidName.ToUpper().Contains(searchString.ToUpper()));
            }

            students = sortOrder switch
            {
                "name_desc" => students.OrderByDescending(s => s.LastName),
                "Date" => students.OrderBy(s => s.EnrollmentDate),
                "date_desc" => students.OrderByDescending(s => s.EnrollmentDate),
                _ => students.OrderBy(s => s.LastName)
            };

            int pageSize = 3;
            Students = await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageIndex ?? 1, pageSize);
        }
    }
}
