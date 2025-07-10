using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public class EDIMenu : EU.GUI.EDIMenu
{
	public EDIMenu()
	{
	}

	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
		set { base.Declaration = value; }
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();
		MenuItems.Remove(SADHDataEntryFormMenuItem);
		MenuItems.Remove(SingleLineEntryMenuItem);

		sendToCustomsMenuItem = new ZMenuItem(Constants.SendToCustomsCaption, (o, e) => SendToCustoms());
		MenuItems.Add(sendToCustomsMenuItem);
		declarationsMenuItem = new ZMenuItem(Constants.SendToCustomsDeclarationsCaption, (o, e) => SendToCustoms());
		serviceMessagesMenuItem = new ZMenuItem(Constants.ServiceMessagesCaption);
		dutyGuaranteeMenuItem = new ZMenuItem(Constants.DutyGuaranteeCaption, (o, e) => SendToCustoms());
		vatDefermentMenuItem = new ZMenuItem(Constants.VatDefermentCaption, (o, e) => SendVatDeferment());
		guaranteeAmountMenuItem = new ZMenuItem(Constants.GuaranteeAmountCaption, (o, e) => SendGuaranteeAmount());
		portMessagingMenuItem = new ZMenuItem(Constants.PortMessagingCaption);
		cargonautExportMenuItem = new ZMenuItem(Constants.CargonautExportCaption);
		portbaseExportMenuItem = new ZMenuItem(Constants.PortbaseExportCaption);
		portbaseImportMenuItem = new ZMenuItem(Constants.PortbaseImportCaption);
		amendDeclarationMenuItem = new ZMenuItem(Constants.AmendDeclarationCaption);
		cancelAmendDeclarationMenuItem = new ZMenuItem(Constants.CancelAmendRequestCaption, (o, e) => CancelAmendDeclarations());
		amendmentRequestMenuItem = new ZMenuItem(Constants.AmendmentRequestCaption, (o, e) => SetAmendDeclarations());
		responseToRfiSentMenuItem = new ZMenuItem(Constants.ResponseToRfiSentCaption, (o, e) => SetResponseToRFISent());
		cancelResponseToRFISentMenuItem = new ZMenuItem(Constants.CancelResponseToRFISentCaption, (o, e) => CancelResponseToRFISent());
		supplementaryDeclarationMenuItem = new ZMenuItem(Constants.SupplementaryDeclarationCaption, (o, e) => SetSupplementaryDeclarations());
		cancelSupplementaryDeclarationMenuItem = new ZMenuItem(Constants.CancelSupplementaryDeclarationCaption, (o, e) => CancelSupplementaryDeclarations());

		MenuItems.Add(portMessagingMenuItem);
		MenuItems.Add(amendDeclarationMenuItem);
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();

		if (Declaration != null && !Declaration.IsInterface)
		{
			sendToCustomsMenuItem.MenuItems.Clear();
			if (Declaration.IsImport)
			{
				sendToCustomsMenuItem.MenuItems.Add(declarationsMenuItem);
				sendToCustomsMenuItem.MenuItems.Add(serviceMessagesMenuItem);
				serviceMessagesMenuItem.MenuItems.Clear();
				serviceMessagesMenuItem.MenuItems.Add(dutyGuaranteeMenuItem);
				serviceMessagesMenuItem.MenuItems.Add(vatDefermentMenuItem);
				serviceMessagesMenuItem.MenuItems.Add(guaranteeAmountMenuItem);
			}
			if (portMessagingMenuItem != null)
			{
				portMessagingMenuItem.MenuItems.Clear();

				if (Declaration.IsImport)
				{
					portMessagingMenuItem.MenuItems.Add(portbaseImportMenuItem);
				}
				else if (Declaration.IsExport)
				{
					portMessagingMenuItem.MenuItems.Add(cargonautExportMenuItem);
					portMessagingMenuItem.MenuItems.Add(portbaseExportMenuItem);
				}
			}
			if (amendDeclarationMenuItem != null)
			{
				amendDeclarationMenuItem.MenuItems.Clear();

				amendDeclarationMenuItem.MenuItems.Add(amendmentRequestMenuItem);
				amendDeclarationMenuItem.MenuItems.Add(responseToRfiSentMenuItem);
				amendDeclarationMenuItem.MenuItems.Add(supplementaryDeclarationMenuItem);
				amendDeclarationMenuItem.MenuItems.Add(cancelSupplementaryDeclarationMenuItem);
				amendDeclarationMenuItem.MenuItems.Add(cancelAmendDeclarationMenuItem);
				amendDeclarationMenuItem.MenuItems.Add(cancelResponseToRFISentMenuItem);

				supplementaryDeclarationMenuItem.Enabled = ShouldSupplementaryDeclarationMenuItemBeEnabled();
				cancelSupplementaryDeclarationMenuItem.Visible = ShouldCancelSupplementaryDeclarationMenuItemBeVisible();

				amendmentRequestMenuItem.Enabled = ShouldAmendDeclarationMenuItemBeEnabled();
				cancelAmendDeclarationMenuItem.Visible = ShouldCancelAmendDeclarationMenuItemBeVisible();

				responseToRfiSentMenuItem.Enabled = ShouldResponseToRFISentMenuItemBeEnabled();
				cancelResponseToRFISentMenuItem.Visible = ShouldCancelResponseToRFISentMenuItemBeVisible();
			}
			sendToCustomsMenuItem.Visible = true;
			portMessagingMenuItem.Visible = true;
			amendDeclarationMenuItem.Visible = true;
		}
		else
		{
			sendToCustomsMenuItem.Visible = false;
			portMessagingMenuItem.Visible = false;
			amendDeclarationMenuItem.Visible = false;
		}
	}

	#region SendToCustoms
	void SendToCustoms()
	{
		bool continueWithSend = true;
		var decWrapper = new JobDeclarationMessageSendingObjectParent(Declaration);

		if (Declaration is JobDeclaration declaration && PreSaveAndMergeIfNeeded(declaration))
		{
			using (var form = GetMessageSendingForm(decWrapper))
			{
				continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}

			if (continueWithSend && CheckCreditGUIHelper.CheckCredit(Declaration))
			{
				var senders = new List<NLMessageSender>();
				var messageSendingObjects = decWrapper.SendingObjectsCollection.Cast<Business.JobDeclarationMessageSendingObject>();
				foreach (var messageSendingObject in messageSendingObjects)
				{
					if (declaration.IsExport)
					{
						switch (messageSendingObject.MessageType)
						{
							case ExportSendMessageTypes.Codes.AMD:
								senders.Add(new ExportAmendmentMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.CAN:
								senders.Add(new ExportCancellationMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.CRE:
								senders.Add(new ExportResponseToRFIMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.DEC:
								senders.Add(new ExportDeclarationMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.EXT:
								senders.Add(new ExportExitInformationMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.FBK:
								senders.Add(new ExportFallbackMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.PRE:
								senders.Add(new ExportPresentationMessageSender(messageSendingObject));
								break;
							case ExportSendMessageTypes.Codes.SUP:
								senders.Add(new ExportSupplementMessageSender(messageSendingObject));
								break;
						}
					}
					else
					{
						switch (messageSendingObject.MessageType)
						{
							case ImportSendMessageTypes.Codes.DEC:
								senders.Add(new ImportDeclarationMessageSender(messageSendingObject));
								break;
							case ImportSendMessageTypes.Codes.PRE:
								senders.Add(new ImportPresentationMessageSender(messageSendingObject));
								break;
						}
					}
				}

				if (senders.Count > 0)
				{
					senders.ForEach(x => x.Send());
					try
					{
						declaration.Factory.Save();
						Globals.Message.Show(Res.GetString("8452D6C0-E8CE-4B45-802E-D0177448050B", "The message(s) have been sent to Customs."));
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}
	}

	void SendVatDeferment()
	{
		var continueWithSend = true;
		var declarationWrapper = new JobDeclarationMessageSendingObjectParent(Declaration);

		using (var form = new CheckVatDefermentForm(Declaration))
		{
			continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form).Equals(DialogResult.OK);
		}
		if (continueWithSend && CheckCreditGUIHelper.CheckCredit(Declaration))
		{
			var messageSender = new VatPartyInformationMessageSender(declarationWrapper);
			var response = messageSender.SendMessage();
			Globals.Message.Show(response, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	void SendGuaranteeAmount()
	{
		var continueWithSend = true;
		var declarationWrapper = new JobDeclarationMessageSendingObjectParent(Declaration);
		using (var form = new CheckGuaranteeAmountForm(Declaration))
		{
			continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form).Equals(DialogResult.OK);
		}
		if (continueWithSend && CheckCreditGUIHelper.CheckCredit(Declaration))
		{
			var messageSender = new GuaranteeAmountInformationMessageSender(declarationWrapper);
			var response = messageSender.SendMessage();
			Globals.Message.Show(response, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	#endregion

	bool ShouldSupplementaryDeclarationMenuItemBeEnabled()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForSupplement();
		return entrySelectionCollection.Count > 0;
	}

	bool ShouldCancelSupplementaryDeclarationMenuItemBeVisible()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForCancelSupplement();
		return entrySelectionCollection.Count > 0;
	}

	void SetSupplementaryDeclarations()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForSupplement(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForSUP()
		, Res.GetData("05BDE10A-BFD0-4334-8550-F4824BE55A77", "Select Entry For Supplementary Mode"));
	}

	void CancelSupplementaryDeclarations()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForCancelSupplement(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForCancelSUP()
		, Res.GetData("5DAFDEA1-957D-4E17-82A1-4DCCD6E70629", "Cancel Supplementary"));
	}

	bool ShouldAmendDeclarationMenuItemBeEnabled()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForAmendment();
		return entrySelectionCollection.Count > 0;
	}

	bool ShouldCancelAmendDeclarationMenuItemBeVisible()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForCancelAmendment();
		return entrySelectionCollection.Count > 0;
	}

	void SetAmendDeclarations()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForAmendment(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForAMD()
		, Res.GetData("B1163242-AD79-45F7-A9DE-3EBB17275DAA", "Select Entry For Amendment Mode"));
	}

	void CancelAmendDeclarations()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForCancelAmendment(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForCancelAMD()
		, Res.GetData("C1645EBB-766A-40F7-8C99-CD5271CFE3AA", "Cancel Amendment"));
	}

	bool ShouldResponseToRFISentMenuItemBeEnabled()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForCRE();
		return entrySelectionCollection.Count > 0;
	}

	bool ShouldCancelResponseToRFISentMenuItemBeVisible()
	{
		var entrySelectionCollection = Declaration.EntrySelections;
		entrySelectionCollection.PopulateCollectionForCancelCRE();
		return entrySelectionCollection.Count > 0;
	}

	void SetResponseToRFISent()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForCRE(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForCRE()
		, Res.GetData("1BEF7577-EECE-4621-B031-E0F4AC0F5C63", "Select Entry For CRE Mode"));
	}

	void CancelResponseToRFISent()
	{
		ModeMenuItemClick((EntrySelectionCollection x) => x.PopulateCollectionForCancelCRE(), (EntrySelection x) => x.SetStatusToCusEntryHeaderForCancelCRE()
		, Res.GetData("05B6DA2C-586A-48D1-8F52-0CC9E7028CAB", "Cancel CRE Mode"));
	}

	void ModeMenuItemClick(Action<EntrySelectionCollection> populateCollection, Action<EntrySelection> setStatusToCusEntryHeader, ResourceStringData formTitle)
	{
		if (Declaration is JobDeclaration declaration && PreSaveAndMergeIfNeeded(declaration))
		{
			using (var form = GetEntrySelectionForm(formTitle))
			{
				var entrySelectionCollection = Declaration.EntrySelections;
				populateCollection(entrySelectionCollection);
				if (entrySelectionCollection.Count > 1)
				{
					var continueSelection = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					if (continueSelection)
					{
						var entrySelection = entrySelectionCollection.Cast<EntrySelection>().SingleOrDefault(x => x.Selected);
						if (entrySelection != null)
						{
							setStatusToCusEntryHeader(entrySelection);
						}
					}
				}
				else
				{
					var entrySelection = (EntrySelection)entrySelectionCollection.Single();
					setStatusToCusEntryHeader(entrySelection);
				}
			}
		}
	}

	bool PreSaveAndMergeIfNeeded(JobDeclaration declaration)
	{
		if (PreSaveDeclaration(declaration))
		{
			var needMerge = declaration != null && (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge) && !declaration.ActiveEntryHeaders.Any();
			if (needMerge)
			{
				var performMergeResult = PerformMerge();
				if (performMergeResult)
				{
					declaration.Factory.Save();
				}
				return performMergeResult;
			}

			return true;
		}
		return false;
	}

	protected override bool DisplayGenerateEntriesMenuOption => true;
	ZMenuItem sendToCustomsMenuItem;
	ZMenuItem declarationsMenuItem;
	ZMenuItem serviceMessagesMenuItem;
	ZMenuItem vatDefermentMenuItem;
	ZMenuItem dutyGuaranteeMenuItem;
	ZMenuItem guaranteeAmountMenuItem;
	ZMenuItem portMessagingMenuItem;
	ZMenuItem cargonautExportMenuItem;
	ZMenuItem portbaseExportMenuItem;
	ZMenuItem portbaseImportMenuItem;
	ZMenuItem amendDeclarationMenuItem;
	ZMenuItem amendmentRequestMenuItem;
	ZMenuItem responseToRfiSentMenuItem;
	ZMenuItem supplementaryDeclarationMenuItem;
	ZMenuItem cancelSupplementaryDeclarationMenuItem;
	ZMenuItem cancelAmendDeclarationMenuItem;
	ZMenuItem cancelResponseToRFISentMenuItem;
	protected MessageSendingForm GetMessageSendingForm(JobDeclarationMessageSendingObjectParent decWrapper) => new MessageSendingForm(decWrapper);
	EntrySelectionForm GetEntrySelectionForm(ResourceStringData formCaption) => new EntrySelectionForm(Declaration, formCaption);

	public static class Constants
	{
		public static MultilingualString SendToCustomsCaption => ResString.GetMultilingualString("498DCACB-3A97-43EC-B889-389468C2E11D", "Send to Customs");
		public static MultilingualString SendToCustomsDeclarationsCaption => ResString.GetMultilingualString("5EC0F80D-EE1E-4BEE-8308-83EC2D9CB479", "Declarations");
		public static MultilingualString ServiceMessagesCaption => ResString.GetMultilingualString("51346176-44DD-4854-A6C8-9A0635F38C38", "Service messages");
		public static MultilingualString DutyGuaranteeCaption => ResString.GetMultilingualString("A77C63D1-2717-4219-9D50-2B7E9A13281A", "Duty guarantee amount check");
		public static MultilingualString VatDefermentCaption => ResString.GetMultilingualString("29B2531F-8B13-45CC-9264-FD02669BB172", "VAT (Article 23) deferment check");
		public static MultilingualString GuaranteeAmountCaption => ResString.GetMultilingualString("C3838B9E-FE32-4F91-914E-DFD8037EF4DB", "Check Guarantee amount left");
		public static MultilingualString PortMessagingCaption => ResString.GetMultilingualString("12345678-ABCD-4321-DCBA-87654321ABCD", "Port Messaging");
		public static MultilingualString CargonautExportCaption => ResString.GetMultilingualString("22334455-AABB-6677-8899-1234567890AB", "Cargonaut - 750/755");
		public static MultilingualString PortbaseExportCaption => ResString.GetMultilingualString("33445566-BBAA-7788-9900-223344556677", "Portbase - Export documentation (MED)");
		public static MultilingualString PortbaseImportCaption => ResString.GetMultilingualString("44556677-AABB-8899-0011-334455667788", "Portbase - Import documentation (MID)");
		public static MultilingualString AmendDeclarationCaption => ResString.GetMultilingualString("A1B2C3D4-E5F6-7890-ABCD-1234567890AB", "Amend Declaration");
		public static MultilingualString AmendmentRequestCaption => ResString.GetMultilingualString("22334455-7788-9900-1122-334455667788", "Amendment Request");
		public static MultilingualString ResponseToRfiSentCaption => ResString.GetMultilingualString("33445566-8899-1122-3344-5566778899BB", "Response to RFI Sent by Customs");
		public static MultilingualString SupplementaryDeclarationCaption => ResString.GetMultilingualString("55667788-99AA-BBCC-DDEE-112233445566", "Supplementary Declaration");
		public static MultilingualString CancelSupplementaryDeclarationCaption => ResString.GetMultilingualString("E86C1C08-C8BB-426B-A0D2-D0C14F99A2F6", "Cancel Supplementary Declaration");
		public static MultilingualString CancelAmendRequestCaption => ResString.GetMultilingualString("C5A2E99D-CF44-46DC-8A88-AD17FC556245", "Cancel Amendment Request");
		public static MultilingualString CancelResponseToRFISentCaption => ResString.GetMultilingualString("51AE9445-CD7B-4473-935E-4E380FA9471E", "Cancel Response To RFI Sent");
	}
}
