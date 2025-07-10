using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public abstract class MessageSendingActionCollectionBase<T1, T2> : NonPersistentBusinessObjectCollection<T1> where T1 : NonPersistentBusinessObject, IMessageSendingActionBase
		where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected MessageSendingActionCollectionBase(DISHostWrapperBase<T2> hostWrapper)
			: this(hostWrapper.Factory)
		{
			PopulateCollection(hostWrapper.DISDocuments);
		}

		protected MessageSendingActionCollectionBase(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		void PopulateCollection(DISDocumentCollectionBase<T2> disDocuments)
		{
			foreach (IDISDocumentBase disDocument in disDocuments)
			{
				var element = CreateElement(disDocument);
				Add(element as T1);

				element.Send = disDocument.Status.IsEmpty;
			}
		}

		public void SendMessages()
		{
			foreach (IMessageSendingActionBase action in this)
			{
				if (action.Send || action.SendWithdrawal)
				{
					var manager = GetMessageManager(action.DisDocument);
					if (action.SendWithdrawal)
					{
						manager.SendWithdrawl();
					}
					else
					{
						manager.SendSubmission();
					}
				}
			}
		}

		protected abstract IMessageSendingActionBase CreateElement(IDISDocumentBase disDocument);

		protected abstract DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
