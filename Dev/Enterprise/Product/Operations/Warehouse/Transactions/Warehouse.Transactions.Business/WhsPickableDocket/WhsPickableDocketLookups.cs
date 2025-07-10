using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickableDocketLookups : WhsDocketLookups
	{
		protected WhsPickableDocketLookups(WhsPickableDocket parent)
			: base(parent)
		{
		}

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get => ((WhsPickableDocket)Parent).GetCarrierServiceLevels() ?? base.CarrierServiceLevels;
		}
	}
}
