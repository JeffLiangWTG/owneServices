using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override ZString TaxOrFeeCodeListType => "VAT";

		public ZZRefCusCodeListCombinedCollection NatureOfTransactionList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, Parent.EffectiveAssessmentDate);

		public override ICodeDescriptionPairList PrimaryPreferenceList => Customs.Business.UniversalReferenceDataHelper.GetPreferenceListByCountry(Factory, Core.Constants.CountryCodes.Turkey);
	}
}
