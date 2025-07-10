using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.Business
{
	public interface IJobDeclarationMessageSendingObjectParent
	{
		BaseJobDeclaration ParentDeclaration { get; }
		IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties { get; }
		IEnumerable<BaseMessageSendingObject> SendingObjectsCollection { get; }
		ZString BizObjValidationMessageErrors { get; }
		ZString AdditionalWarnings { get; }

		event EventHandler SelectedSendingObjectsChanged;
	}

	public class JobDeclarationMessageSendingObjectParent<T> : BaseMessageSendingObjectParent<T>, IJobDeclarationMessageSendingObjectParent where T : JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObjectParent(BaseJobDeclaration declaration) : base(declaration.Factory)
		{
			ParentDeclaration = declaration;
		}

		#region Implementation of IJobDeclarationMessageSendingObjectParent

		public readonly BaseJobDeclaration ParentDeclaration;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
		{
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageType, true, 80),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationType, true, 100),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryStatus, true, 100)
		};

		#endregion

		protected override NonPersistentBusinessObjectCollection<T> GetSendingObjectsCollectionCore()
		{
			var result = new JobDeclarationMessageSendingObjectCollection<T>(Factory);
			foreach (var header in GetEntryHeadersToBeSent())
			{
				result.Add(CreateNewJobDeclarationMessageSendingObject(header));
			}
			return result;
		}

		protected virtual IEnumerable<CusEntryHeader> GetEntryHeadersToBeSent() => ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>();

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return CreateNewNotificationCollector().GetMessageErrors();
		}

		protected JobDeclarationMessageSendingNotificationCollector CreateNewNotificationCollector() =>  new(ParentDeclaration, SelectedSendingObjects.Cast<JobDeclarationMessageSendingObject>().Select(x => x.Header));

		protected virtual JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(CusEntryHeader header)
		{
			return new JobDeclarationMessageSendingObject(header);
		}

		public override BusinessObject TopLevelBusinessObject => ParentDeclaration;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		BaseJobDeclaration IJobDeclarationMessageSendingObjectParent.ParentDeclaration => ParentDeclaration;

		IEnumerable<BaseMessageSendingObject> IJobDeclarationMessageSendingObjectParent.SendingObjectsCollection => this.SendingObjectsCollection.Cast<BaseMessageSendingObject>();
	}
}
