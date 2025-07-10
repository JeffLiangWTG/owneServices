using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusClassPartPivotValidation : AutoTWCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		protected override void CheckCI_RN_NKCountryOfOrigin()
		{
			base.CheckCI_RN_NKCountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_RN_NKCountryOfOriginInfo);
		}

		protected override void CheckCI_PartPivotUOM()
		{
			base.CheckCI_PartPivotUOM();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_PartPivotUOMInfo);
		}

		protected override void CheckCI_DeclGoodsDescMode()
		{
			base.CheckCI_DeclGoodsDescMode();
			var targetInfo = Parent.CI_DeclGoodsDescModeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo, Parent.Lookups.DeclarationGoodsDescriptionModeList);
		}

		protected override void CheckCI_Price()
		{
			TypeValidation.CheckValidMoney(Parent.CI_PriceInfo, 19, 4);
		}

		protected override void CheckCI_PriceCurr()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_PriceCurrInfo, Parent.Lookups.CurrencyList);
		}

		protected override void CheckCI_ModeOfStatistics()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_ModeOfStatisticsInfo, Parent.Lookups.ModeOfStatistics);
		}

		protected override void CheckCI_DutyTreatment()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_DutyTreatmentInfo, Parent.Lookups.DutyTreatment);
		}

		protected override void CheckCI_CarType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_CarTypeInfo, Parent.Lookups.CarTypeCodeList);
		}

		protected override void CheckCI_Transmission()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_TransmissionInfo, Parent.Lookups.TransmissionCodeList);
		}

		protected override void CheckCI_EngineType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_EngineTypeInfo, Parent.Lookups.EngineTypeCodeList);
		}

		protected override void CheckCI_LHD()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_LHDInfo, Parent.Lookups.LeftSideSteeringCodeList);
		}

		protected override void CheckCI_HasCatalystConverter()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_HasCatalystConverterInfo, Parent.Lookups.CatalystConverterPrintModeList);
		}

		protected override void CheckCI_EquipmentPrintMode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_EquipmentPrintModeInfo, Parent.Lookups.EquipmentPrintModeList);
		}

		protected override void CheckCI_CarCondition()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_CarConditionInfo, Parent.Lookups.CarConditionCodeList);
		}

		protected override void CheckCI_ModelYear()
		{
			var modelYear = Parent.CI_ModelYear;
			if (!modelYear.IsEmpty)
			{
				CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.CI_ModelYearInfo, Parent.CI_ModelYear, 1000, 9999, Res.GetString("1AB05E2D-6BE4-4E29-B4DE-1CFAEC22A743", "Model Year should be 1000-9999."));
			}
		}

		void CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(ZPropertyInfo info, ZInt value, ZInt minValue, ZInt maxValue, string messageError)
		{
			if (value < minValue || value > maxValue)
			{
				info.AddMessageError(messageError);
			}
		}

		protected override void CheckCI_Displacement()
		{
			var ciDisplacement = Parent.CI_Displacement;
			if (!ciDisplacement.IsEmpty)
			{
				var info = Parent.CI_DisplacementInfo;
				var maxValue = System.Math.Pow(10, info.MaxLength - 1) - 1;

				if (!ZDecimal.TryParse(ciDisplacement, out var displacement))
				{
					info.AddMessageError(Res.GetString("A95F948D-C29E-4475-89B7-15E6B18B3F52", "The value should only contain numeric characters."));
				}
				else if (!displacement.IsInRange(ZDecimal.Zero, maxValue))
				{
					info.AddMessageError(Res.GetString("9F886F9B-7987-4762-B81D-F9E16EEDBEE4", "The value should be between 0 and ") + maxValue.ToString(CultureInfo.CurrentCulture) + Res.GetString("8FA520CC-4D51-4BC5-BD78-66B6EDE362A6", " cc."));
				}
			}
		}

		protected override void CheckCI_EPTDigit1()
		{
			CheckInvalidCodeOrEmptyIfAllowEnvironmentalProtectionTariff(Parent.CI_EPTDigit1Info, Parent.Lookups.ContainerMaterialList);
		}

		protected override void CheckCI_EPTDigit2()
		{
			CheckInvalidCodeOrEmptyIfAllowEnvironmentalProtectionTariff(Parent.CI_EPTDigit2Info, Parent.Lookups.ContainerCapacityList);
		}

		protected override void CheckCI_EPTDigit3()
		{
			CheckInvalidCodeOrEmptyIfAllowEnvironmentalProtectionTariff(Parent.CI_EPTDigit3Info, Parent.Lookups.ContainerMaterialNumberList);
		}

		void CheckInvalidCodeOrEmptyIfAllowEnvironmentalProtectionTariff(ZPropertyInfo info, ICodeDescriptionPairList list)
		{
			if (ShouldAllowEnvironmentalProtectionTariff)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(info, list);
			}
		}

		bool ShouldAllowEnvironmentalProtectionTariff => Parent?.ShouldAllowEnvironmentalProtectionTariff ?? false;
	}
}
