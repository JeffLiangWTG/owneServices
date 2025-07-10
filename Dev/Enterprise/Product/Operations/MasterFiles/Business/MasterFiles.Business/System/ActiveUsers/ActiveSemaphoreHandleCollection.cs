using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveSemaphoreHandleCollection : NonPersistentBusinessObjectCollection<ActiveSemaphoreHandle>
	{
		public ActiveSemaphoreHandleCollection(BusinessObjectFactory factory, ActiveUser parent) : base(factory)
		{
			Parent = parent;
		}

		public ActiveUser Parent { get; private set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ActiveSemaphoreHandle(Factory, Parent);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
