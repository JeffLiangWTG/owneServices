using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.NL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

sealed class MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestCreateMenuItems()
	{
		AssertContainsExactElementsInExactOrder(
			new[] {
				"Send to Customs",
				"Print Emergency Document",
				"Make Arrival Notification for this Departure",
				"-",
				"Inventory Management",
				"TS Register Management",
				"-",
				"Import Entry Lines",
				"Import Invoice Lines",
				"&Copy Previous Goods Item",
				"Lock Customs Declaration",
				"Unlock Customs Declaration"
			},
			menuItems.Select(x => x.Text));
	}

	public void TestGetProvider()
	{
		AssertType<MessagingMenuProvider>(Phase5MessagingMenuProvider.GetProvider(Factory.New<NctsHeader>()));
	}

	public void TestCanSendToCustoms_Departure() => CombineAssertions(() =>
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Send to Customs");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			AssertMenuItemVisibility(true, menuItem, NctsMovementType.Codes.Departure, messageStatus);
			AssertMenuItemVisibility(false, menuItem, NctsMovementType.Codes.Departure, messageStatus, true);
		}
	});

	public void TestCanSendToCustoms_Arrival() => CombineAssertions(() =>
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Send to Customs");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			AssertMenuItemVisibility(true, menuItem, NctsMovementType.Codes.Arrival, messageStatus);
		}
	});

	public void TestCanPrintEmergencyDocument_Departure() => CombineAssertions(() =>
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Print Emergency Document");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			AssertMenuItemVisibility(false, menuItem, NctsMovementType.Codes.Departure, messageStatus);
			AssertMenuItemVisibility(true, menuItem, NctsMovementType.Codes.Departure, messageStatus, true);
		}
	});

	public void TestCanPrintEmergencyDocument_Arrival() => CombineAssertions(() =>
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Print Emergency Document");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			AssertMenuItemVisibility(false, menuItem, NctsMovementType.Codes.Arrival, messageStatus);
		}
	});

	[TestDate(2025, 01, 29, 10, 04, 00)]
	public void TestPrintEmergencyDocument()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		var moveHeader = header.MovementHeader;
		moveHeader.IsFallbackProcedure = true;

		using (var form = new Phase5DepartureMovementForm(header))
		{
			form.Show();

			var menu = (Phase5NctsMessagingMenuItem)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&NCTS");
			var printEmergencyDocument = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Print Emergency Document");
			printEmergencyDocument.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Acceptance date is completed", new ZDateTime(2025, 01, 29, 10, 04, 00), moveHeader.BM_EntryDate);
				var queuedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
				AssertEquals("There should be a document in the print job queue.", 1, queuedPrintJobs.Length);
				var printJob = queuedPrintJobs[0];
				AssertNotNull("Print job is valid", printJob);
				Assert($"There should be DVA_NOOD in the declaration print job queue.", printJob.SP_EmailAttachments.StartsWith("DVA_NOOD"));
				AssertEquals("Departure Status", "EMP", moveHeader.BM_CustomsStatus);
				AssertEquals("Phase Status", "015", moveHeader.BM_Phase);
				AssertEquals("Message Status", "ACC", moveHeader.BM_MessageStatus);
			});
		}
	}

	public void TestSendPresentationNotification()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertSendMessage(NctsMessageTypeListNL.Codes.PresentationNotification, "CC170C");
	}

	public void TestSendNonArrivedInformation()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertSendMessage(NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement, "CC141C");
	}

	public void TestSendCancellation()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertSendMessage(NctsMessageTypeListNL.Codes.InvalidationCancellation, "CC014C");
	}

	public void TestSendDeclaration()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertSendMessage(NctsMessageTypeListNL.Codes.Declaration, "CC015C");
	}

	public void TestSendArrivalNotification()
	{
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertSendMessage(NctsMessageTypeListNL.Codes.ArrivalNotification, "CC007C");
	}

	public void TestSendAmendment()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertSendMessage(NctsMessageTypeListNL.Codes.Amendment, "CC013C");
	}

	public void TestSendUnloadingRemarks()
	{
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertSendMessage(NctsMessageTypeListNL.Codes.UnloadingRemarks, "CC044C");
	}

	public void TestNctsHeader()
	{
		AssertType<NctsHeader>(provider.Header);
	}

	public void TestIsResending()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var testprovider = new MessagingMenuProviderForTest(header);
		CombineAssertions(() =>
		{
			foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
			{
				header.MovementHeader.BM_MessageStatus = messageStatus;
				if (messageStatus.In("SNT", "ACK"))
				{
					AssertEquals("IsResending for message status " + messageStatus, true, testprovider.IsResendingOverride(header.MovementHeader));
				}
				else
				{
					AssertEquals("IsResending for message status " + messageStatus, false, testprovider.IsResendingOverride(header.MovementHeader));
				}
			}
		});
	}

	void AssertMenuItemVisibility(bool expected, ZMenuItem menuItem, string headerType, string messageStatus, bool fallBack = false)
	{
		header.BH_HeaderType = headerType;
		header.EffectiveMessageStatus = messageStatus;
		if (header.IsDepartureMovement)
		{
			header.MovementHeader.IsFallbackProcedure = fallBack;
		}
		provider.RefreshMenu();
		AssertEquals($"menuItem: {menuItem.Name}, headerType: {headerType}, messageStatus: {messageStatus}, fall back: {fallBack}", expected, menuItem.Visible);
	}

	void AssertSendMessage(ZString messageType, string expectedMessageType)
	{
		switch (header.BH_HeaderType)
		{
			case NctsMovementType.Codes.Arrival:
				using (var form = new Phase5ArrivalMovementForm(header))
				{
					DoSendAction(messageType, form, expectedMessageType);
				}
				break;
			case NctsMovementType.Codes.Departure:
				using (var form = new Phase5DepartureMovementForm(header))
				{
					DoSendAction(messageType, form, expectedMessageType);
				}
				break;
		}
	}

	void DoSendAction(ZString messageType, ZTemplateForm form, string expectedMessageType)
	{
		form.Show();

		var menu = (Phase5NctsMessagingMenuItem)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&NCTS");
		var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var dialog = (MessageSendingForm)obj;
			var action = dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
			action.MessageType = messageType;
			action.ShouldSend = true;
		});

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		sendToCustomsMenu.PerformClick();
		CombineAssertions(() =>
		{
			AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			EDIMessage sentMessage;
			if (header.IsDepartureMovement)
			{
				sentMessage = (EDIMessage)header.DepartureMovementHeaders[0].Messages.Single();
			}
			else
			{
				sentMessage = (EDIMessage)header.Messages.Single();
			}

			AssertContains($"<messageType>{expectedMessageType}</messageType>", sentMessage.EM_MessageText);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		provider = new MessagingMenuProvider(header);
		menuItems = provider.CreateMenuItems();
	}
	NctsHeader header;
	MessagingMenuProvider provider;
	IEnumerable<ZMenuItem> menuItems;

	class MessagingMenuProviderForTest : MessagingMenuProvider
	{
		public MessagingMenuProviderForTest(NctsHeader header) : base(header)
		{
		}

		public bool IsResendingOverride(CusInBondMoveHeader movementHeader) => base.IsResending(movementHeader);
	}
}
