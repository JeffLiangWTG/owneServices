using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public abstract class CurrentCompanyJobDeclarationFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected CurrentCompanyJobDeclarationFilterStripBusinessObject()
		{
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(GetCompanyQuery());
				return result;
			}
		}

		ZQuery GetCompanyQuery()
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(companySubQuery, JoinCondition.And);
			return result;
		}
	}
}
