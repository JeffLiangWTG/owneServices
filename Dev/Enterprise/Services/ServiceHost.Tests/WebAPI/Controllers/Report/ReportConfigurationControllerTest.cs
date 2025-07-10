using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ReportConfigurationControllerTest : BaseReportControllerTest<ReportConfigurationController>
	{
		public void TestGetConfigurations()
		{
			var expectedConfigurations = new List<ConfigurationData>
			{
				new ConfigurationData
				{
					Description = "Config1"
				},
				new ConfigurationData
				{
					Description = "Config2"
				}
			};

			var reportId = Guid.NewGuid();
			mockService.Setup(x => x.GetConfigurations(reportId)).Returns(expectedConfigurations).Verifiable();

			var actualResult = controller.GetConfigurations(reportId);
			AssertJsonResult(expectedConfigurations, actualResult);
		}

		public void TestSaveConfiguration()
		{
			var configurationData = new SelectedValueConfigurationData();
			configurationData.UniqueDescription = "Test Configuration";
			var configurations = new List<string>();
			mockService.Setup(x => x.SaveConfiguration(configurationData)).Callback(() =>
			{
				configurations.Add(configurationData.UniqueDescription);
			}).Verifiable();
			controller.SaveConfiguration(configurationData);

			AssertEquals(1, configurations.Count);
			AssertEquals("Test Configuration", configurations[0]);
		}

		[ExpectNoExceptions]
		public void TestDeleteConfiguration()
		{
			var reportId = Guid.NewGuid();
			var configurationName = "Configuration Name";
			mockService.Setup(x => x.DeleteConfiguration(reportId, configurationName)).Verifiable();
			controller.DeleteConfiguration(reportId, configurationName);
			mockService.Verify();
		}

		public void TestGetXmlFormat()
		{
			AssertEquals("", (controller.GetXmlFormat("") as OkNegotiatedContentResult<ZString>).Content);
			AssertEquals("", (controller.GetXmlFormat(null) as OkNegotiatedContentResult<ZString>).Content);
			var text = "a—— b_ c~ d! e！ f@ g# h￥ i$ j% k^ l…… m& n* o( p) q（ r）s；t; u? v？中 文";
			AssertEquals("ab_cdefghijPercentklmnopqrstuv中文", (controller.GetXmlFormat(text) as OkNegotiatedContentResult<ZString>).Content);
		}

		protected override string[] StaffOnlyAuthorizationFilterMethods => new string[] { nameof(ReportConfigurationController.SaveConfiguration), nameof(ReportConfigurationController.DeleteConfiguration) };
	}
}
