using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineTaxLookups : Customs.Business.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(JobComInvoiceLineTax parent) : base(parent)
		{ }

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		public override ICodeDescriptionPairList TypeList => Factory.GetCachedValue("TW.JobComInvoiceLineTaxLookups.GetSecondaryTariffsList", ()
			=> new AdditionalDutiesTariffTypeList(Factory,
				Core.Constants.CountryCodes.Taiwan,
				new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, SQLComparisonOperator.NotEqual, Universal.Constants.TariffTypes.HarmonizedSystem)));

		public ChildTariffViewCollection TariffCollection
		{
			get
			{
				var tariffDetailParent = Parent.InvoiceLine;
				var type = Parent.JLT_Type;
				var effectiveAssessmentDate = tariffDetailParent?.EffectiveAssessmentDate ?? ZDateTime.Today;
				return ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Taiwan, type, effectiveAssessmentDate, null);
			}
		}

		public override CodeDescriptionPairList MOPList
		{
			get
			{
				var parent = Parent;
				var isROR = parent.IsRorType && (parent.InvoiceLine?.IsROR ?? false);
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.JobComInvoiceLineTaxLookups.MOPList_{isROR}", () =>
				{
					var result = new CodeDescriptionPairList(new TaxFeePaymentMethodList());
					if (!isROR)
					{
						result.RemoveCode(TaxFeePaymentMethodList.Codes.ROR);
					}
					return result;
				});
			}
		}
	}
}
