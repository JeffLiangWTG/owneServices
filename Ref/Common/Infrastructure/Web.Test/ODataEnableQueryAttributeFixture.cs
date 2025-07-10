using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class ODataEnableQueryAttributeFixture
	{
		[Test]
		public void ValidateQuery_NullReferenceException()
		{
			var attribute = new ODataEnableQueryAttribute();
			var exception = Assert.Throws<ArgumentException>(() => attribute.ValidateQuery(new Mock<HttpRequest>().Object, null));
			Assert.That(exception.InnerException != null);
			Assert.That(exception.InnerException is ArgumentNullException);
			Assert.That(exception.Message == exception.InnerException.Message);
			Assert.That(exception.Message == "Value cannot be null. (Parameter 'queryOptions')");

			exception = Assert.Throws<ArgumentException>(() => attribute.ValidateQuery(null, null));
			Assert.That(exception.InnerException is ArgumentNullException);
			Assert.That(exception.Message == "Value cannot be null. (Parameter 'request')");
		}

		[Test]
		public void ValidateQuery_ODataException()
		{
			var attribute = new ODataEnableQueryAttribute();
			var request = CreateRequest("Get", "http://localhost/?$xxx");
			var model = GetEdmModel();
			var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(DummyParent), null), request);

			var exception = Assert.Throws<ArgumentException>(() => attribute.ValidateQuery(request, options));
			Assert.That(exception.InnerException is ODataException);
			Assert.That(exception.Message == exception.InnerException.Message);
			Assert.That(exception.InnerException.Message == "Custom query option '$xxx' that starts with '$' is not supported.");
		}

		[TestCase("$filter=Name eq 'abc'")]
		[TestCase("$select=ID")]
		[TestCase("$orderby=ID")]
		[TestCase("$skip=1")]
		[TestCase("$top=2")]
		[TestCase("$expand=DummyCodes")]
		public void ValidateQuery_NoException(string query)
		{
			var attribute = new ODataEnableQueryAttribute();
			HttpRequest request = CreateRequest("Get", "http://localhost/?" + query);

			var model = GetEdmModel();
			var context = new ODataQueryContext(model, typeof(DummyParent), null);
			context.DefaultQueryConfigurations.EnableFilter = true;
			context.DefaultQueryConfigurations.EnableSelect = true;
			context.DefaultQueryConfigurations.EnableOrderBy = true;
			context.DefaultQueryConfigurations.EnableExpand = true;
			context.DefaultQueryConfigurations.MaxTop = 10;

			var options = new ODataQueryOptions(context, request);
			Assert.DoesNotThrow(() => attribute.ValidateQuery(request, options));
		}

		static HttpRequest CreateRequest(string method, string uri)
		{
			HttpContext context = new DefaultHttpContext();
			HttpRequest request = context.Request;
			IServiceCollection services = new ServiceCollection();
			context.RequestServices = services.BuildServiceProvider();

			request.Method = method;
			Uri requestUri = new Uri(uri);
			request.Scheme = requestUri.Scheme;
			request.Host = requestUri.IsDefaultPort ? new HostString(requestUri.Host) : new HostString(requestUri.Host, requestUri.Port);
			request.QueryString = new QueryString(requestUri.Query);
			request.Path = new PathString(requestUri.AbsolutePath);
			return request;
		}

		static IEdmModel GetEdmModel()
		{
			var builder = new ODataConventionModelBuilder();
			builder.EntitySet<DummyParent>("DummyParents");
			builder.EntitySet<DummyCode>("DummyCodes");
			return builder.GetEdmModel();
		}
	}
}
