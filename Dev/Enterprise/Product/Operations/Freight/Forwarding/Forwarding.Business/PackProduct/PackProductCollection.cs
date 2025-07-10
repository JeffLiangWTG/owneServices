using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackProductCollection : ActiveBusinessObjectCollection<PackProduct>
	{
		public PackProductCollection(ForwardingPackLine master)
			: base(master)
		{
		}

		#region Multiple Item Managers

		public MultipleItemManager PackProductManager
		{
			get { return new MultipleItemManager(this, JobPackProductSchema.D2_ProductCode, true); }
		}

		#endregion

		#region Lookups

		public OrgSupplierPartCollection OrgSupplierPartProductCodes
		{
			get { return new OrgSupplierPartCollection(Factory); }
		}

		#endregion
	}
}
