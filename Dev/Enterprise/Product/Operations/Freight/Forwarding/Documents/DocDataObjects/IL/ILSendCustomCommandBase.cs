using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	abstract class ILSendCustomCommandBase : ILCustomCommandBase
	{
		protected ILSendCustomCommandBase(IILElectronicMessageProvider messageProvider, bool isCancelActionTypeCode)
			: base(messageProvider)
		{
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		public override bool Invoke()
		{
			if (!CheckMessageValid())
			{
				return false;
			}

			if (!(documentInfo?.Document?.Data.Value is DocDataObject ilDocDataObject))
			{
				return false;
			}

			var messageBuilder = GetMessageBuilder(ilDocDataObject, isCancelActionTypeCode);
			var messageBuilderResult = messageBuilder?.PopulateMessages();

			if (!(messageBuilderResult?.IsSuccess ?? false))
			{
				var errors = messageBuilderResult?.GetBuilderResults()
					.SelectMany(result => result.Errors)
					.ToList();

				if (errors != null && errors.Count > 0)
				{
					var errorMessage = string.Join(System.Environment.NewLine, errors);
					ShowMessage(errorMessage);
				}
				else
				{
					ShowGenericErrorMessage();
				}

				return false;
			}

			var result = messageBuilderResult.GetBuilderResults().First();
			messageProvider.BusinessObject.Logs.AddNew(MessageSentEvent, GetEventParameters());

			messageProvider.Factory.Save();
			ShowMessage(MessageGeneratedCore());
			Notify(new DocumentHardRefreshEvent(documentInfo.Document));

			return true;
		}

		public override bool IsVisible => true;

		public override string Id => CommandIds.SendMessage;

		public override string Caption => CommandResources.Captions.SendMessage;

		public override bool IsEnabled
			=> CommandEnablerHelper.IsSendCustomCommandEnabled(messageProvider, DocumentName);

		protected bool CheckMessageValid()
		{
			var errorMessage = string.Empty;
			var check = CheckAllowSendMessage(out errorMessage)
				&& CheckHasNoChanges(out errorMessage)
				&& CheckHasNoErrors(out errorMessage)
				&& CheckHasNoMessageErrors(out errorMessage);

			if (!check && !string.IsNullOrEmpty(errorMessage))
			{
				ShowMessage(errorMessage);
			}

			return check;
		}

		protected bool CheckAllowSendMessage(out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!CheckAllowSendMessage())
			{
				return false;
			}

			return CheckBeforeInvoke(ref errorMessage);
		}

		protected virtual bool CheckBeforeInvoke(ref string errorMessage)
		{
			var messageExtensions = messageProvider.BusinessObject.GetSupporter().GetMessagingExtensions(documentInfo.Document, documentInfo.Descriptor?.MessageInstructions);
			var notifications = documentInfo.Services.Resolve<IUserNotificationService>();
			var canContinue = messageExtensions?.ContinueWithSendingMessage(notifications);
			if (canContinue == false)
			{
				errorMessage = Res.GetString("B3440DA2-3E9E-4BC3-9130-48A70CBD4424", "Due to changes in the state of the entity, please reopen the form before sending a message.");
				return false;
			}

			return true;
		}

		protected bool CheckHasNoChanges(out string errorMessage)
		{
			if (documentInfo?.Document?.Data?.HasChanges == null)
			{
				errorMessage = Res.GetString("80C0DFC0-FF40-4B7C-92CC-6CE05538FFDA", "Unable to process your request");
				return false;
			}
			else if (documentInfo.Document.Data.HasChanges)
			{
				errorMessage = Res.GetString("C2589056-73A0-4B27-87C2-8FE39D9CEBBD", "Please save changes before sending message.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckHasNoMessageErrors(out string errorMessage)
		{
			if (documentInfo.Document.HasMessageErrors())
			{
				errorMessage = Res.GetString("56A86B20-8B57-4C86-A932-346C2815416B", "This document contains message errors. Please fix all message errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected abstract string MessageGeneratedCore();

		protected abstract IMessageBuilder GetMessageBuilder(DocDataObject docDataObject, bool isCancelActionTypeCode);

		protected virtual Event MessageSentEvent { get; } = Events.MessageSent;

		protected virtual KeyValuePair<string, string>[] GetEventParameters()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary["DEP"] = ILMessageEventParameter.Department;
			dictionary["MST"] = DocumentName;
			return dictionary.ToArray();
		}

		readonly bool isCancelActionTypeCode;
	}
}
