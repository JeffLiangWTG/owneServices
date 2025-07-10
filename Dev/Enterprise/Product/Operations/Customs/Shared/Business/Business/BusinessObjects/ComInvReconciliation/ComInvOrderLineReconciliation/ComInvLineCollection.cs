using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class ComInvLineCollection : BusinessObjectCollection<ComInvOrderLineReconciliation>
	{
		public ComInvLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void CopyPersistentValuesToAnotherFactory(BusinessObjectFactory anotherFactory)
		{
			foreach (ComInvOrderLineReconciliation line in this)
			{
				line.CopyPersistentValuesToAnotherFactory(anotherFactory);
			}
		}
	}
}
