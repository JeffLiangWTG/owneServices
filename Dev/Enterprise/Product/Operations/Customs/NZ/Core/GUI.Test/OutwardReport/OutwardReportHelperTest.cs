using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class OutwardReportHelperTest : TestCaseWithFactory
	{
		public void TestOutwardReportHelper()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			UnitTestUserNotification.Instance.ClearMessages();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			var additionalMessageInfo = OutwardReportHelper.GetAdditionalMessageInformation(consol, TSWTransactionTypes.Original, false);
			var ocrSender = new SendOCRFromConsolForTest(consol, additionalMessageInfo, manifestStatus, TSWTransactionTypes.Original);
			OutwardReportHelper.TryToSendOCR(manifestStatus, ocrSender, additionalMessageInfo, TSWTransactionTypes.Original, true, false);
			AssertEquals(0, consol.Messages.Count);
			AssertContains("Unable to send Original due to the following errors", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
			ocrSender.errorList.Clear();
			OutwardReportHelper.TryToSendOCR(manifestStatus, ocrSender, additionalMessageInfo, TSWTransactionTypes.Original, true, false);
			AssertEquals(1, consol.Messages.Count);
			AssertEquals("Original message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
		}

		ForwardingConsol consol;
	}
}
