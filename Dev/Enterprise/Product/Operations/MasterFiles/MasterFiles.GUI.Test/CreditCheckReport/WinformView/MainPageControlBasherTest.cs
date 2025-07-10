using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using WTG.CreditCheck;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class MainPageControlBasherTest : ZFormBasherTest
	{
		public OrgHeader OrgWithCreditorAndDebtorOn
		{
			get
			{
				if (orgWithCreditorAndDebtorOn == null)
				{
					orgWithCreditorAndDebtorOn = Factory.New<OrgHeader>();
					orgWithCreditorAndDebtorOn.OH_RL_NKClosestPort = "AU";
				}

				return orgWithCreditorAndDebtorOn;
			}
		}

		OrgHeader orgWithCreditorAndDebtorOn;

		protected override Form GetFormToBashCore()
		{
			var form = new ZEmptyFormForBasherTest();
			var creditReportControl = new CreditReportUserControl();
			var mockService = new Mock<ISupportCreditCheckService>();
			mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new MockHttpMessageHandler());
			mockService.Setup(service => service.IsProductionSystem).Returns(creditReportControl.ServiceWrapper.IsProductionSystem);
			mockService.Setup(service => service.EndpointAddresses).Returns(creditReportControl.ServiceWrapper.EndpointAddresses);
			creditReportControl.ServiceWrapper = mockService.Object;

			creditReportControl.SetDataBinding(OrgWithCreditorAndDebtorOn, string.Empty);
			creditReportControl.Dock = DockStyle.Fill;
			form.CaptionRenderingEnabled = true;
			form.Size = ControlDpiScalingHelper.NewScaledSize(600, 800);
			form.Controls.Add(creditReportControl);
			form.ControllerID = DummyControllerIDs.Dummy;
			return form;
		}
	}

	class MockHttpMessageHandler : HttpClientHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		}
	}
}
