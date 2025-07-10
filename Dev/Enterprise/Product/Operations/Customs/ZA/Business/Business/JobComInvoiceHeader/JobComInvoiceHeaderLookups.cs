using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent as JobComInvoiceHeader; }
		}

		public override ICodeDescriptionPairList ValuationCodeList
		{
			get { return Factory.GetCachedValue<MasterFiles.Business.Customs.ZA.ValuationCodeList>(); }
		}

		public ICodeDescriptionPairList PaymentTermsList => Factory.GetCachedValue<PaymentTermsList>();

		public ICodeDescriptionPairList ROOTypesList
		{
			get { return ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, ZDateTime.Today); }
		}
	}
}
