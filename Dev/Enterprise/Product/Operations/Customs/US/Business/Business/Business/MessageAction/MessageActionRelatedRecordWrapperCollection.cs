using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public enum MessageAttacheeActionType { Added, Deleted }

	public delegate void MessageAttacheesAddedOrDeleted(IMessageAttacheeInDeclaration msgAttachee, MessageAttacheeActionType actionType);

	public class MessageAttacheesAddedOrDeletedEvent : IService
	{
		protected MessageAttacheesAddedOrDeletedEvent(BusinessObjectFactory factory)
		{
		}

		public static void AddMessageAttacheesService(BusinessObjectFactory factory, MessageAttacheesAddedOrDeleted handler)
		{
			MessageAttacheesAddedOrDeletedEvent instance = factory.ServiceContainer.GetService<MessageAttacheesAddedOrDeletedEvent>();

			if (instance == null)
			{
				instance = new MessageAttacheesAddedOrDeletedEvent(factory);
				factory.ServiceContainer.AddService(instance);
			}

			instance.onMessageAttacheeAddedOrDeleted += handler;
		}

		public static void InvokeMessageAttacheeAddedOrDeletedService(BusinessObjectFactory factory, IMessageAttacheeInDeclaration msgAttachee, MessageAttacheeActionType actionType)
		{
			MessageAttacheesAddedOrDeletedEvent instance = factory.ServiceContainer.GetService<MessageAttacheesAddedOrDeletedEvent>();

			if (instance != null && instance.onMessageAttacheeAddedOrDeleted != null)
			{
				instance.onMessageAttacheeAddedOrDeleted(msgAttachee, actionType);
			}
		}

		MessageAttacheesAddedOrDeleted onMessageAttacheeAddedOrDeleted;
	}

	public delegate IEnumerable<IMessageAttachee> GetMessageAttacheesToBuild(MessagesToShowCollection.MessagesStatus messageStatus);

	public class MessageActionRelatedRecordWrapperCollection : NonPersistentBusinessObjectCollection<MessageActionRelatedRecordWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MessageActionRelatedRecordWrapperCollection(IMessageActionHeader actionHeader, GetMessageAttacheesToBuild getMessageAttachees)
			: base(actionHeader.Factory)
		{
			this.actionHeader = actionHeader;
			this.getMessageAttachees = getMessageAttachees;
			MessageAttacheesAddedOrDeletedEvent.AddMessageAttacheesService(Factory, OnMessageAttacheeAddedOrDeleted);
			Populate();
		}

		readonly GetMessageAttacheesToBuild getMessageAttachees;
		readonly IMessageActionHeader actionHeader;

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		#endregion

		public void ReBuild(MessagesToShowCollection.MessagesStatus messageStatus)
		{
			RemoveAll();

			foreach (IMessageAttacheeInDeclaration relatedRecord in getMessageAttachees(messageStatus))
			{
				Add(new MessageActionRelatedRecordWrapper(relatedRecord, messageStatus));
			}

			Sort(new Sorter());
		}

		public MessageActionRelatedRecordWrapper GetElementWrapping(IMessageAttachee header, bool isForBIRD = false)
		{
			var messageAttachee = GetMessageAttachee(header, isForBIRD);

			foreach (MessageActionRelatedRecordWrapper wrapper in this)
			{
				if (wrapper.relatedRecord == messageAttachee)
				{
					return wrapper;
				}
			}
			return null;
		}

		static IMessageAttachee GetMessageAttachee(IMessageAttachee messageAttachee, bool isForBIRD)
		{
			var result = messageAttachee;

			if (isForBIRD)
			{
				var messageAttacheeInDec = messageAttachee as IMessageAttacheeInDeclaration;
				if (messageAttacheeInDec != null)
				{
					var declaration = messageAttachee.Factory.Load<JobDeclaration>(messageAttacheeInDec.DeclarationPK);
					if (declaration != null)
					{
						result = declaration.BIRDMessageAttachee;
					}
				}
			}
			return result;
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#region Implementation

		void OnMessageAttacheeAddedOrDeleted(IMessageAttacheeInDeclaration msgAttachee, MessageAttacheeActionType actionType)
		{
			var wrapper = GetElementWrapping(msgAttachee, isForBIRD: false);//MessageAttachee for BIRD messages is not to be removed once a job has BIRD messages. Add is handled by RebuildIfNecessary

			if (actionType == MessageAttacheeActionType.Added)
			{
				if (wrapper == null && new List<IMessageAttachee>(getMessageAttachees(MessagesToShowCollection.MessagesStatus.ActiveOnly)).Contains(msgAttachee))
				{
					Add(new MessageActionRelatedRecordWrapper(msgAttachee, MessagesToShowCollection.MessagesStatus.ActiveOnly));
					Sort(new Sorter());
				}
			}
			else
			{
				if (wrapper != null)
				{
					Remove(wrapper);
				}
			}
		}

		internal void RebuildIfNecessary(IMessageAttachee msgAttachee, bool isForBIRD)
		{
			MessageActionRelatedRecordWrapper wrapper = GetElementWrapping(msgAttachee, isForBIRD);

			if (wrapper == null)
			{
				RemoveAll();

				Populate();
			}
		}

		void Populate()
		{
			ReBuild(MessagesToShowCollection.MessagesStatus.ActiveOnly);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var messageAttachee = actionHeader.MessageAttachees.Count > 0 ? actionHeader.MessageAttachees[0] : null;
			return new MessageActionRelatedRecordWrapper(messageAttachee, MessagesToShowCollection.MessagesStatus.ActiveOnly);
		}

		#endregion

		#region Sorter

		public class Sorter : IComparer
		{
			#region IComparer Members

			int IComparer.Compare(object x, object y)
			{
				int result = 0;

				string s1 = ((MessageActionRelatedRecordWrapper)x).RecordTypeDescription;
				string s2 = ((MessageActionRelatedRecordWrapper)y).RecordTypeDescription;

				if (s1 == s2)
				{
					s1 = ((MessageActionRelatedRecordWrapper)x).HumanFriendlyReference;
					s2 = ((MessageActionRelatedRecordWrapper)y).HumanFriendlyReference;
					result = s1.CompareTo(s2);
				}
				else
				{
					if (s1 == MessageAttacheeRecordTypeDescriptions.CargoRelease)
					{
						result = -1;
					}
					else if (s2 == MessageAttacheeRecordTypeDescriptions.CargoRelease)
					{
						result = 1;
					}
					else if (s1 == MessageAttacheeRecordTypeDescriptions.Entry)
					{
						result = -1;
					}
					else if (s2 == MessageAttacheeRecordTypeDescriptions.Entry)
					{
						result = 1;
					}
					else if (s1 == MessageAttacheeRecordTypeDescriptions.InBond)
					{
						result = -1;
					}
					else if (s2 == MessageAttacheeRecordTypeDescriptions.InBond)
					{
						result = 1;
					}
					else if (s1 == MessageAttacheeRecordTypeDescriptions.ElectronicInvoice)
					{
						result = -1;
					}
					else if (s2 == MessageAttacheeRecordTypeDescriptions.ElectronicInvoice)
					{
						result = 1;
					}
					else
					{
						result = s1.CompareTo(s2);
					}
				}

				return result;
			}

			#endregion
		}

		#endregion
	}
}
