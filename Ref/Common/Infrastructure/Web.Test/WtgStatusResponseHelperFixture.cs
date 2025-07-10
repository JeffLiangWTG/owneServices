using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class WtgStatusResponseHelperFixture
	{
		[TestCase("GET")]
		[TestCase("HEAD")]
		[TestCase("POST")]
		[TestCase("OPTIONS")]
		[TestCase("PUT")]
		[TestCase("DELETE")]
		public async Task WriteWtgStatusResponse(string requestMethod)
		{
			requestMethod = requestMethod.ToUpperInvariant();
			var context = new DefaultHttpContext();
			context.Response.Body = new MemoryStream();
			context.Request.Method = requestMethod;
			var entries = new Dictionary<string, HealthReportEntry>
			{
				["service1"] = new HealthReportEntry(HealthStatus.Healthy, "ok", TimeSpan.FromSeconds(1), null, null),
				["service2"] = new HealthReportEntry(HealthStatus.Degraded, "ok", TimeSpan.FromSeconds(1), null, null),
				["service3"] = new HealthReportEntry(HealthStatus.Unhealthy, "unavailable.\r\nAn error occurred", TimeSpan.FromSeconds(1), null, null)
			};
			var healthReport = new HealthReport(new ReadOnlyDictionary<string, HealthReportEntry>(entries), TimeSpan.FromSeconds(3));
			await WtgStatusResponseHelper.WriteWtgStatusResponse(context, healthReport);
			if (requestMethod == "GET" || requestMethod == "HEAD")
			{
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
				Assert.AreEqual("text/plain; charset=utf-8", context.Response.ContentType);
				context.Response.Body.Position = 0;
				using (var reader = new StreamReader(context.Response.Body))
				{
					var content = reader.ReadToEnd();
					Assert.AreEqual(@"INFO(service1): ok
WARNING(service2): ok
ERROR(service3): unavailable.  An error occurred", content);
				}
			}
			else
			{
				Assert.AreEqual(StatusCodes.Status405MethodNotAllowed, context.Response.StatusCode);
				Assert.AreEqual(new[] { HttpMethods.Get, HttpMethods.Head }, context.Response.Headers.Allow.ToArray());
			}
		}
	}
}
