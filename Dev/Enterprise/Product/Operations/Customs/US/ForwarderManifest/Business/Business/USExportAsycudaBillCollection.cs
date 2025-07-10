using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillCollection : AsycudaBillCollection<USExportAsycudaBill, USExportAsycudaManifestHeader>
	{
		public USExportAsycudaBillCollection(USExportAsycudaManifestHeader master)
			: base(master, new ZQuery(AsycudaBillSchema.PK, SQLComparisonOperator.NotEqual, master.MasterBill.PK))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Master is USExportAsycudaManifestHeader master)
			{
				var bill = (USExportAsycudaBill)child;
				bill.ABL_BolType = GetDefaultBolType(master);
			}
		}

		ZString GetDefaultBolType(USExportAsycudaManifestHeader master)
		{
			var result = AsycudaBill.ChildBolCode;
			if (!master.MasterBill.ABL_BillIssuer.IsEmpty && !master.MasterBOL.IsEmpty)
			{
				result = Core.Constants.ShipmentTypes.StandardHouse;
			}
			return result;
		}
	}
}
