using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<MessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType)
		: this(declaration, messageType, false)
		{
		}

		protected JobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, ZBool includeControllingMessageInformation)
		: base(declaration)
		{
			MessageType = messageType;
			switch (messageType)
			{
				case MessageTypeList.Codes.IEA:
					getMessageSendingObjectForHeader = h => new N5167.N5167MessageSendingObject(h);
					break;
				case MessageTypeList.Codes.ECD:
					getMessageSendingObjectForHeader = h => new N5203.N5203MessageSendingObject(h);
					break;
				case MessageTypeList.Codes.ICD:
					getMessageSendingObjectForHeader = h => new NX5105MessageSendingObject(h, includeControllingMessageInformation);
					break;
				case MessageTypeList.Codes.CAA:
					getMessageSendingObjectForHeader = h => new NX5105MessageSendingObject(h, false, true);
					break;
				case MessageTypeList.Codes.ADM:
					getMessageSendingObjectForHeader = h => new NX5901MessageSendingObject(h);
					break;
				default:
					getMessageSendingObjectForHeader = h => null;
					break;
			}
			declaration.LoadChildEditableObjects();
			declaration.RunPreSaveValidation();
		}

		readonly Func<CusEntryHeader, MessageSendingObject> getMessageSendingObjectForHeader;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			if (sendingObjectsCollection == null)
			{
				sendingObjectsCollection = new MessageSendingObjectCollection(Factory);
				foreach (var header in ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>())
				{
					var sendingObject = getMessageSendingObjectForHeader?.Invoke(header);
					if (sendingObject != null)
					{
						sendingObjectsCollection.Add(sendingObject);
					}
				}
			}
			RegisterEditableChildObject(sendingObjectsCollection);
			return sendingObjectsCollection;
		}

		MessageSendingObjectCollection sendingObjectsCollection;

		public bool UpdateEntryInstructionDataForDutyIfNeeded()
		{
			bool result = true;
			if (SendingObjectsCollection.Cast<MessageSendingObject>().Any(x => x.ShouldSend && x.Action == ActionCodeList.Codes.Create && x.Header.EntryInstruction != null))
			{
				ParentDeclaration?.DoMerge();
				try
				{
					Factory.Save();
				}
				catch (ZSaveException e)
				{
					result = false;
					ZExceptionReporting.HandleSaveException(e);
				}
			}
			return result;
		}

		public readonly ZString MessageType;

		protected override ZString GetBizObjValidationMessageErrors() => GetNotificationsMessage(() => TWMessageSendingValidation.New(this, null, false).CheckBusinessObjectLevelValidation().NotificationsAsString());

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(WarningsMessage.ToUniqueMessageListString);

		ZString GetNotificationsMessage(Func<string> getNotificationsMessage)
		{
			var result = string.Empty;
			if (SelectedSendingObjects.Any())
			{
				var decIsTopLevel = ParentDeclaration.IsTopLevel;
				try
				{
					ParentDeclaration.IsTopLevel = false;
					RegisterEditableChildObject(ParentDeclaration);
					SendingObjectsCollection.Cast<MessageSendingObject>().ForEach(c => c.RunPreSaveValidation());
					result = Regex.Replace(getNotificationsMessage(), "(?<!\r)\n", "\r\n");
				}
				finally
				{
					ParentDeclaration.IsTopLevel = decIsTopLevel;
					UnRegisterEditableChildObject(ParentDeclaration);
				}
			}
			return result;
		}

		public virtual ZBool AllowSendWithError
		{
			get => fAllowSendWithError;
			set => SetNonPersistentPropertyValue(AllowSendWithErrorInfo, ref fAllowSendWithError, value);
		}
		ZBool fAllowSendWithError;

		public virtual ZPropertyInfo AllowSendWithErrorInfo => GetZPropertyInfo(nameof(AllowSendWithError));

		IEnumerable<INotification> WarningsMessage => new CustomsNotificationCollector(this, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings();

		public ZString MenuCaption { get; set; }
	}
}
