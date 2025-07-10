using System.Reflection;
using Enterprise.MasterFiles.GUI.Test;
using Moq;
using NUnit.Framework;
using WTG.CreditCheck;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PayablesUserControl))]
	sealed class PayablesUserControlSecurityTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			var payablesUserControl = new PayablesUserControl();
			var creditReportControlField = payablesUserControl.GetType().GetField("creditReportControl", BindingFlags.Instance | BindingFlags.NonPublic);
			var creditReportUserControl = creditReportControlField.GetValue(payablesUserControl) as CreditReportUserControl;
			var mockService = new Mock<ISupportCreditCheckService>();
			mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new MockHttpMessageHandler());
			mockService.Setup(service => service.IsProductionSystem).Returns(creditReportUserControl.ServiceWrapper.IsProductionSystem);
			mockService.Setup(service => service.EndpointAddresses).Returns(creditReportUserControl.ServiceWrapper.EndpointAddresses);
			creditReportUserControl.ServiceWrapper = mockService.Object;
			return payablesUserControl;
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyPayables" }; }
		}
	}
}
