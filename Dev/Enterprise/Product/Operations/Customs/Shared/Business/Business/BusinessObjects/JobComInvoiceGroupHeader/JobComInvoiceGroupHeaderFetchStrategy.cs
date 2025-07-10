using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class JobComInvoiceGroupHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobComInvoiceGroupHeaderFetchStrategy(BaseJobComInvoiceGroupHeader groupHeader)
			: base(groupHeader)
		{
		}

		BaseJobComInvoiceGroupHeader groupHeader
		{
			get { return BusinessObject as BaseJobComInvoiceGroupHeader; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobDeclaration), groupHeader.JZ_JE);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, BusinessObject.PK);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, BusinessObject.PK);
		}
	}
}
