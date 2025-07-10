using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	public class IsEditableActionFilterAttributeFixture
	{
		[Test]
		public void TestOnActionExecuted()
		{
			var dummyController = new DummyController();
			var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), new ModelStateDictionary());

			var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), dummyController)
			{
				Result = new ObjectResult(CreateAList<Item1>())
			};

			var attribute = new IsEditableActionFilterAttribute();
			attribute.OnActionExecuted(context);

			dummyController.AuthHelperMock.Verify(x => x.InitAuthorizations(It.IsAny<IEnumerable<Item1>>(), It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void TestOnActionExecuted_ISelectExpandWrapper()
		{
			var dummyController = new DummyController();
			var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), new ModelStateDictionary());

			var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), dummyController)
			{
				Result = new ObjectResult(CreateAList<Item2>())
			};

			var attribute = new IsEditableActionFilterAttribute();
			attribute.OnActionExecuted(context);

			dummyController.AuthHelperMock.Verify(x => x.InitAuthorizationsForSelectExpandWrapper(It.IsAny<IEnumerable<Item2>>(), It.IsAny<string>()), Times.Once());
		}

		List<T> CreateAList<T>() where T : new()
		{
			var result = new List<T>
			{
				new T()
			};
			return result;
		}

		class DummyController : ODataController, IUserAuthorizationHelperProvider
		{
			public DummyController()
			{
				AuthHelperMock = new Mock<IAuthorizationHelper>();
			}

			public Mock<IAuthorizationHelper> AuthHelperMock { get; set; }

			public IAuthorizationHelper UserAuthorizationHelper => AuthHelperMock.Object;
		}
	}

	class Item1
	{
		public int I1_Code { get; set; }
	}

	class Item2 : ISelectExpandWrapper
	{
		public int I2_Code { get; set; }

		public IDictionary<string, object> ToDictionary()
		{
			throw new NotImplementedException();
		}

		public IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> propertyMapperProvider)
		{
			throw new NotImplementedException();
		}
	}
}
