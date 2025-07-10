using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Enterprise.Services.ServiceHost.Common;
using NUnit.Framework;
using WTG.TestHelpers.IISExpress;
using WTG.TestHelpers.Net;

namespace Enterprise.Services.ServiceHost.Tests
{
	public abstract class IntegrationTestBase : TestCase
	{
		protected IISExpressHostedApplication expressApplication;
		public HttpClient HttpClient { get; private set; }
		UnusedPortLocator.Port port;
		protected string WebConfigPath => GetSupplementaryContentPath("Enterprise", "Services", "ServiceHost", "Web.config");
		Stream webConfigContent;

		protected override void SetUp()
		{
			base.SetUp();
			port = UnusedPortLocator.Find(35010);
			webConfigContent = new FileStream(WebConfigPath, FileMode.Open);
			expressApplication = IISExpressHostedApplication.CreateFromTestAssembly(typeof(Global), port.Num, webConfigContent,
				new string[]
				{
					"CargoWise.ApplicationContext.XmlSerializers.dll",
					"CargoWise.Definitions.XmlSerializers.dll",
				});
			CopyAssemblyMetaData(expressApplication.LocationOnDisk);
			expressApplication.Start();
			HttpClient = IISExpressHostedApplication.CreateClient();
		}

		void CopyAssemblyMetaData(string path)
		{
			var sourceDir = Path.GetDirectoryName(typeof(Global).Assembly.Location);
			var binDir = Path.Combine(path, "Bin");

			// Remove all ApplicationConfigurationAttribute setting to stop system expecting to have all dlls which have embedded configuration to exists in binDirectory
			var contents = File.ReadAllText(Path.Combine(sourceDir, "AssemblyMetaData.xml")).Replace("<ApplicationConfigurationAttribute", "<_ApplicationConfigurationAttribute").Replace("</ApplicationConfigurationAttribute", "</_ApplicationConfigurationAttribute");
			File.WriteAllText(Path.Combine(binDir, "AssemblyMetaData.xml"), contents);
		}

		protected override void TearDown()
		{
			base.TearDown();
			HttpClient?.Dispose();
			webConfigContent?.Dispose();
			if (expressApplication != null)
			{
				expressApplication.Stop();
				expressApplication.Dispose();
				expressApplication = null;
			}

			port?.Dispose();
		}

		public HttpRequestMessage CreateRequest(HttpMethod method, string url)
		{
			return new HttpRequestMessage(method, new Uri(expressApplication.ApplicationUri, url));
		}

		protected void TestSecurityHeaders(string url, HttpMethod method = null, HttpContent requestContent = null, HttpStatusCode? expectedStatusCode = HttpStatusCode.OK, Action<HttpRequestMessage> requestInit = null)
		{
			var request = CreateRequest(method ?? HttpMethod.Get, url);
			if (requestContent == null)
			{
				request.Content = requestContent;
			}
			if (requestInit != null)
			{
				requestInit(request);
			}

			var response = HttpClient.SendAsync(request).ConfigureAwait(true).GetAwaiter().GetResult();
			try
			{
				CombineAssertions(url, () =>
				{
					if (expectedStatusCode != null)
					{
						AssertEquals(expectedStatusCode, response.StatusCode);
					}
					foreach (var headerKeyValue in SecurityHelper.MandatoryHeaders)
					{
						AssertEquals($"Header does not exist: {headerKeyValue[0]}={headerKeyValue[1]}", true, response.Headers.Any(h => h.Key == headerKeyValue[0] && h.Value.Contains(headerKeyValue[1])));
					}
					foreach (var header in SecurityHelper.DangerousHeaders)
					{
						AssertEquals($"Header must NOT be in the response: {header}", false, response.Headers.Contains(header));
					}
				});
			}
			catch (AssertionFailedError)
			{
				//For debugging porpuses
				var content = response.Content.ReadAsStringAsync().Result;
				throw;
			}
		}
	}
}
