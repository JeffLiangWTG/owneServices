using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ModuleCusEntryPayInfoCollection : BusinessObjectCollection<CusEntryPayInfo>
	{
		public ModuleCusEntryPayInfoCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			query.AddToFilter(CusEntryPayInfoSchema.C9_TransactionType, UniversalReferenceConstants.TaxOrFeeTypeCode.VAT);
			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentDate, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentStatus, CusEntryPayInfoStatusList.Codes.Clear);
			var sub = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey);
			sub.AddToFilter(JobDeclarationSchema.JE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, sub, JoinCondition.And);
			return query;
		}
	}
}
