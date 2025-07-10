using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	public class CustomAuthorizationMiddlewareFixture
	{
		[Test]
		public async Task InvokeAsync_CheckAuthorization()
		{
			//user is authorized
			var contextTrue = new DefaultHttpContext();
			contextTrue.Request.Path = "/quartz/aaa";
			contextTrue.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "userTrue") }));
			contextTrue.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "execute_job" } });
			SetupRequestServices(contextTrue);

			//user is not authorized
			var contextFalse = new DefaultHttpContext();
			contextFalse.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "userFalse") }));
			contextFalse.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "execute_job" } });
			SetupRequestServices(contextFalse);

			//user is empty
			var contextWithoutUser = new DefaultHttpContext();
			contextWithoutUser.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "execute_job" } });
			SetupRequestServices(contextWithoutUser);

			mockAuthorizationHelper.Setup(o => o.IsAuthorized("userTrue", It.IsAny<IFormCollectionService>())).Returns(true);
			mockAuthorizationHelper.Setup(o => o.IsAuthorized("userFalse", It.IsAny<IFormCollectionService>())).Returns(false);

			await instance.InvokeAsync(contextTrue);
			await instance.InvokeAsync(contextFalse);
			await instance.InvokeAsync(contextWithoutUser);

			Assert.That(contextTrue.User, Is.Not.Null);
			Assert.That(contextFalse.User, Is.Not.Null);
			Assert.That(contextWithoutUser.User, Is.Not.Null);
			mockNext.Verify(x => x.Invoke(contextTrue), Times.Once);
			mockNext.Verify(x => x.Invoke(contextFalse), Times.Never);
			mockNext.Verify(x => x.Invoke(contextWithoutUser), Times.Never);
		}

		[Test]
		public async Task InvokeAsync_CheckCommand()
		{
			//Request doesn't have form
			var context1 = new DefaultHttpContext();
			SetupRequestServices(context1);

			//no command
			var context2 = new DefaultHttpContext();
			context2.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "NoCommand", "" } });
			SetupRequestServices(context2);

			//command is empty
			var context3 = new DefaultHttpContext();
			context3.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "" } });
			SetupRequestServices(context3);

			//command starts with get
			var context4 = new DefaultHttpContext();
			context4.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "get_data" } });
			SetupRequestServices(context4);

			//command doesn't start with get
			var context5 = new DefaultHttpContext();
			context5.Request.Form = new FormCollection(new Dictionary<string, StringValues> { { "command", "execute_job" } });
			SetupRequestServices(context5);

			await instance.InvokeAsync(context1);
			await instance.InvokeAsync(context2);
			await instance.InvokeAsync(context3);
			await instance.InvokeAsync(context4);
			await instance.InvokeAsync(context5);

			mockNext.Verify(x => x.Invoke(context1), Times.Once);
			mockNext.Verify(x => x.Invoke(context2), Times.Once);
			mockNext.Verify(x => x.Invoke(context3), Times.Once);
			mockNext.Verify(x => x.Invoke(context4), Times.Once);
			mockNext.Verify(x => x.Invoke(context5), Times.Never);
		}

		void SetupRequestServices(HttpContext context)
		{
			mockContextAccessor.Setup(x=> x.HttpContext).Returns(context);
			var formCollectionService = new FormCollectionService(mockContextAccessor.Object);
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(typeof(IFormCollectionService))).Returns(formCollectionService);
			context.RequestServices = mockServiceProvider.Object;
		}

		[SetUp]
		public void Setup()
		{
			mockContextAccessor = new Mock<IHttpContextAccessor>();
			mockAuthorizationHelper = new Mock<IAuthorizationHelper>();
			mockNext = new Mock<RequestDelegate>();
			instance = new CustomAuthorizationMiddleware(mockNext.Object, mockAuthorizationHelper.Object);
		}

		Mock<IHttpContextAccessor> mockContextAccessor;
		Mock<IAuthorizationHelper> mockAuthorizationHelper;
		Mock<RequestDelegate> mockNext;
		CustomAuthorizationMiddleware instance;
	}
}
