using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(QueryQuotaVisaOptionForm))]
	sealed class QueryVisaOptionFormTest : ZFormBasherTest
	{
		public void TestSendButtonClick()
		{
			QueryQuotaVisaOption option = new QueryQuotaVisaOption(Factory, false);
			using (QueryQuotaVisaOptionForm form = new QueryQuotaVisaOptionForm(option))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendButton_Click(null, EventArgs.Empty);
				AssertEquals("PreCondition:Validation is run", true, option.HasNotifications());
				AssertEquals("Notifications are warned", QueryQuotaVisaOptionForm.NotificationsWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.QueryQuota);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory));
				MQEDIMessage[] messagesGenerated = Factory.Load<MQEDIMessage>(query);
				AssertEquals("No message is generated", 0, messagesGenerated.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				option.FillWithValidTestData();
				form.SendButton_Click(null, EventArgs.Empty);
				messagesGenerated = Factory.Load<MQEDIMessage>(query);
				AssertEquals("One message is generated", 1, messagesGenerated.Length);
			}
		}

		protected override Form GetFormToBashCore() => new QueryQuotaVisaOptionForm(new QueryQuotaVisaOption(Factory, true));

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
