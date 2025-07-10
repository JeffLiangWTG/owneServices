using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceGroupHeaderLookups : Customs.Business.JobComInvoiceGroupHeaderLookups
	{
		public JobComInvoiceGroupHeaderLookups(JobComInvoiceGroupHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceGroupHeader Invoice
		{
			get { return Parent; }
		}

		protected new JobComInvoiceGroupHeader Parent
		{
			get { return (JobComInvoiceGroupHeader)base.Parent; }
		}

		public override CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<TWJobMessageTypeList>();
	}
}
