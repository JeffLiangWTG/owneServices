using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypes
		{
			get { return MessageTypeCodeList.GetListWithAdvanceShippingNotice(Factory); }
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		#region IncoTerm List

		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get { return Factory.GetCachedValue<UnitPriceTermTypeCodeList>(); }
		}

		#endregion
	}
}
