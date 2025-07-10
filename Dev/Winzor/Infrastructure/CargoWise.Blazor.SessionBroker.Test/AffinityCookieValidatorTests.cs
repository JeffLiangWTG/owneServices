using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using static CargoWise.Blazor.SessionBroker.Test.TestHelpers;

namespace CargoWise.Blazor.SessionBroker.Test
{
	class AffinityCookieValidatorTests
	{
		CustomWebApplicationFactory<Startup> factory;
		IAffinityCookieValidator validator;
		HttpClient client;
		string validCookie;

		[SetUp]
		public async Task SetUpAsync()
		{
			factory = new CustomWebApplicationFactory<Startup>();
			validator = factory.Services.GetService<IAffinityCookieValidator>();
			(client, var response) = await CreateTestNodeInCluster(factory);
			validCookie = GetAffinityCookie(response, client.BaseAddress);
		}

		[TearDown]
		public void TearDown()
		{
			client?.Dispose();
			factory?.Dispose();
		}

		[Test]
		public void ValidateShouldReturnFalseWhenCookieIsMissing()
		{
			var context = new DefaultHttpContext();
			Assert.That(validator.Validate(context), Is.False);
		}

		[Test]
		public void ValidateShouldReturnFalseWhenCookieIsInvalid()
		{
			var context = new DefaultHttpContext();
			var cookieValue = new CookieHeaderValue(AffinityCookieOptionsProvider.DefaultCookieName, "some_invalid_cookie").ToString();
			context.Request.Headers[HeaderNames.Cookie] = cookieValue;

			Assert.That(validator.Validate(context), Is.False);
		}

		[Test]
		public void ValidateShouldReturnTrueWhenCookieIsValid()
		{
			var context = new DefaultHttpContext();
			var cookieValue = new CookieHeaderValue(AffinityCookieOptionsProvider.DefaultCookieName, validCookie).ToString();
			context.Request.Headers[HeaderNames.Cookie] = cookieValue;

			Assert.That(validator.Validate(context), Is.True);
		}
	}
}
