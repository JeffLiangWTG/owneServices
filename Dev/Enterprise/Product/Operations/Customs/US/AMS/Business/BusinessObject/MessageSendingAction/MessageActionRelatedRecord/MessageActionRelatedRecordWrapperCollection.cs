using System.Collections;
using System.Collections.Generic;

using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public enum MessageAttacheeActionType { Added, Deleted }

	public delegate void MessageAttacheesAddedOrDeleted(IMessageAttacheeInHeader msgAttachee, MessageAttacheeActionType actionType);

	public class MessageAttacheesAddedOrDeletedEvent : IService
	{
		protected MessageAttacheesAddedOrDeletedEvent(BusinessObjectFactory factory)
		{
		}

		public static void AddMessageAttacheesService(BusinessObjectFactory factory, MessageAttacheesAddedOrDeleted handler)
		{
			var instance = factory.ServiceContainer.GetService<MessageAttacheesAddedOrDeletedEvent>();

			if (instance == null)
			{
				instance = new MessageAttacheesAddedOrDeletedEvent(factory);
				factory.ServiceContainer.AddService(instance);
			}

			instance.onMessageAttacheeAddedOrDeleted += handler;
		}

		public static void InvokeMessageAttacheeAddedOrDeletedService(BusinessObjectFactory factory, IMessageAttacheeInHeader msgAttachee, MessageAttacheeActionType actionType)
		{
			var instance = factory.ServiceContainer.GetService<MessageAttacheesAddedOrDeletedEvent>();

			if (instance != null && instance.onMessageAttacheeAddedOrDeleted != null)
			{
				instance.onMessageAttacheeAddedOrDeleted(msgAttachee, actionType);
			}
		}

		MessageAttacheesAddedOrDeleted onMessageAttacheeAddedOrDeleted;
	}

	public delegate IEnumerable<IMessageAttacheeInHeader> GetMessageAttacheesToBuild();

	public class MessageActionRelatedRecordWrapperCollection : NonPersistentBusinessObjectCollection<MessageActionRelatedRecordWrapper>
	{
		public MessageActionRelatedRecordWrapperCollection(IMessageActionHeader actionHeader, GetMessageAttacheesToBuild getMessageAttachees)
			: base(actionHeader.Factory)
		{
			this.actionHeader = actionHeader;
			this.getMessageAttachees = getMessageAttachees;
			MessageAttacheesAddedOrDeletedEvent.AddMessageAttacheesService(Factory, OnMessageAttacheeAddedOrDeleted);
			ReBuild();
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

		public void ReBuild()
		{
			RemoveAll();

			foreach (var relatedRecord in getMessageAttachees())
			{
				Add(new MessageActionRelatedRecordWrapper(relatedRecord));
			}

			Sort(new Sorter());
		}

		public MessageActionRelatedRecordWrapper GetElementWrapping(IMessageAttacheeInHeader header)
		{
			foreach (MessageActionRelatedRecordWrapper wrapper in this)
			{
				if (wrapper.relatedRecord == header)
				{
					return wrapper;
				}
			}
			return null;
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#region Implementation

		void OnMessageAttacheeAddedOrDeleted(IMessageAttacheeInHeader msgAttachee, MessageAttacheeActionType actionType)
		{
			var wrapper = GetElementWrapping(msgAttachee);

			if (actionType == MessageAttacheeActionType.Added)
			{
				if (wrapper == null && new List<IMessageAttacheeInHeader>(getMessageAttachees()).Contains(msgAttachee))
				{
					Add(new MessageActionRelatedRecordWrapper(msgAttachee));
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

		internal void RebuildIfNecessary(IMessageAttacheeInHeader msgAttachee)
		{
			var wrapper = GetElementWrapping(msgAttachee);

			if (wrapper == null)
			{
				RemoveAll();

				ReBuild();
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var messageAttachee = actionHeader.MessageAttachees.Count > 0 ? actionHeader.MessageAttachees[0] : null;
			return new MessageActionRelatedRecordWrapper(messageAttachee);
		}

		#endregion

		#region Sorter

		public class Sorter : IComparer
		{
			#region IComparer Members

			int IComparer.Compare(object x, object y)
			{
				var result = 0;

				string s1 = ((MessageActionRelatedRecordWrapper)x).RecordTypeDescription;
				string s2 = ((MessageActionRelatedRecordWrapper)y).RecordTypeDescription;

				if (s1 != s2)
				{
					if (s1 == SubApplicationCodeList.Descriptions.AMS || (s1 == SubApplicationCodeList.Descriptions.PermitToTransfer && s2 == SubApplicationCodeList.Descriptions.MasterInBond))
					{
						result = -1;
					}
					else if (s2 == SubApplicationCodeList.Descriptions.AMS || (s2 == SubApplicationCodeList.Descriptions.PermitToTransfer && s1 == SubApplicationCodeList.Descriptions.MasterInBond))
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
