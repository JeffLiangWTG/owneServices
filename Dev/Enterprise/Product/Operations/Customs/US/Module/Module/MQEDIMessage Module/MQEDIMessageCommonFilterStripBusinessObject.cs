using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public abstract class MQEDIMessageCommonFilterStripBusinessObject : EDIMessageFilterBusinessObject
	{
		protected MQEDIMessageCommonFilterStripBusinessObject()
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
			var result = new ZDBOnlyQuery(typeof(MQEDIMessage));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), EDIMessageSchema.EM_GB);
			companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(companySubQuery, JoinCondition.And);
			return result;
		}
	}
}
