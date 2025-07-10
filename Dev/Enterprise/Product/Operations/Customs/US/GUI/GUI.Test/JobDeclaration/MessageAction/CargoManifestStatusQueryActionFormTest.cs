using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(CargoManifestStatusQueryActionForm))]
	sealed class CargoManifestStatusQueryActionFormTest : ZFormBasherTest
	{
		public void TestClickSendButtonWhenInvalid()
		{
			using (var form = (CargoManifestStatusQueryActionForm)GetFormToBash())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.headerSendingObject.ActionCode = "";
				AssertHasErrors("Action has an error", form.headerSendingObject.ActionCodeInfo);
				form.SendButton_Click(form.SendButton, EventArgs.Empty);
				AssertEquals("Please fix the errors first", UnitTestUserNotification.Instance.LastMessage.Text);
				form.headerSendingObject.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
				form.headerSendingObject.SendingObjects.OfType<CargoManifestQuerySendingObject>().ToList().ForEach(x => x.ShouldSendMessage = false);
				form.SendButton_Click(form.SendButton, EventArgs.Empty);
				AssertEquals("You have not selected any related records to send messages for.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, form.headerSendingObject.ShouldSendMessage);
			}
		}

		public void TestClickSendButtonForCargoManifestQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			using (var form = new CargoManifestStatusQueryActionForm(sendingHeader))
			{
				form.Show();
				sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
				sendingHeader.SendingObjects.OfType<CargoManifestQuerySendingObject>().ToList().ForEach(x => x.ShouldSendMessage = true);
				Assert(sendingHeader.SendingObjects[0].RequestForRelatedBOLInfo.ReadOnly);
				ZGridColumns columns = form.LevelsGrid.Columns;
				AssertDefaultColumn("HumanFriendlyReference", columns[0]);
				AssertDefaultColumn("ShouldSendMessage", columns[1]);
				AssertDefaultColumn("RequestForRelatedBOL", columns[2]);
				AssertDefaultColumn("UpdateEntryWithResults", columns[3]);
				AssertDefaultColumn("LimitOutputOption", columns[4]);
				form.SendButton_Click(null, EventArgs.Empty);
				AssertEquals("MessageAction.ShouldSendMessage should have been set", true, sendingHeader.ShouldSendMessage);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			return new CargoManifestStatusQueryActionForm(sendingHeader);
		}

		void AssertDefaultColumn(string expectedName, ZGridColumn column)
		{
			AssertEquals(expectedName, column.ColumnStyle.MappingName);
			AssertEquals(true, column.IsVisible);
		}
	}
}
