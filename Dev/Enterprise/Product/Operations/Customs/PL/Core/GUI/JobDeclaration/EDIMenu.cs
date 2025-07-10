using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class EDIMenu : EU.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get => (JobDeclaration)base.Declaration;
		set => base.Declaration = value;
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();

		AddSendToCustomsMenu();
	}

	protected override EU.Business.Declaration.SingleLineEntryManager GetNewSingleLineEntryManager() => new SingleLineEntryManager(Declaration);

	protected override EU.GUI.SingleLineEntry.SingleLineEntryForm GetNewSingleLineEntryForm(EU.Business.Declaration.SingleLineEntryManager singleLineEntryManager) =>
		Declaration.IsImport
			? new ImportSingleLineEntryForm(singleLineEntryManager)
			: new SingleLineEntryForm(singleLineEntryManager);

	void AddSendToCustomsMenu()
	{
		sendCustomsDeclarationMenu = new ZMenuItem(Constants.SendCustomsDeclaration);
		MenuItems.Add(sendCustomsDeclarationMenu);

		AddCustomsDeclaration();
		AddRetrospectiveQuotaRequest();
	}

	void AddCustomsDeclaration()
	{
		customsDeclarationMenuItem = new ZMenuItem(Constants.CustomsDeclaration, CustomsDeclarationSendMessage_Click);
		sendCustomsDeclarationMenu.MenuItems.Add(customsDeclarationMenuItem);
	}

	void AddRetrospectiveQuotaRequest()
	{
		retrospectiveQuotaRequestMenuItem = new ZMenuItem(Constants.RetrospectiveQuotaRequest, RetrospectiveQuotaRequest_Click);
		sendCustomsDeclarationMenu.MenuItems.Add(retrospectiveQuotaRequestMenuItem);
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		sendCustomsDeclarationMenu.Visible = GetCustomsDeclarationMenuItemVisible();
		retrospectiveQuotaRequestMenuItem.Visible = GetRetrospectiveQuotaRequestMenuItemVisible();
	}

	bool GetRetrospectiveQuotaRequestMenuItemVisible() => Declaration is JobDeclaration declaration && declaration.IsImport && declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_ConcessionOrder.IsEmpty);

	bool GetCustomsDeclarationMenuItemVisible() => Declaration is JobDeclaration declaration && declaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin;

	public static class Constants
	{
		public static MultilingualString SendCustomsDeclaration => ResString.GetMultilingualString("PLJobDeclarationForm|Menu|SendCustomsDeclaration", "Send to Customs");
		public static MultilingualString CustomsDeclaration => ResString.GetMultilingualString("PLJobDeclarationForm|Menu|CustomsDeclaration", "Customs Declaration");
		public static MultilingualString RetrospectiveQuotaRequest => ResString.GetMultilingualString("PLJobDeclarationForm|Menu|RetrospectiveQuotaRequest", "Retrospective quota request");
	}

	void RetrospectiveQuotaRequest_Click(object sender, EventArgs e) => SendMessage_Click(RetrospectiveQuotaRequestMessageSendingObject.PLRetrospectiveSchema.ActionType);

	void CustomsDeclarationSendMessage_Click(object sender, EventArgs e) => SendMessage_Click(ZString.Empty);

	void SendMessage_Click(ZString formType)
	{
		var notifications = new NotificationsDecorator(GlobalNotificationsWrapper.Instance);
		new MessagePreSendingValidation(Declaration).Validate(notifications);
		if (!notifications.ReportedFatalError)
		{
			SendToCustoms(formType);
		}
	}

	void SendToCustoms(ZString formType)
	{
		var declaration = Declaration;
		var sendingMessageFactory = new BusinessObjectFactory();
		var messageSendingStrategy = GetMessageSendingStrategy(formType, declaration, sendingMessageFactory);

		if (messageSendingStrategy != null && RunPreSaveValidationIfHasNoChanges(declaration) && PreSaveDeclaration(declaration))
		{
			try
			{
				var messageParent = messageSendingStrategy.GetMessageParent();
				using (var form = messageSendingStrategy.GetSendingForm(messageParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var shouldSendObjects = messageParent.SendingObjectsCollection.Where(x => x.ShouldSend);
						foreach (var sendingObject in shouldSendObjects)
						{
							var messageSender = messageSendingStrategy.GetMessageSender(sendingObject, messageParent);
							messageSender.Send();
							sendingMessageFactory.Save();
							Globals.Message.Show(MessageSentSuccessfully);
						}

						if (messageParent.HasEDocs && messageSendingStrategy.GetAttachmentSender(messageParent) is AttachmentMessageSender attachmentSender)
						{
							attachmentSender.Send();
							sendingMessageFactory.Save();
							Globals.Message.Show(MessageSentSuccessfully);
						}
					}
				}
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errMsg = FormattableString.Invariant($"{MessageSendFailure}{System.Environment.NewLine}{ex.Message}");
				Globals.Message.Show(errMsg);
			}
		}
	}

	static MultilingualString MessageSentSuccessfully => ResString.GetMultilingualString("PL.MessageSendingResult|MessageSentSuccessfully", "Message sent successfully");

	static MultilingualString MessageSendFailure => ResString.GetMultilingualString("PL.MessageSendingResult|MessageSendFailure", "Failed to send message");

	BaseMessageSendingStrategy GetMessageSendingStrategy(ZString formType, JobDeclaration declaration, BusinessObjectFactory sendingMessageFactory)
	{
		BaseMessageSendingStrategy result = null;
		switch (formType)
		{
			case RetrospectiveQuotaRequestMessageSendingObject.PLRetrospectiveSchema.ActionType:
				result = new RetrospectiveQuotaRequestMessageSendingStrategy(sendingMessageFactory, declaration);
				break;
			default:
				if (Declaration.IsImport)
				{
					result = new ImportDeclarationMessageSendingStrategy(sendingMessageFactory, declaration);
				}
				else if (Declaration.IsExport)
				{
					result = new ExportDeclarationMessageSendingStrategy(sendingMessageFactory, declaration);
				}
				break;
		}

		return result;
	}

	ZMenuItem customsDeclarationMenuItem;
	ZMenuItem retrospectiveQuotaRequestMenuItem;
	ZMenuItem sendCustomsDeclarationMenu;
}
