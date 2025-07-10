using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingMovementCollection : NonPersistentBusinessObjectCollection<MessageSendingMovement>
	{
		public MessageSendingMovementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SetToSend()
		{
			foreach (MessageSendingMovement obj in this)
			{
				obj.MM_Send = true;
			}
		}

		public void ReleaseAllInBondNumberMutex()
		{
			foreach (MessageSendingMovement sendingObject in this)
			{
				sendingObject.UnLockInBondNumberAllocationMutex();
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
