using System;
using System.Collections.Generic;
using System.Security.Claims;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class InternalDataSetActionFilterAttributeFixture
	{
		InternalDataSetActionFilterAttribute internalDataSetActionFilter;

		[SetUp]
		public void SetUp()
		{
			internalDataSetActionFilter = new InternalDataSetActionFilterAttribute();
		}

		[Test]
		public void OnActionExecuting_UserNotAuthenticated_ReturnsUnauthorizedResult()
		{
			var context = CreateActionExecutingContext(isAuthenticated: false);
			internalDataSetActionFilter.OnActionExecuting(context);
			Assert.That(context.Result, Is.InstanceOf<UnauthorizedResult>());
		}

		[Test]
		public void OnActionExecuting_UserIdIsEmpty_ReturnsForbiddenResult()
		{
			var context = CreateActionExecutingContext(isAuthenticated: true, userId: "");
			internalDataSetActionFilter.OnActionExecuting(context);
			Assert.That(context.Result, Is.InstanceOf<StatusCodeResult>());
			Assert.That(((StatusCodeResult)context.Result)?.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
		}

		[Test]
		public void OnResultExecuting_NoResultsReturn()
		{
			var context = CreateResultExecutingContext();
			internalDataSetActionFilter.OnResultExecuting(context);
			Assert.That(context.Result, Is.Null);
		}

		ResultExecutingContext CreateResultExecutingContext(object controller = null)
		{
			var resultContext = new ResultExecutingContext(
				new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
				new List<IFilterMetadata>(),
				null,
				controller
			);
			return resultContext;
		}

		ActionExecutingContext CreateActionExecutingContext(bool isAuthenticated, string userId = null)
		{
			var claims = new ClaimsIdentity();
			if (isAuthenticated)
			{
				claims = new ClaimsIdentity(
					new List<Claim>
					{
						new Claim(AuthClaimType.UniqueName, userId),
						new Claim(ClaimTypes.AuthenticationMethod, "TestAuthentication")
					}, "TestAuthentication");
			}

			var user = new ClaimsPrincipal(claims);
			var httpContext = new DefaultHttpContext { User = user };

			var actionContext = new ActionContext(httpContext, new RouteData(),
				new ActionDescriptor());
			return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
				new Dictionary<string, object>(), new object());
		}

		static IServiceProvider CreateServiceProvider(IAuthorizationHelper authorizationHelper)
		{
			var serviceCollection = new ServiceCollection();
			if (authorizationHelper != null)
			{
				serviceCollection.AddSingleton(authorizationHelper);
			}
			return serviceCollection.BuildServiceProvider();
		}
	}
}
