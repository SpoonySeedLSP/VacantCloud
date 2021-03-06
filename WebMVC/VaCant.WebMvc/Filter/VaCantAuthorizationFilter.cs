using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VaCant.Common;
using VaCant.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace VaCant.WebMvc.Filter
{
    public class VaCantAuthorizationFilter : Attribute, IAuthorizationFilter//, IAsyncAuthorizationFilter
    {
        public ILogger Logger { get; set; }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //权限过滤的两种思路 认证过滤器和方法过滤器都可以
            //一：在每个控制器和action上添加相应的特性标签，把标签名称存到数据库权限表中，获取当前请求控制器下的action的对应的特性标签 与数据库存储的进行对比，相同就是有权限
            //二：把每个控制器和action以 ControllerName_ActionName 的形式存在数据库权限表中，获取当前访问的控制器名称和Action 名称与当前用户的所在角色下的权限进行比对 ，相同就是有权限

            //return;
            //匿名标识 无需验证
            if (context.Filters.Any(e => (e as AllowAnonymous) != null))
            {
                return;
            }
            //和上面一样
            var endpoint = context?.HttpContext?.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                return;
            }
            //不是控制器里的方法 无需验证
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            #region 获取当前用户id和用户名判断是否登录和是否admin管理员

            string userId = context.HttpContext.Session.GetString("LoginUserId");
            string userName = context.HttpContext.Session.GetString("LoginUserName");
            if (string.IsNullOrEmpty(userId))
            {
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    //是ajax请求
                    context.Result = new JsonResult(new { status = "error", message = "你没有登录" });
                }
                else
                {
                    var result = new RedirectResult("~/Account/Login");
                    context.Result = result;
                }
                return;
            }
            //admin管理员不需要权限检查
            if (!string.IsNullOrEmpty(userName) && userName.Equals("admin"))
            {
                return;
            }

            #endregion

            #region 获取访问目标对象上的特性

            //所有目标对象上所有特性Attribute
            //var attrs = context.ActionDescriptor.EndpointMetadata.ToList();
            //获取所有目标对象上所有特性CheckPermissionAttribute
            var attrs = context.ActionDescriptor.EndpointMetadata.ToList().Where(r => r as CheckPermissionAttribute != null).ToList();
            //获取CheckPermissionAttribute最下面的一个
            //var attr = endpoint?.Metadata.GetMetadata<CheckPermissionAttribute>();
            //获取过滤器特性
            //var pAttr = context.Filters.Where(r => r as VaCantAuthorizationFilter != null).ToList();
           

            #endregion

            #region 获取当前访问的区域、控制器和action

            //1. 获取区域、控制器、Action的名称
            ////必须在区域里的控制器上加个特性[Area("")]才能获取
            //string areaName = context.ActionDescriptor.RouteValues["area"] == null ? "" : context.ActionDescriptor.RouteValues["area"].ToString();
            //string controllerName = context.ActionDescriptor.RouteValues["controller"] == null ? "" : context.ActionDescriptor.RouteValues["controller"].ToString();
            //string actionName = context.ActionDescriptor.RouteValues["action"] == null ? "" : context.ActionDescriptor.RouteValues["action"].ToString();

            //获取请求的区域，控制器，action名称
            var area = context.RouteData.DataTokens["area"]?.ToString();
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            #endregion

            #region 当前登入的非admin用户的权限检查
            //获取当前用户对应角色的所有权限
           var permissionNames= CacheCommon.GetCacheByPermissionNames();
            if (permissionNames != null && permissionNames.Count > 0)
            {
                foreach (var item in attrs)
                {
                    if (!permissionNames.Contains(item.ToString()))
                    {
                        if (IsAjaxRequest(context.HttpContext.Request))
                        {
                            //是ajax请求
                            context.Result = new JsonResult(new { status = "error", message = "你没有权限" });
                        }
                        else
                        {
                            var result = new RedirectResult("~/Home/Index");
                            context.Result = result;
                        }
                        return;
                    }
                }
            }
            else
            {
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    //是ajax请求
                    context.Result = new JsonResult(new { status = "error", message = "你没有任何权限" });
                }
                else
                {
                    var result = new RedirectResult("~/Home/Index");
                    context.Result = result;
                }
                return;
            }
            
            #endregion


        }

        /// <summary>
        /// 判断是否是ajax请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool IsAjaxRequest(HttpRequest request)
        {
            string header = request.Headers["X-Requested-With"];
            return "XMLHttpRequest".Equals(header);
        }

        //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        //{

        //    var endpoint = context?.HttpContext?.GetEndpoint();
        //    // Allow Anonymous skips all authorization
        //    if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        //    {
        //        return;
        //    }

        //    if (!context.ActionDescriptor.IsControllerAction())
        //    {
        //        return;
        //    }

        //    //TODO: Avoid using try/catch, use conditional checking
        //    try
        //    {


        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex.ToString(), ex);


        //    }
        //}


    }

    public static class ActionDescriptorExtension
    {
        public static bool IsControllerAction(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor is ControllerActionDescriptor;
        }
    }
}
