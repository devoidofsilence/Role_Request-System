using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text;

namespace RoleRequest.Filters
{
    public class ValidateAjaxAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                    return;

                var modelState = filterContext.Controller.ViewData.ModelState;
                if (!modelState.IsValid)
                {
                    var errorModel =
                            from x in modelState.Keys
                            where modelState[x].Errors.Count > 0
                            select new
                            {
                                key = x,
                                errors = modelState[x].Errors.
                                                       Select(y => y.ErrorMessage).
                                                       ToArray()
                            };
                    filterContext.Result = new JsonResult()
                    {
                        Data = errorModel
                    };
                    filterContext.HttpContext.Response.StatusCode =
                                                          (int)HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append("4. " + ex.Message);
                // flush every 20 seconds as you do it
                System.IO.File.AppendAllText(HttpContext.Current.Server.MapPath("/log.txt"), sb.ToString());
                sb.Clear();
            }
        }
    }
}