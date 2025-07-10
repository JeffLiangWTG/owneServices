using CargoWise.Integration;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public override ICodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<RelationshipIndicatorList>();

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>();

		public override CodeDescriptionPairList MessageTypes => Factory.GetCachedValue<TWJobMessageTypeList>();
	}
}
