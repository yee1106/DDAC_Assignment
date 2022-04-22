using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    // Define permissions policies
    public static class Permissions
    {
        public static List<string> GeneratePermissionsForModule(string module)
        {
            return new List<string>()
        {
            $"Permissions.{module}.Create",
            $"Permissions.{module}.View",
            $"Permissions.{module}.Edit",
            $"Permissions.{module}.Delete",
        };
        }

        public static class User 
        {
            public const string View = "Permissions.User.View";
            public const string Create = "Permissions.User.Create";
            public const string Edit = "Permissions.User.Edit";
            public const string Delete = "Permissions.User.Delete";
        }

        public static class Category
        {
            public const string View = "Permissions.Category.View";
            public const string Create = "Permissions.Category.Create";
            public const string Edit = "Permissions.Category.Edit";
            public const string Delete = "Permissions.Category.Delete";
        }

        public static class News
        {
            public const string View = "Permissions.News.View";
            public const string Create = "Permissions.News.Create";
            public const string Edit = "Permissions.News.Edit";
            public const string Delete = "Permissions.News.Delete";
        }

        public static class ReadNews
        {
            public const string Read = "Permissions.ReadNews.Read";
        }

        public static class Advertisements
        {
            public const string View = "Permissions.Advertisements.View";
            public const string Create = "Permissions.Advertisements.Create";
            public const string Edit = "Permissions.Advertisements.Edit";
            public const string Delete = "Permissions.Advertisements.Delete";
        }

    }

    public class PermissionViewModel
    {
        public string RoleId { get; set; }
        public IList<RoleClaimsViewModel> RoleClaims { get; set; }
    }

}
