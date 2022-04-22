using DDAC_Assignment.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class Navigation
    {
        public static List<string> getNavigationItem(IServiceProvider serviceProvider)
        {
            List<string> navigationList = new List<string>();
            using (var context = new DDAC_AssignmentNewsDatabase(
            serviceProvider.GetRequiredService<DbContextOptions<DDAC_AssignmentNewsDatabase>>()))
            {
                if (!context.Category.Any())
                {
                    return new List<string>();
                }

                if (context.Category.Any())
                {
                    foreach (var item in context.Category)
                    {
                        if (item.ParentCategory.Equals("None"))
                        {
                            navigationList.Add(item.CategoryName);
                        }
                    }
                }
                return navigationList;
            }
        }
    }
}
