using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankDefaultCollection : DependentBusinessObjectCollection<OrgDebtorGroupBankDefault, OrgDebtorGroup>
	{
		public OrgDebtorGroupBankDefaultCollection(OrgDebtorGroup master, GlbCompany company)
			: base(master)
		{
			this.Company = company;
		}

		readonly GlbCompany Company;

		public OrgDebtorGroupBankDefaultCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (Company != null)
			{
				result.AddToFilter(JoinCondition.And, OrgDebtorGroupBankDefaultSchema.P6_GC, SQLComparisonOperator.Equal, Company.PK);
			}
			return result;
		}
	}
}
