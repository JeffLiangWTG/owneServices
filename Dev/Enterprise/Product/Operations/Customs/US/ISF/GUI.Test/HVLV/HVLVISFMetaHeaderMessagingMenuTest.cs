using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	public class HVLVISFMetaHeaderMessagingMenuTest : TestCaseWithFactory
	{
		void SetZFormModaliserDelegate()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is HVLVISFMessagesSendForm messageSendForm)
				{
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonSend").PerformClick();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}
			});
		}

		public void TestSendMessages()
		{
			SetZFormModaliserDelegate();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder("Pre-condition: Expected no messages to have been sent for both ISF headers.", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x => x.Messages.Count));

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				AssertNotNull(sendMenuItem);
				sendMenuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Expected to have sent 1 message for each related jobs.", new[] { 1, 1 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
				{
					x.Messages.Load();
					return x.Messages.Count;
				}));
			}
		}

		public void TestGivenISFHeadersWithDeletedCustomsReference_WhenSendMessage_ThenDoNoSendMessageAndDisplayErrors()
		{
			SetZFormModaliserDelegate();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";
			consignment3.HVC_WaybillNumber = "HVC003";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";
				headers[2].BF_JobReference = "ISF003";
				headers[0].BF_CustomsReference = "CUS001";
				headers[1].BF_CustomsReference = "CUS002";
				headers[2].BF_CustomsReference = "CUS003";
				headers[0].BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
				headers[1].BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;

				AssertEquals("Pre-Condition", "HVC001", headers[0].BF_HouseBill);
				AssertEquals("Pre-Condition", "HVC002", headers[1].BF_HouseBill);
				AssertEquals("Pre-Condition", "HVC003", headers[2].BF_HouseBill);

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder("Pre-condition: Expected no messages to have been sent for both ISF headers.", new[] { 0, 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x => x.Messages.Count));

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				sendMenuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Expected to have sent no message for each related jobs.", new[] { 0, 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
				{
					x.Messages.Load();
					return x.Messages.Count;
				}));

				AssertEquals("Expected the error message to contain 'Cannot send further messages as at least one Customs Reference has been deleted from Customs system.'", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Cannot send further messages as at least one Customs Reference has been deleted from Customs system."));
				AssertEquals("Expected the error message to contain 'You can use 'Reset to Original' to reuse the job and get a new Customs Reference.'", true, UnitTestUserNotification.Instance.LastMessage.Contains("You can use 'Reset to Original' to reuse the job and get a new Customs Reference."));
				AssertEquals("Expected the error message to contain HVC001 HouseBill", true, UnitTestUserNotification.Instance.LastMessage.Contains("HVC001"));
				AssertEquals("Expected the error message to contain HVC002 HouseBill", true, UnitTestUserNotification.Instance.LastMessage.Contains("HVC002"));
				AssertEquals("Expected the error message not to contain HVC003 HouseBill", false, UnitTestUserNotification.Instance.LastMessage.Contains("HVC003"));
			}
		}

		public void TestGivenISFHeadersHasChanges_WhenSendingMessage_ThenShowDataNotYetBeenSavedErrorAndDoNotSendMessage()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is HVLVISFMessagesSendForm messageSendForm)
				{
					var selectAllButton = messageSendForm.FindSingleOrDefault<ZButton>("ButtonSelectAll");
					selectAllButton?.PerformClick();

					var sendButton = messageSendForm.FindSingleOrDefault<ZButton>("ButtonSend");
					sendButton?.PerformClick();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				var isfHeader = form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().FirstOrDefault();
				isfHeader.BF_JobReference = "ISF003";

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				sendMenuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Expected to have sent no message for each related jobs.", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
				{
					x.Messages.Load();
					return x.Messages.Count;
				}));

				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGivenISFHeadersWithValidationWarnings_WhenSendingMessage_ThenShowWarningsAndPromptIfUserShouldContinue()
		{
			SetZFormModaliserDelegate();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				sendMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Expected warning message", "Your message(s) have warnings.\r\nDo you want to send the message(s) despite these warnings?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

					AssertContainsExactElementsInAnyOrder("Expected to have sent 1 message for each related jobs.", new[] { 1, 1 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
					{
						x.Messages.Load();
						return x.Messages.Count;
					}));
				});
			}
		}

		public void TestGivenISFHeadersWithValidationWarnings_WhenUserClicksNoOnWarningPrompt_DoNotSendMessages()
		{
			SetZFormModaliserDelegate();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				sendMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Expected warning message", "Your message(s) have warnings.\r\nDo you want to send the message(s) despite these warnings?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertContainsExactElementsInAnyOrder("Expected no messages to have been sent", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
					{
						x.Messages.Load();
						return x.Messages.Count;
					}));
				});
			}
		}

		public void TestGivenISFHeadersWithErrorMessage()
		{
			SetZFormModaliserDelegate();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";

				var isfLine = Factory.NewWithValidTestData<CusISFLine>();
				isfLine.CustomAttribute1 = "ian";
				isfLine.AddRowError("error");
				headers[0].Lines.Add(isfLine);
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				sendMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Expected warning message", "Your message(s) have errors.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertContainsExactElementsInAnyOrder("Expected no messages to have been sent", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x =>
					{
						x.Messages.Load();
						return x.Messages.Count;
					}));
				});
			}
		}

		public void TestSendMessages_WhenSelectAllButtonIsClickedTwice_NoMessagesAreSelected()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is HVLVISFMessagesSendForm messageSendForm)
				{
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonSelectAll").PerformClick();
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonSend").PerformClick();
				}
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder("Pre-condition: Expected no messages to have been sent for both ISF headers.", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x => x.Messages.Count));

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				AssertNotNull(sendMenuItem);
				sendMenuItem.PerformClick();

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "No Job Number has been flagged for submission.");
			}
		}

		public void TestSendMessages_WhenCancelButtonIsClicked_NoMessagesAreSent()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is HVLVISFMessagesSendForm messageSendForm)
				{
					messageSendForm.FindSingleOrDefault<ZButton>("ButtonCancel").PerformClick();
				}
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();
			}

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder("Pre-condition: Expected no messages to have been sent for both ISF headers.", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x => x.Messages.Count));

				var sendMenuItem = form.Menu.MenuItems.FindByText("Send", true);
				AssertNotNull(sendMenuItem);
				sendMenuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder("Cancel button clicked: Expected no messages to have been sent for both ISF headers.", new[] { 0, 0 }, form.MetaHeader.RelatedJobs.OfType<CusISFHeader>().Select(x => x.Messages.Count));
			}
		}

		public void TestSendMessageAfterCustomsReceived_Send()
		{
			TestSendMessageAfterCustomsReceived("Send", "Action Code (8-8)                   :R");
		}

		public void TestSendMessageAfterCustomsReceived_Delete()
		{
			TestSendMessageAfterCustomsReceived("Delete", "Action Code (8-8)                   :D");
		}

		void TestSendMessageAfterCustomsReceived(string sendMessageButtonText, string expectedActionCodeInMessage)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is HVLVISFMessagesSendForm messageSendForm)
				{
					var selectAllButton = messageSendForm.FindSingleOrDefault<ZButton>("ButtonSelectAll");
					selectAllButton?.PerformClick();

					UnitTestUserNotification.Instance.AddYesAnswer();

					var sendButton = messageSendForm.FindSingleOrDefault<ZButton>("ButtonSend");
					sendButton?.PerformClick();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}
			});

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "HVC001";

			Factory.Save();

			CusISFHeader header;
			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				header = Factory.Load<CusISFHeader>(headerQuery)[0];
				header.BF_JobReference = "ISF001";
				header.BF_CustomsReference = "CustomsRef";
				header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;

				Factory.Save();
			}

			Assert("Precondition: ShouldSendAdd is false", !header.ShouldSendAdd);

			using (var form = new HVLVISFMetaHeaderFormForTest(shipment.Factory, shipment.PK))
			{
				form.Show();
				Application.DoEvents();
				form.FireSaveButton();

				var sendMenuItem = form.Menu.MenuItems.FindByText(sendMessageButtonText, true);
				sendMenuItem.PerformClick();
			}

			header.Reload();
			AssertContains(expectedActionCodeInMessage, header.Messages[0].EM_MessageInterpretation);
		}
	}
}
