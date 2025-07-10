using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffViewValidation : AutoTariffViewValidation
	{
		public TariffViewValidation(AutoTariffView parent) : base(parent)
		{
		}

		protected new TariffView Parent => (TariffView)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			if (!Parent.ZZ1_IsSystem)
			{
				base.ValidateAll();
				ValidateDuplicateAndOverlapingRecord();
			}
		}

		protected override void CheckZZ1_ZZZ_NKDataGrouping()
		{
			base.CheckZZ1_ZZZ_NKDataGrouping();
			ListValidation.ErrorIfInvalidCode(Parent.ZZ1_ZZZ_NKDataGroupingInfo, Parent.Lookups.CountryCodeList);
		}

		protected override void CheckZZ1_ZZI_NKTariffType()
		{
			base.CheckZZ1_ZZI_NKTariffType();
			ListValidation.ErrorIfInvalidCode(Parent.ZZ1_ZZI_NKTariffTypeInfo, Parent.Lookups.TariffTypeList);
		}

		protected override void CheckZZ1_TariffCode()
		{
			base.CheckZZ1_TariffCode();

			if (!Parent.ZZ1_TariffCode.IsNumbersOnlyOrEmpty)
			{
				Parent.ZZ1_TariffCodeInfo.AddError(Res.GetString("8bdaf6d9-6087-4ac3-8630-24c708896434", "Tariff Codes must be numeric."));
			}
		}

		protected override void CheckZZ1_StartDate()
		{
			base.CheckZZ1_StartDate();
			var endDate = Parent.ZZ1_EndDate;
			if (endDate.IsValid & Parent.ZZ1_StartDate > endDate)
			{
				Parent.ZZ1_StartDateInfo.AddError(Res.GetString("80afdf32-f476-4351-99ca-1d23032bc70c", "Effective From date cannot be later than Effective To date."));
			}
		}

		protected override void CheckZZ1_EndDate()
		{
			base.CheckZZ1_EndDate();
			MandatoryValidation.CheckEntered(Parent.ZZ1_EndDateInfo);

			var startDate = Parent.ZZ1_StartDate;
			if (startDate.IsValid & Parent.ZZ1_EndDate < startDate)
			{
				Parent.ZZ1_EndDateInfo.AddError(Res.GetString("292b4348-0e3b-40c0-be86-fb5265576c3f", "Effective To date cannot be earlier than the Effective From date."));
			}
		}

		protected override void CheckZZ1_EndDateIsValidZDateTimeRange()
		{
			// This validation isn't necessary for this BusinessObject.
		}

		protected override void CheckZZ1_StartDateIsValidZDateTimeRange()
		{
			if (Parent.ZZ1_StartDate < new ZDateTime(2000, 1, 1))
			{
				Parent.ZZ1_StartDateInfo.AddError(Res.GetString("c483cd91-b7bf-4724-b496-1b8a407fec3b", "Effective From date must not be before 01/01/2000."));
			}
		}

		protected override void CheckZZ1_Description()
		{
			base.CheckZZ1_Description();
			MandatoryValidation.CheckEntered(Parent.ZZ1_DescriptionInfo);
		}

		protected override void CheckZZ1_CompositeKeyOnZZ5IsNotEmpty()
		{
			// This validation isn't necessary for this BusinessObject.
		}

		protected override void CheckZZ1_ZZF_NKTaxOrFeeCodeIsNotEmpty()
		{
		}

		protected override void CheckZZ1_ZZF_NKTaxOrFeeCode()
		{
			base.CheckZZ1_ZZF_NKTaxOrFeeCode();

			ListValidation.ErrorIfInvalidCode(Parent.ZZ1_ZZF_NKTaxOrFeeCodeInfo);
		}

		protected void ValidateDuplicateAndOverlapingRecord()
		{
			if (Parent.Factory.ExistsInDatabase(TariffViewSchema.Constants.TableName, TariffView.Loader.GetDuplicateManualTariffFilter(Parent, Parent.ZZ1_ZZI_NKTariffType, Parent.ZZ1_ZZZ_NKDataGrouping, Parent.ZZ1_TariffCode, Parent.ZZ1_CRT_NKTariffVersion, Parent.ZZ1_StartDate, Parent.ZZ1_EndDate)))
			{
				Parent.AddRowError(Res.GetString("2cf5fd18-6c96-42c8-b055-3e36f38812d7", "Save would result in a duplicate for this Tariff."));
			}
			else if (Parent.Factory.ExistsInDatabase(TariffViewSchema.Constants.TableName, TariffView.Loader.GetDateOverlapManualTariffFilter(Parent, Parent.ZZ1_ZZI_NKTariffType, Parent.ZZ1_ZZZ_NKDataGrouping, Parent.ZZ1_TariffCode, Parent.ZZ1_CRT_NKTariffVersion, Parent.ZZ1_StartDate, Parent.ZZ1_EndDate)))
			{
				Parent.AddRowError(Res.GetString("75a28714-d77f-4f1a-95cf-b43e6e4dd166", "The date range of this tariff overlaps with another tariff."));
			}
		}

		protected override void CheckZZ1_CRT_NKTariffVersion()
		{
			base.CheckZZ1_CRT_NKTariffVersion();
			ListValidation.ErrorIfInvalidCode(Parent.ZZ1_CRT_NKTariffVersionInfo);
		}
	}
}
