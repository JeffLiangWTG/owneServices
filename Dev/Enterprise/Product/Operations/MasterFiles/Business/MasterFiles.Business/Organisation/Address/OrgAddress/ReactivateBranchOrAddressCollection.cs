using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ReactivateBranchOrAddressCollection : NonPersistentBusinessObjectCollection<ReactivateBranchOrAddressItem>
	{
		public ReactivateBranchOrAddressCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReactivateBranchOrAddressItem();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
