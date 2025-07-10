
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccBankAccount)]
	public class AccBankAccountCollection : BusinessObjectCollection<AccBankAccount>, IAccBankAccountCollection
	{
		public AccBankAccountCollection(BusinessObjectFactory factory, GlbBranch branch, ZQuery filter) : this(factory, GetBranchFilter(branch, filter))
		{
		}

		public AccBankAccountCollection(BusinessObjectFactory factory, GlbBranch branch) : base(factory, GetBranchFilter(branch))
		{
		}

		public AccBankAccountCollection(BusinessObjectFactory factory, GlbCompany company) : base(factory, GetCompanyFilter(company))
		{
		}

		public AccBankAccountCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery filter)
			: base(factory, GetCompanyFilter(company, filter))
		{
		}

		public AccBankAccountCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccBankAccountCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected static ZQuery GetBranchFilter(GlbBranch branch)
		{
			return GetBranchFilter(branch, new ZQuery());
		}

		protected static ZQuery GetBranchFilter(GlbBranch branch, ZQuery extraFilter)
		{
			ZQuery filter1 = new ZQuery(AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, Env.CurrentCompany.PK);
			ZQuery filter2 = new ZQuery(AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);
			ZQuery filter = new ZQuery(filter1, JoinCondition.And, filter2);

			ZQuery filter3;
			if (branch != null)
			{
				filter3 = new ZQuery(AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, branch.PK);
			}
			else
			{
				filter3 = new ZQuery(AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, ZGuid.NewZGuid());
			}

			ZQuery result = new ZQuery(filter, JoinCondition.Or, filter3);
			result.AddToFilter(extraFilter);
			return result;
		}

		protected static ZQuery GetCompanyFilter(GlbCompany company)
		{
			return GetCompanyFilter(company, new ZQuery());
		}

		protected static ZQuery GetCompanyFilter(GlbCompany company, ZQuery extraFilter)
		{
			ZGuid filterGuid = company == null ? ZGuid.NewZGuid() : company.PK;
			ZQuery filter = new ZQuery(AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, filterGuid);
			filter.AddToFilter(extraFilter);
			return filter;
		}
	}
}
