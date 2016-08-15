using AppInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace AppInventory.CustomFilters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeRole : AuthorizeAttribute
    {
        public AuthorizeRole()
        {
            View = "AuthorizeFailed";
        }
 
        public string View { get; set; }

        /// <summary>
        /// Check for Authorization
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            IsUserAuthorized(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (String.IsNullOrEmpty(Roles))
                return true;

            // Verify that the user membership in the roles
            if (IsRoleAuthorized(httpContext, SplitString(Roles)))
                return true;

            return false;
        }
 
        /// <summary>
        /// Method to check if the user is Authorized or not
        /// if yes continue to perform the action else redirect to error page
        /// </summary>
        /// <param name="filterContext"></param>
        private void IsUserAuthorized(AuthorizationContext filterContext)
        {
            // If the Result returns null then the user is Authorized 
            if (filterContext.Result == null)
                return;
 
            //If the user is Un-Authorized then Navigate to Auth Failed View 
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                
               // var result = new ViewResult { ViewName = View };
                var vr = new ViewResult();
                vr.ViewName = View;
 
                ViewDataDictionary dict = new ViewDataDictionary();
                dict.Add("Message", "You are not authorized to perform this action.");
 
                vr.ViewData = dict;
 
                var result = vr;
                 
                filterContext.Result = result;
            }
        }

        public static bool IsRoleAuthorized(HttpContextBase httpContext, List<string> roles)
        {
            try
            {
                AppInventoryEntities db = new AppInventoryEntities();
                string loginName = httpContext.User.Identity.Name.ToLower().Replace(@"blackrock\", "");

                string sid = db.UserInfoes.FirstOrDefault(u => u.UserName == loginName).UserID;
                var userMemebrships = db.GroupMembers.Where(g => g.MemberSID == sid).Select(g => g.GroupID);
                var roleMemberships = db.RBACGroups.Where(r => roles.Contains(r.RBACInfo.Name)).Select(g => g.GroupID).Where(g => userMemebrships.Contains(g));

                if (roleMemberships.Count() != 0)
                    return true;
            }
            catch (Exception) { }
            return false;
        }


        internal static List<string> SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new  List<string>() ;
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToList<string>();
        }

    }
}