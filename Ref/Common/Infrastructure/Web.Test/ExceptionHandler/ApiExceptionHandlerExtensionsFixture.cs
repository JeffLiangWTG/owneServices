using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.ExceptionHandler;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test.ExceptionHandler
{
	[TestFixture]
	public class ApiExceptionHandlerExtensionsFixture
	{
		/// <summary>
		/// WI00856814 Make sure to pass the Exception instance to the second param of log.Error
		/// </summary>
		/// <exception cref="Exception"></exception>
		[Test]
		public async Task CallLoggerWithExceptionAsSecondParam()
		{
			var exception = new Exception("Test exception");
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(l => l.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);

			var webHostBuilder = new WebHostBuilder()
				.ConfigureServices(services =>
				{
					services.AddSingleton(logWrapper.Object);
				})
				.Configure(app =>
				{
					app.UseCommonApiExceptionHandler();
					app.Run(context => throw exception);
				});

			using (var server = new TestServer(webHostBuilder))
			using (var client = server.CreateClient())
			{
				await client.GetAsync(new Uri("/", UriKind.Relative));
			}

			log.Verify(l => l.Error("Exception in /", exception), Times.Once);
		}
	}
}
