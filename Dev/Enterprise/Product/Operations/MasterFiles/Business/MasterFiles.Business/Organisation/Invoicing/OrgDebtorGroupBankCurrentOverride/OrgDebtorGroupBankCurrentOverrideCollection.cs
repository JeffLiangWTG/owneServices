using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankCurrentOverrideCollection : DependentBusinessObjectCollection<OrgDebtorGroupBankCurrentOverride, OrgDebtorGroupBankDefault>
	{
		public OrgDebtorGroupBankCurrentOverrideCollection(OrgDebtorGroupBankDefault master)
			: base(master)
		{
		}

		public OrgDebtorGroupBankCurrentOverrideCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			bizOAdded[OrgDebtorGroupBankCurrentOverrideSchema.PB_P6] = Master.PK;
		}
	}
}
