using System;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	abstract class CargoControlAndTransitCommand : ICommand, INotifiableDocumentInfoCreated
	{
		protected IDocumentInfo documentInfo;

		public abstract string Id { get; }
		public abstract string Caption { get; }
		public virtual object Image { get; }
		public virtual bool IsEnabled => documentInfo != null && !hasMessageBeenSent;
		public virtual bool IsVisible => true;

		public abstract bool Invoke();

		public bool Invoke(MacroMap parameters) => Invoke();

		bool hasMessageBeenSent;

		#region INotifiableDocumentInfoCreated members

		public void NotifyDocumentInfoCreated(IDocumentInfo info)
		{
			if (info == null
				|| documentInfo != null)
			{
				return;
			}

			documentInfo = info;

			var services = documentInfo.Services;
			var broker = services.Resolve<IEventBroker>();

			broker.GetEvent<MessageSentEvent>().Subscribe(_ => hasMessageBeenSent = true);
			broker.GetEvent<MessageWithdrawalSentEvent>().Subscribe(_ => hasMessageBeenSent = true);
		}

		#endregion

		protected IDocument Document => documentInfo?.Document;

		protected IEDocsDeliveryParameters CreateEDocsDeliveryParameters(IDocumentDescriptor descriptor, IEDocsInstructions eDocsInstructions)
		{
			return new EDocsDeliveryParameters
			{
				DocumentName = descriptor.Name,
				DocumentTitle = descriptor.PrintInstructions?.Title,
				DocumentType = descriptor.DocumentType,
				AttachedFileName = descriptor.Name,
				BusinessObject = eDocsInstructions.Parent as IBusiness
			};
		}

		protected bool CheckAllowSendMessage()
		{
			var security = documentInfo?.Services?.Resolve<IDocumentSecurityService>();

			if (!security?.CanSendMessage ?? false)
			{
				security.ShowSendMessageError();
				return false;
			}

			return true;
		}

		protected bool CheckHasNoChanges(out string errorMessage)
		{
			if ((Document?.Data?.HasChanges ?? false) || (documentInfo?.DocumentData?.HasChanges ?? false))
			{
				errorMessage = Res.GetString("B42BE2B4-60FB-4C3A-8059-635EE462E4F1", "Please save changes before sending message.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckHasNoErrors(out string errorMessage)
		{
			if (Document?.HasErrors() ?? true)
			{
				errorMessage = Res.GetString("F2B22BA8-933F-4A93-8E2B-8A254B817907", "This document contains errors. Please fix all errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected bool CheckHasNoMessageErrors(out string errorMessage)
		{
			if (Document?.HasMessageErrors() ?? true)
			{
				errorMessage = Res.GetString("861E36BA-B0BC-4619-9667-EE2E9FBDF8C7", "This document contains message errors. Please fix all message errors before sending.");
				return false;
			}

			errorMessage = null;
			return true;
		}

		protected void ShowMessage(string message, string caption)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				var notificationService = documentInfo?.Services?.Resolve<IUserNotificationService>();
				notificationService?.ShowMessage(message, caption);
			}
		}
	}
}
