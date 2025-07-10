using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingObjectCollection : NonPersistentBusinessObjectCollection<MessageSendingObject>
	{
		public MessageSendingObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void UnRegisterBillAsEditableChildObject()
		{
			foreach (MessageSendingObject obj in this)
			{
				obj.UnRegisterBillAsEditableChildObject();
			}
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
