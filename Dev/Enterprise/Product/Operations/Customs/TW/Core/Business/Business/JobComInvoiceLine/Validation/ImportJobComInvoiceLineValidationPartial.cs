using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class ImportJobComInvoiceLineValidation
	{
		protected override void CheckJI_Compositions()
		{
			base.CheckJI_Compositions();
			var line = InvoiceLine;
			if (line.JI_Compositions.IsEmpty)
			{
				var targetInfo = line.JI_CompositionsInfo;
				if (line.IsL1Declaration)
				{
					targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyDeclareNIL(targetInfo.HumanReadableName));
				}
				if (line.HasRegulationsCodeF01OrF02)
				{
					targetInfo.AddWarning(Res.GetString("F7DA008A-1A1D-4FEF-B185-E45CAAFB162F", "'Specification' might be required when the goods is subject to F01 or F02 importer regulation."));
				}
			}
		}

		protected override void CheckJI_ModelYear()
		{
			base.CheckJI_ModelYear();
			if (!Parent.JI_ModelYear.IsEmpty)
			{
				CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_ModelYearInfo, Parent.JI_ModelYear, 1000, 9999, Res.GetString("8C1F9EF0-2E5A-451D-B784-DE91084FB2A8", "Model Year should be 1000-9999."));
			}

			if (!InvoiceLine.IsCarRelatedDataEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ModelYearInfo);
			}
		}

		protected override void CheckJI_NumberOfDoor()
		{
			base.CheckJI_NumberOfDoor();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_NumberOfDoorInfo, Parent.JI_NumberOfDoor, 0, 9, Res.GetString("AD5270D9-2E36-495B-A872-15C6D0B6ED9E", "Number of Door should be 0-9."));
		}

		protected override void CheckJI_Displacement()
		{
			base.CheckJI_Displacement();
			var displacement = Parent.JI_Displacement;
			if (!displacement.IsEmpty)
			{
				ZDecimal displacementParseResult;
				var targetInfo = Parent.JI_DisplacementInfo;
				if (ZDecimal.TryParse(displacement, out displacementParseResult))
				{
					if (displacementParseResult <= 0 || displacementParseResult >= 1000000m)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.DisplacementOutOfRange);
					}
				}
				else
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.DisplacementShouldBeOnlyNumerics);
				}
			}
		}

		protected override void CheckJI_Cylinders()
		{
			base.CheckJI_Cylinders();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_CylindersInfo, Parent.JI_Cylinders, 0, 99, Res.GetString("85027664-88D7-4AA6-9220-F0927C5C7B4A", "Number of Cylinder should be 0-99."));
		}

		protected override void CheckJI_Gears()
		{
			base.CheckJI_Gears();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_GearsInfo, Parent.JI_Gears, 0, 99, Res.GetString("68BEB039-2CB6-428F-A328-B72D1AC0D703", "Number of Gear should be 0-99."));
		}

		protected override void CheckJI_Seats()
		{
			base.CheckJI_Seats();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_SeatsInfo, Parent.JI_Seats, 0, 99, Res.GetString("BCD88AE4-4F08-4924-BDE2-25029D2D0AD7", "Number of Seat should be 0-99."));
		}

		void CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(ZPropertyInfo info, ZInt value, ZInt minValue, ZInt maxValue, string messageError)
		{
			if (value < minValue || value > maxValue)
			{
				info.AddMessageError(messageError);
			}
		}

		protected override void CheckJI_CarType()
		{
			base.CheckJI_CarType();
			if (!InvoiceLine.IsCarRelatedDataEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CarTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CarTypeInfo, Parent.Lookups.CarTypeCodeList);
		}

		protected override void CheckJI_EquipmentPrintMode()
		{
			base.CheckJI_EquipmentPrintMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_EquipmentPrintModeInfo, Parent.Lookups.EquipmentPrintModeList);
		}

		protected override void CheckJI_LHD()
		{
			base.CheckJI_LHD();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_LHDInfo, Parent.Lookups.LeftSideSteeringCodeList);
		}

		protected override void CheckJI_EngineType()
		{
			base.CheckJI_EngineType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_EngineTypeInfo, Parent.Lookups.EngineTypeCodeList);

			if (Parent.JI_CarType == CarTypeCodeList.Codes.J1 && Parent.JI_EngineType == EngineTypeCodeList.Codes.GA && !InvoiceLine.IsCarRelatedDataEmpty)
			{
				Parent.JI_EngineTypeInfo.AddMessageError(Res.GetString("79E0CE0A-CECA-4381-9D88-4B6FB5F927A2", "Engine type cannot be 'GA' when car type is 'J1'"));
			}
		}

		protected override void CheckJI_HasCatalystConverter()
		{
			base.CheckJI_HasCatalystConverter();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_HasCatalystConverterInfo, Parent.Lookups.CatalystConverterPrintModeList);
		}

		protected override void CheckJI_Transmission()
		{
			base.CheckJI_Transmission();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_TransmissionInfo, Parent.Lookups.TransmissionCodeList);
		}

		protected override void CheckJI_CarCondition()
		{
			base.CheckJI_CarCondition();
			if (!InvoiceLine.IsCarRelatedDataEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CarConditionInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CarConditionInfo, Parent.Lookups.CarConditionCodeList);
		}

		protected override void CheckJI_GoodsType()
		{
			base.CheckJI_GoodsType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_GoodsTypeInfo, Parent.Lookups.GoodsTypeList);
		}

		protected override void CheckJI_ProductGrade()
		{
			base.CheckJI_ProductGrade();
			if (!Parent.JI_ProductGrade.IsEmpty && !IsAlphaNumeric(Parent.JI_ProductGrade))
			{
				Parent.JI_ProductGradeInfo.AddMessageError(ValidationConstants.InvoiceLine.GradeOnlyAllowsAlphanumericCharacters);
			}
		}

		protected override void CheckJI_ProductThickness()
		{
			base.CheckJI_ProductThickness();
			if (!Parent.JI_ProductThickness.IsEmpty && !IsAlphaNumeric(Parent.JI_ProductThickness))
			{
				Parent.JI_ProductThicknessInfo.AddMessageError(ValidationConstants.InvoiceLine.ThicknessOnlyAllowsAlphanumericCharacters);
			}
		}

		bool IsAlphaNumeric(string val)
		{
			return Regex.IsMatch(val, @"^[A-Z0-9]*$", RegexOptions.IgnoreCase);
		}

		protected override void CheckJI_TariffAdditionalCode()
		{
			base.CheckJI_TariffAdditionalCode();
			if (!Parent.JI_TariffAdditionalCode.IsEmpty && !IsAlphaNumeric(Parent.JI_TariffAdditionalCode))
			{
				Parent.JI_TariffAdditionalCodeInfo.AddMessageError(ValidationConstants.InvoiceLine.WarningTariffAdditionalCodeNotAlphanumericCharacters);
			}
		}

		protected override void CheckJI_AlcoholAge()
		{
			base.CheckJI_AlcoholAge();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_AlcoholAgeInfo, Parent.JI_AlcoholAge, 0, 9999);
		}

		protected override void CheckJI_BarCode()
		{
			base.CheckJI_BarCode();
			var barCode = Parent.JI_BarCode;
			if (!barCode.IsEmpty)
			{
				var targetInfo = Parent.JI_BarCodeInfo;
				if (!barCode.IsLettersAndNumbersOnlyOrEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(targetInfo.HumanReadableName));
				}

				if (barCode.Length != 13)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.BarCodeShouldBe13Characters);
				}
			}
		}

		protected override void CheckJI_PHValueCore()
		{
			base.CheckJI_PHValueCore();
			var phValue = Parent.JI_PHValue;
			if (!phValue.IsEmpty)
			{
				var targetInfo = Parent.JI_PHValueInfo;
				if (ZDecimal.CanParse(phValue))
				{
					var result = ZDecimal.Parse(phValue);
					if (result < 0 || result > 14)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PHScaleRanges);
					}
				}
				else
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckJI_SterilizationValueCore()
		{
			base.CheckJI_SterilizationValueCore();
			var sterilizationValue = Parent.JI_SterilizationValue;
			if (!sterilizationValue.IsEmpty)
			{
				var targetInfo = Parent.JI_SterilizationValueInfo;
				if (ZDecimal.CanParse(sterilizationValue))
				{
					var result = ZDecimal.Parse(sterilizationValue);
					if (result < 0 || result >= 10)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.SterilizationScaleRanges);
					}
				}
				else
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckJI_InnerPackType()
		{
			base.CheckJI_InnerPackType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InnerPackTypeInfo, Parent.Lookups.InnerPackageTypeList);
		}

		protected override void CheckJI_InnerPackingMaterial()
		{
			base.CheckJI_InnerPackingMaterial();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InnerPackingMaterialInfo, Parent.Lookups.InnerPackingMaterialList);
		}

		protected override void CheckJI_CusValueConvRatio()
		{
			base.CheckJI_CusValueConvRatio();
			var targetInfo = Parent.JI_CusValueConvRatioInfo;
			if (Parent.JI_CusValueConvRatio < 0.0M)
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.CusValueConvRatioNegative);
			}
			if (!IsBetween0And1(Parent.JI_CusValueConvRatio))
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.RangeMustBeBetween0And1);
			}
		}

		public bool IsBetween0And1(ZDecimal zDecimal)
		{
			return zDecimal >= 0 && zDecimal <= 1;
		}

		protected override void CheckJI_EPTDigit1()
		{
			base.CheckJI_EPTDigit1();
			var targetInfo = Parent.JI_EPTDigit1Info;
			if (InvoiceLine.IsEnvironmentalProtectionTariff)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.ContainerMaterialList);
		}

		protected override void CheckJI_EPTDigit2()
		{
			base.CheckJI_EPTDigit2();
			var targetInfo = Parent.JI_EPTDigit2Info;
			if (InvoiceLine.IsEnvironmentalProtectionTariff)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.ContainerCapacityList);
		}

		protected override void CheckJI_EPTDigit3()
		{
			base.CheckJI_EPTDigit3();
			var targetInfo = Parent.JI_EPTDigit3Info;
			if (InvoiceLine.IsEnvironmentalProtectionTariff)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.ContainerMaterialNumberList);
		}

		protected override void CheckJI_RAPPrice()
		{
			base.CheckJI_RAPPrice();
			var parentInvoiceLine = InvoiceLine;
			if (parentInvoiceLine.IsRAPOrROR)
			{
				var targetInfo = parentInvoiceLine.JI_RAPPriceInfo;
				if (!parentInvoiceLine.JI_RAPRORPriceReadOnly)
				{
					var procedure = parentInvoiceLine.JI_Procedure;
					if (parentInvoiceLine.IsROR)
					{
						targetInfo.AddWarning(Res.GetString("5D7CBEB1-B9F6-489A-930C-3A1D1D7F4B99", "When the Duty Treatment is {0}, the duties for the remainder of the Customs Value, excluding the ROR Price, will be calculated as a Non-Cash payment.", procedure));
					}
					else if (parentInvoiceLine.IsRAP && parentInvoiceLine.JI_RAPPrice.IsEmpty)
					{
						targetInfo.AddWarning(Res.GetString("5DE226C5-A658-4A8C-83CC-2112A8AD3C73", "When the duty treatment is {0} and the RAP Price is 0, duties will be calculated using 0 as the base value.", procedure));
					}
				}

				MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
			}
		}

		protected override void CheckJI_RAPCurr()
		{
			base.CheckJI_RAPCurr();
			var parentInvoiceLine = InvoiceLine;
			if (parentInvoiceLine.IsRAPOrROR)
			{
				var targetInfo = parentInvoiceLine.JI_RAPCurrInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				if (!parentInvoiceLine.JI_RAPPrice.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckJI_AntiDumpingDutyRate()
		{
			base.CheckJI_AntiDumpingDutyRate();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_AntiDumpingDutyRateInfo);
		}

		protected override void CheckJI_CountervailingDutyRate()
		{
			base.CheckJI_CountervailingDutyRate();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CountervailingDutyRateInfo);
		}

		protected override void CheckJI_AdditionalDutyRate()
		{
			base.CheckJI_AdditionalDutyRate();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_AdditionalDutyRateInfo);
		}

		protected override void CheckJI_RetaliatoryDutyRate()
		{
			base.CheckJI_RetaliatoryDutyRate();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_RetaliatoryDutyRateInfo);
		}

		protected override void CheckJI_AlcoholPercentage()
		{
			base.CheckJI_AlcoholPercentage();
			if (Parent.JI_AlcoholPercentage == 0 && InvoiceLine.IsForCMHeaderMessageTypeNX301_DN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_AlcoholPercentageInfo);
			}
		}
	}
}
