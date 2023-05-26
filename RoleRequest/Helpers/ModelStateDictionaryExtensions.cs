using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Reflection;

namespace RoleRequest.Helpers
{
    /// <summary>
    /// Extensions for the <see cref="ModelStateDictionary"/> class.
    /// </summary>
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Removes the specified member from the  <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="lambdaExpression">The lambda expression.</param>
        public static void Remove<TViewModel>(
            this ModelStateDictionary me,
            Expression<Func<TViewModel, object>> lambdaExpression)
        {
            me.Remove(GetPropertyName(lambdaExpression));
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="lambdaExpression">The lambda expression.</param>
        /// <returns></returns>
        private static string GetPropertyName(this Expression lambdaExpression)
        {
            var e = lambdaExpression;

            while (true)
            {
                switch (e.NodeType)
                {
                    case ExpressionType.Lambda:
                        e = ((LambdaExpression)e).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        var propertyInfo =
                            ((MemberExpression)e).Member
                            as PropertyInfo;

                        return propertyInfo != null
                                   ? propertyInfo.Name
                                   : null;

                    case ExpressionType.Convert:
                        e = ((UnaryExpression)e).Operand;
                        break;

                    default:
                        return null;
                }
            }
        }
    }
}