using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(SendInBondMessageMenuItemForm))]
	sealed class SendInBondMessageMenuItemFormTest : ZFormBasherTest
	{
		public void TestShowFormAndClickSendButton_Arrival()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			var movementHeader4 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader4.InBondNumber = "111111114";
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3, movementHeader4 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var entity = form.BusinessEntity;
				AssertEquals(4, entity.InBondMenuItemMessageSendingObjects.Count);
				var inBondMenuItemMessageSendingObject = entity.InBondMenuItemMessageSendingObjects.AddNew();
				inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				sendButton.PerformClick();
				AssertContains("The messages connot be sent because there are errors in the InBondMenuItemMessageData", Enterprise.Customs.Business.MessageSendingValidation.ErrorExistHeaderText, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				entity.InBondMenuItemMessageSendingObjects.RemoveAndDelete(inBondMenuItemMessageSendingObject);
				inBondMenuItemMessageSendingObject = entity.InBondMenuItemMessageSendingObjects.AddNew();
				inBondMenuItemMessageSendingObject.InBondNumber = "111111115";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				entity.USDestinationPortCode = "A";
				entity.FIRMSCode = "B";
				entity.ArrivalDate = new ZDateTime(2021, 10, 22);
				sendButton.PerformClick();
				AssertContains("There are message errors in the InBondMenuItemMessageData", Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistHeaderText, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				sendButton.PerformClick();
				AssertContains("5 messages have been successfully sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowFormAndClickSendButton_Export()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			movementHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			movementHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			movementHeader3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var movementHeader4 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader4.InBondNumber = "111111114";
			movementHeader4.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3, movementHeader4 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var entity = form.BusinessEntity;
				AssertEquals(4, entity.InBondMenuItemMessageSendingObjects.Count);
				var inBondMenuItemMessageSendingObject = entity.InBondMenuItemMessageSendingObjects.AddNew();
				inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				sendButton.PerformClick();
				AssertContains("The messages connot be sent because there are errors in the InBondMenuItemMessageData", Enterprise.Customs.Business.MessageSendingValidation.ErrorExistHeaderText, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				entity.InBondMenuItemMessageSendingObjects.RemoveAndDelete(inBondMenuItemMessageSendingObject);
				inBondMenuItemMessageSendingObject = entity.InBondMenuItemMessageSendingObjects.AddNew();
				inBondMenuItemMessageSendingObject.InBondNumber = "111111115";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				entity.USDestinationPortCode = "A";
				entity.ExportDate = new ZDateTime(2021, 10, 22);
				sendButton.PerformClick();
				AssertContains("There are message errors in the InBondMenuItemMessageData", Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistHeaderText, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				sendButton.PerformClick();
				AssertContains("5 messages have been successfully sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowFormAndClickSendButton_PrintDocument()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				using (ZFormModaliser.SuspendDispose())
				{
					sendButton.PerformClick();
					var docDeliveryForm = (ZChildForm)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("Deliver Documents", docDeliveryForm.CaptionResourceString.Caption);
				}
			}
		}

		public void TestShowInfoWhenNoRecord_PrintDocument()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				sendButton.PerformClick();
				AssertContains("Please select at least one movement header to print.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormTextAndTabVisiableAndEntryType()
		{
			using (var form = new SendInBondMessageMenuItemForm(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival)))
			{
				form.Show();
				AssertEquals("Send In-Bond Arrival Messages", form.Text);
				var arrivalTabPage = form.Controls.Find("SendArrivalMessagesTabPage", true)[0] as ZTabPage;
				Assert("SendArrivalMessagesTabPage is Visible", arrivalTabPage.TabVisible);
				AssertEquals("SendExportationMessagesTabPage is Invisible", 0, form.Controls.Find("SendExportationMessagesTabPage", true).Length);
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				Assert("Have no EntryType", !grid.Columns.Contains("EntryType"));
				Assert("Have no PedimentoNumber", !grid.Columns.Contains("PedimentoNumber"));
				var splitContainer = form.Controls.Find("TopSplitContainer", true)[0] as KSplitContainer;
				Assert("Panels should not be collapsed", !splitContainer.Panel2Collapsed);
			}

			using (var form = new SendInBondMessageMenuItemForm(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export)))
			{
				form.Show();
				AssertEquals("Send In-Bond Exportation Messages", form.Text);
				AssertEquals("SendArrivalMessagesTabPage is Invisible", 0, form.Controls.Find("SendArrivalMessagesTabPage", true).Length);
				var exportTabPage = form.Controls.Find("SendExportationMessagesTabPage", true)[0] as ZTabPage;
				Assert("SendExportationMessagesTabPage is Visible", exportTabPage.TabVisible);
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				Assert("Have EntryType", grid.Columns.Contains("EntryType"));
				Assert("Have no PedimentoNumber", !grid.Columns.Contains("PedimentoNumber"));
				var splitContainer = form.Controls.Find("TopSplitContainer", true)[0] as KSplitContainer;
				Assert("Panels should not be collapsed", !splitContainer.Panel2Collapsed);
			}

			using (var form = new SendInBondMessageMenuItemForm(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento)))
			{
				form.Show();
				AssertEquals("Allocate Pedimento Number", form.Text);
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				Assert("Have no EntryType", !grid.Columns.Contains("EntryType"));
				var button = form.Controls.Find("SendButton", true)[0] as ZButton;
				AssertEquals("&OK", button.Text);
				var splitContainer = form.Controls.Find("TopSplitContainer", true)[0] as KSplitContainer;
				Assert("Panels should be collapsed", splitContainer.Panel2Collapsed);
			}

			using (var form = new SendInBondMessageMenuItemForm(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument)))
			{
				form.Show();
				AssertEquals("Bulk Print 7512 Departure Document", form.Text);
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				Assert("Have no PedimentoNumber", !grid.Columns.Contains("PedimentoNumber"));
				var button = form.Controls.Find("SendButton", true)[0] as ZButton;
				AssertEquals("&Next", button.Text);
				var splitContainer = form.Controls.Find("TopSplitContainer", true)[0] as KSplitContainer;
				Assert("Panels should be collapsed", splitContainer.Panel2Collapsed);
			}
		}

		public void TestShowFormAndAllocatePredimentoNumber()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var bill1 = inBondHeader1.Bills.AddNew();
			var fenNumber1 = bill1.AdditionalReferences.AddNew();
			fenNumber1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber1.BR_ReferenceNum = "A";
			var bill2 = inBondHeader1.Bills.AddNew();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			var movementDetail1 = movementHeader1.MovementDetails.AddNew();
			movementDetail1.B9_B0 = bill1.PK;
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			var movementDetail2 = movementHeader2.MovementDetails.AddNew();
			movementDetail2.B9_B0 = bill2.PK;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var bill3 = inBondHeader1.Bills.AddNew();
			var fenNumber2 = bill3.AdditionalReferences.AddNew();
			fenNumber2.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber2.BR_ReferenceNum = "B";
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			var movementDetail3 = movementHeader3.MovementDetails.AddNew();
			movementDetail3.B9_B0 = bill3.PK;
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var entity = form.BusinessEntity;
				AssertEquals(2, entity.InBondMenuItemMessageSendingObjects.Count);
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				sendButton.PerformClick();
				AssertContains("PedimentoNumber: Please enter a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "111111112");
				inBondMenuItemMessageSendingObject.PedimentoNumber = "D";
				inBondMenuItemMessageSendingObject = entity.InBondMenuItemMessageSendingObjects.AddNew();
				inBondMenuItemMessageSendingObject.InBondNumber = "111111113";
				sendButton.PerformClick();
				AssertContains("Allocate Pedimento Number Successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowFormAndValidateFIRMSCodeWithoutAirMode()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "1234", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";

			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader2 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			movementHeader2.BM_FIRMS = "1235";

			var inBondHeader3 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader3.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			movementHeader3.BM_FIRMS = "";

			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("FIRMS Code: The code you have selected is not in the list.(In-Bond: 111111112)", UnitTestUserNotification.Instance.LastMessage.Text);

				movementHeader2.BM_FIRMS = "1234";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);

				movementHeader3.BM_FIRMS = "1235";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("FIRMS Code: The code you have selected is not in the list.(In-Bond: 111111113)", UnitTestUserNotification.Instance.LastMessage.Text);

				inBondMenuItemMessageData.FIRMSCode = "1234";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("FIRMS Code: The code you have selected is not in the list.(In-Bond: 111111113)", UnitTestUserNotification.Instance.LastMessage.Text);

				movementHeader3.BM_FIRMS = "1234";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowFormAndValidateFIRMSCodeWithAirMode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "1234", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var inBondHeader1 = Factory.New<CusInBondHeader>();
			inBondHeader1.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";

			var inBondHeader2 = Factory.New<CusInBondHeader>();
			inBondHeader2.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var movementHeader2 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			movementHeader2.BM_FIRMS = "1235";

			var inBondHeader3 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader3.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			movementHeader3.BM_FIRMS = "";

			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			using (var form = new SendInBondMessageMenuItemForm(inBondMenuItemMessageData))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);

				movementHeader3.BM_FIRMS = "1235";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("FIRMS Code: The code you have selected is not in the list.(In-Bond: 111111113)", UnitTestUserNotification.Instance.LastMessage.Text);

				movementHeader3.BM_FIRMS = "1234";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);

				inBondMenuItemMessageData.FIRMSCode = "1235";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);

				inBondMenuItemMessageData.FIRMSCode = "1234";
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				sendButton.PerformClick();
				AssertNotContains("FIRMS Code: You have not entered a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("FIRMS Code: The code you have selected is not in the list.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
			=> new SendInBondMessageMenuItemForm(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
	}
}
