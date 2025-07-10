using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ModuleCusWHSOperatorTransactionsCollection : ActiveBusinessObjectCollection<CusWHSOperatorTransaction>
	{
		public ModuleCusWHSOperatorTransactionsCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory)
		{
			this.company = company;
		}

		readonly GlbCompany company;

		protected override ZQuery CreateRelationshipFilter()
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(CusWHSOperatorTransactionBatch), CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch);
			companyQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_GC_Company, company.PK);

			var result = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
			result.AddSubQuery(companyQuery, JoinCondition.And);
			return result;
		}

		protected override bool AllowNew => false;

		public override void Delete(CusWHSOperatorTransaction businessObject) => throw new NotSupportedException();
	}
}
