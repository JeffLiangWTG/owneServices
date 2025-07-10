using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Universal
{
	public sealed class VATApplicabilityView : AutoVATApplicabilityView, ITariffEffectiveDatesRelatedBusinessObject
	{
		public VATApplicabilityView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZX5_ZZ1_ParentTariffOrNationalCode
		{
			get => base.ZX5_ZZ1_ParentTariffOrNationalCode;
			set => base.ZX5_ZZ1_ParentTariffOrNationalCode = value;
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZX5_ZZ1_ParentTariffOrNationalCode);

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZX5_ZZZ_NKDataGrouping { get => base.ZX5_ZZZ_NKDataGrouping; set => base.ZX5_ZZZ_NKDataGrouping = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_EndDate", Caption = "End Date")]
		public override ZDateTime ZX5_EndDate { get => base.ZX5_EndDate; set => base.ZX5_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_StartDate", Caption = "Start Date")]
		public override ZDateTime ZX5_StartDate { get => base.ZX5_StartDate; set => base.ZX5_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_AdditionalCode", Caption = "Additional Code")]
		public override ZString ZX5_AdditionalCode { get => base.ZX5_AdditionalCode; set => base.ZX5_AdditionalCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_Description", Caption = "Description")]
		public override ZString ZX5_Description { get => base.ZX5_Description; set => base.ZX5_Description = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_ZZF_NKTaxOrFeeCode", Caption = "Tax or Fee")]
		public override ZString ZX5_ZZF_NKTaxOrFeeCode { get => base.ZX5_ZZF_NKTaxOrFeeCode; set => base.ZX5_ZZF_NKTaxOrFeeCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.VATApplicabilityView|ZX5_VATCategory", Caption = "Category")]
		public override ZString ZX5_VATCategory { get => base.ZX5_VATCategory; set => base.ZX5_VATCategory = value; }

		#region ITariffEffectiveDatesRelatedBusinessObject Members
		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZX5_ZZZ_NKDataGrouping;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.StartDate => ZX5_StartDate;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.EndDate => ZX5_EndDate;
		#endregion
	}
}
