using DDAC_Assignment.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DDAC_Assignment.Models
{
    public class CategoryDatabase
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //this function: to first load to store the default category data
            using (var context = new DDAC_AssignmentNewsDatabase(
            serviceProvider.GetRequiredService<DbContextOptions<DDAC_AssignmentNewsDatabase>>()))
            {
                if (context.Category.Any())
                {
                    return;
                }

                if (!context.Category.Any())
                {
                    context.Category.AddRange(
                        new Category
                        {
                            CategoryName = "World",
                            ParentCategory = "None",
                            Description = "This category is related to world such US, China etc.",
                        },
                        new Category
                        {
                            CategoryName = "Politics",
                            ParentCategory = "None",
                            Description = "This category is related to local politic (Malaysia).",
                        },
                        new Category
                        {
                            CategoryName = "Business",
                            ParentCategory = "None",
                            Description = "This category is related to business includes economic, markets etc.",
                        },
                        new Category
                        {
                            CategoryName = "Technology",
                            ParentCategory = "None",
                            Description = "This category is related to technology such as mobile, internet etc.",
                        },
                        new Category
                        {
                            CategoryName = "Entertainment",
                            ParentCategory = "None",
                            Description = "This category is related to entertainment includes movides, music etc.",
                        },
                        new Category
                        {
                            CategoryName = "Sport",
                            ParentCategory = "None",
                            Description = "This category is related to Sport like basketball, football, badminton etc.",
                        },
                        new Category
                        {
                            CategoryName = "Science",
                            ParentCategory = "None",
                            Description = "This category is related to science like physics, genetic etc.",
                        },
                        new Category
                        {
                            CategoryName = "Health",
                            ParentCategory = "None",
                            Description = "This category is related to health like medicine, healthcare etc.",
                        }
                    );

                    //after edit, save the changes in the database
                    context.SaveChanges();
                }
            }
        }
    }
}
