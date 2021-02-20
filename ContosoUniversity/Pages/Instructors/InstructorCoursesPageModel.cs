using System.Collections.Generic;
using System.Linq;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Instructors
{
    public class InstructorCoursesPageModel : PageModel
    {
        public List<AssignedCourseData> AssignedCourseDataList;

        public void PopulateAssignedCourseData(
            SchoolContext context,
            Instructor instructor)
        {
            var allCourses = context.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments
                .Select(c => c.CourseId));

            AssignedCourseDataList = new List<AssignedCourseData>();

            foreach (var course in allCourses)
            {
                AssignedCourseDataList.Add(new AssignedCourseData
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseId)
                });
            }
        }

        public void UpdateInstructorCourses(
            SchoolContext context,
            string[] selectedCourses,
            Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHs = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseAssignments
                .Select(c => c.Course.CourseId));

            foreach (var course in context.Courses)
            {
                if (selectedCoursesHs.Contains(course.CourseId.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseId))
                    {
                        instructorToUpdate.CourseAssignments.Add(
                            new CourseAssignment
                            {
                                InstructorId = instructorToUpdate.Id,
                                CourseId = course.CourseId
                            });
                    }
                }
                else
                {
                    if (!instructorCourses.Contains(course.CourseId)) continue;

                    var courseToRemove = instructorToUpdate
                        .CourseAssignments
                        .SingleOrDefault(i => i.CourseId == course.CourseId);

                    context.Remove(courseToRemove);
                }
            }
        }
    }
}
