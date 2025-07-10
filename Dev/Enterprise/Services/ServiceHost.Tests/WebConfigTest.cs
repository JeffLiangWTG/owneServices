using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		XmlDocument GetWebConfigXmlFile()
		{
			var config = new XmlDocument();
			config.Load(DebugFilePath);
			AssertNotNull("The web configuration path must be valid", config);
			return config;
		}

		string GetApplicationSettingValue(string key)
		{
			var config = GetWebConfigXmlFile();

			var appSettingsSection = config.DocumentElement.SelectSingleNode("appSettings");
			AssertNotNull("The appSettings section is missing", appSettingsSection);

			var appSettingNode = appSettingsSection.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "add" && n.Attributes["key"].Value == key);
			AssertNotNull("The request queue limit per session node is missing", appSettingNode);

			return appSettingNode.Attributes["value"].Value;
		}

		public void TestRequestQueueLimitPerSession()
		{
			var appSettingValue = GetApplicationSettingValue("aspnet:RequestQueueLimitPerSession");

			var expectedValue = int.MaxValue.ToString();
			AssertEquals("Should have value from https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/retargeting/4.6.2-4.7.1#throttle-concurrent-requests-per-session", expectedValue, appSettingValue);
		}

		public void TestServerName()
		{
			var appSettingValue = GetApplicationSettingValue("ServerName");

			var expectedValue = ".";
			AssertEquals("ServerName should be not be changed", expectedValue, appSettingValue);
		}

		public void TestDatabaseName()
		{
			var appSettingValue = GetApplicationSettingValue("DatabaseName");

			var expectedValue = "Odyssey";
			AssertEquals("DatabaseName should be not be changed", expectedValue, appSettingValue);
		}

		public void TestSystemWebEnableVersionHeader()
		{
			var config = GetWebConfigXmlFile();

			var systemWeb = config.DocumentElement.SelectSingleNode("system.web");
			AssertNotNull("The system.web section is missing", systemWeb);

			var httpRuntime = systemWeb.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "httpRuntime");
			AssertNotNull("httpRuntime node is missing", httpRuntime);

			var enableVersionHeaderAttribute = httpRuntime.Attributes["enableVersionHeader"];
			AssertNull("enableVersionHeader is not compatible with IIS 8. While self hosted clients use IIS 8 we cannot add this", enableVersionHeaderAttribute);
		}

		public void TestSystemWebServerHeader()
		{
			var config = GetWebConfigXmlFile();

			var systemWeb = config.DocumentElement.SelectSingleNode("system.webServer");
			AssertNotNull("The system.webServer section is missing", systemWeb);

			var securityNode = systemWeb.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "security");
			AssertNotNull("security node is missing", securityNode);

			var requestFilteringNode = securityNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "requestFiltering" && n.Attributes["removeServerHeader"]?.Value == "true");
			AssertNull("removeServerHeader is not compatible with IIS 8. While self hosted clients use IIS 8 we cannot add this", requestFilteringNode);
		}

		public void TestSystemWebPoweredByHeader()
		{
			var config = GetWebConfigXmlFile();

			var systemWeb = config.DocumentElement.SelectSingleNode("system.webServer");
			AssertNotNull("The system.webServer section is missing", systemWeb);

			var httpProtocolNode = systemWeb.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "httpProtocol");
			AssertNotNull("httpProtocol node is missing", httpProtocolNode);

			var customHeadersNode = httpProtocolNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "customHeaders");
			AssertNotNull("customHeaders node is missing", customHeadersNode);

			var removeNode = customHeadersNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "remove" && n.Attributes["name"]?.Value == "X-Powered-By");
			AssertNotNull("removeNode node is missing", customHeadersNode);
		}

		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "Enterprise.Services.ServiceHost";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Services", "ServiceHost", "Web.config");
		protected override string DebugCompilationAssembliesXPath => "system.web/compilation/assemblies";
	}
}
