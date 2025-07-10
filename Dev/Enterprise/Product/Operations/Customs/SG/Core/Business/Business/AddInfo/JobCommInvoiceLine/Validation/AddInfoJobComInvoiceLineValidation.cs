using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLineValidation : SGAddInfoValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region Check Packs

		internal const string MustBeInSequence = "Packing units must be entered in sequence: Outer -> In -> Inner -> Inmost pack qty and units.";

		bool PacksNotEnteredInSequence
		{
			get
			{
				var result = false;

				if (Parent.SG_InmostPackQuantity > 0 &&
					(Parent.SG_InnerPackQuantity == 0 || Parent.SG_InPackQuantity == 0 || Parent.SG_OuterPackQuantity == 0))
				{
					result = true;
				}
				else if (Parent.SG_InnerPackQuantity > 0 && (Parent.SG_InPackQuantity == 0 || Parent.SG_OuterPackQuantity == 0))
				{
					result = true;
				}
				else if (Parent.SG_InPackQuantity > 0 && Parent.SG_OuterPackQuantity == 0)
				{
					result = true;
				}

				return result;
			}
		}

		protected override void CheckSG_InmostPackQuantity()
		{
			base.CheckSG_InmostPackQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_InmostPackQuantityInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SG_InmostPackQuantityInfo, 99999999);
			if (Parent.SG_InmostPackQuantity == 0 && !Parent.SG_InmostPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InmostPackQuantityInfo.AddWarning("Inmost Pack Quantity is required when package type is entered.");
			}

			if (PacksNotEnteredInSequence)
			{
				Parent.SG_InmostPackQuantityInfo.AddMessageError(MustBeInSequence);
			}

			ValidateSG_InmostPackQuantityUnit();
		}

		protected override void CheckSG_InmostPackQuantityUnit()
		{
			base.CheckSG_InmostPackQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_InmostPackQuantityUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_InmostPackQuantity > 0 && Parent.SG_InmostPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InmostPackQuantityUnitInfo.AddWarning("Package type is required when Inmost Pack Quantity is greater than 0.");
			}

			ValidateSG_InmostPackQuantity();
		}

		protected override void CheckSG_InnerPackQuantity()
		{
			base.CheckSG_InnerPackQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_InnerPackQuantityInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SG_InnerPackQuantityInfo, 99999999);
			if (Parent.SG_InnerPackQuantity == 0 && !Parent.SG_InnerPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InnerPackQuantityInfo.AddWarning("Inner Pack Quantity is required when package type is entered.");
			}

			if (PacksNotEnteredInSequence)
			{
				Parent.SG_InnerPackQuantityInfo.AddMessageError(MustBeInSequence);
			}

			ValidateSG_InnerPackQuantityUnit();
		}

		protected override void CheckSG_InnerPackQuantityUnit()
		{
			base.CheckSG_InnerPackQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_InnerPackQuantityUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_InnerPackQuantity > 0 && Parent.SG_InnerPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InnerPackQuantityUnitInfo.AddWarning("Package type is required when Inner Pack Quantity is greater than 0.");
			}

			ValidateSG_InnerPackQuantity();
		}

		protected override void CheckSG_InPackQuantity()
		{
			base.CheckSG_InPackQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_InPackQuantityInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SG_InPackQuantityInfo, 99999999);
			if (Parent.SG_InPackQuantity == 0 && !Parent.SG_InPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InPackQuantityInfo.AddWarning("In Pack Quantity is required when package type is entered.");
			}

			if (PacksNotEnteredInSequence)
			{
				Parent.SG_InPackQuantityInfo.AddMessageError(MustBeInSequence);
			}

			ValidateSG_InPackQuantityUnit();
		}

		protected override void CheckSG_InPackQuantityUnit()
		{
			base.CheckSG_InPackQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_InPackQuantityUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_InPackQuantity > 0 && Parent.SG_InPackQuantityUnit.IsEmpty)
			{
				Parent.SG_InPackQuantityUnitInfo.AddWarning("Package type is required when In Pack Quantity is greater than 0.");
			}

			ValidateSG_InPackQuantity();
		}

		protected override void CheckSG_OuterPackQuantity()
		{
			base.CheckSG_OuterPackQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_OuterPackQuantityInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SG_OuterPackQuantityInfo, 99999999);
			if (Parent.SG_OuterPackQuantity == 0 && !Parent.SG_OuterPackQuantityUnit.IsEmpty)
			{
				Parent.SG_OuterPackQuantityInfo.AddWarning("Outer Pack Quantity is required when package type is entered.");
			}

			if (PacksNotEnteredInSequence)
			{
				Parent.SG_OuterPackQuantityInfo.AddMessageError(MustBeInSequence);
			}

			ValidateSG_OuterPackQuantityUnit();
		}

		protected override void CheckSG_OuterPackQuantityUnit()
		{
			base.CheckSG_OuterPackQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_OuterPackQuantityUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_OuterPackQuantity > 0 && Parent.SG_OuterPackQuantityUnit.IsEmpty)
			{
				Parent.SG_OuterPackQuantityUnitInfo.AddWarning("Package type is required when Outer Pack Quantity is greater than 0.");
			}

			ValidateSG_OuterPackQuantity();
		}

		protected override void CheckSG_UnitDutiableWGTVOLQTY()
		{
			base.CheckSG_UnitDutiableWGTVOLQTY();
			CompareValidation.CheckNumberNotNegative(Parent.SG_UnitDutiableWGTVOLQTYInfo);
			if (Parent.SG_UnitDutiableWGTVOLQTY == 0 && !Parent.SG_UnitDutiableWGTVOLQTYUnit.IsEmpty)
			{
				Parent.SG_UnitDutiableWGTVOLQTYInfo.AddWarning("Unit Dutiable WGT/VOL/QTY is required when Unit of Quantity is entered");
			}

			ValidateSG_UnitDutiableWGTVOLQTYUnit();
		}

		protected override void CheckSG_UnitDutiableWGTVOLQTYUnit()
		{
			base.CheckSG_UnitDutiableWGTVOLQTYUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_UnitDutiableWGTVOLQTYUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_UnitDutiableWGTVOLQTY > 0 && Parent.SG_UnitDutiableWGTVOLQTYUnit.IsEmpty)
			{
				Parent.SG_UnitDutiableWGTVOLQTYUnitInfo.AddMessageError("Package type is required when Unit Dutiable WGT/VOL/QTY is greater than 0.");
			}

			var tariff = Tariff;
			if (tariff != null)
			{
				if (tariff.IsDutiableType())
				{
					ListValidation.WarnIfInvalidCode(Parent.SG_UnitDutiableWGTVOLQTYUnitInfo, Parent.InvoiceLine.Lookups.DutiableUQList, DutiableUnitsOfQty);

					var tariffUQ = tariff.ZZ1_ZZ8_UQ2;
					if (tariffUQ != ZString.Empty && tariffUQ != SGConstants.LPA && Parent.SG_UnitDutiableWGTVOLQTYUnit != tariffUQ)
					{
						Parent.SG_UnitDutiableWGTVOLQTYUnitInfo.AddMessageError("Unit of Quantity should comply with customs reporting requirements. Valid Unit of Quantity is: " + tariffUQ);
					}
				}
			}

			ValidateSG_UnitDutiableWGTVOLQTY();
		}

		const string DutiableUnitsOfQty = "For dutiable commodities, UQ is generally required as one of: " + UnitOfQuantityCodeList.Codes.DAL + ", " + UnitOfQuantityCodeList.Codes.KGM + ", " + UnitOfQuantityCodeList.Codes.LTR + ", " + UnitOfQuantityCodeList.Codes.NMB + " or " + UnitOfQuantityCodeList.Codes.STK + ". ";

		protected override void CheckSG_TotalDutiableWGTVOLQTY()
		{
			base.CheckSG_TotalDutiableWGTVOLQTY();
			CompareValidation.CheckNumberNotNegative(Parent.SG_TotalDutiableWGTVOLQTYInfo);
			if (Parent.SG_TotalDutiableWGTVOLQTY == 0 && !Parent.SG_TotalDutiableWGTVOLQTYUnit.IsEmpty)
			{
				Parent.SG_TotalDutiableWGTVOLQTYInfo.AddWarning("Total Dutiable WGT/VOL/QTY is required when Unit of Quantity is entered");
			}

			ValidateSG_TotalDutiableWGTVOLQTYUnit();
		}

		protected override void CheckSG_TotalDutiableWGTVOLQTYUnit()
		{
			base.CheckSG_TotalDutiableWGTVOLQTYUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_TotalDutiableWGTVOLQTYUnitInfo, Parent.Lookups.UnitOfQuantityList);

			if (Parent.SG_TotalDutiableWGTVOLQTY > 0 && Parent.SG_TotalDutiableWGTVOLQTYUnit.IsEmpty)
			{
				Parent.SG_TotalDutiableWGTVOLQTYUnitInfo.AddMessageError("Package type is required when Total Dutiable WGT/VOL/QTY is greater than 0.");
			}

			var tariff = Tariff;
			if (tariff != null)
			{
				if (tariff.IsDutiableType())
				{
					ListValidation.WarnIfInvalidCode(Parent.SG_TotalDutiableWGTVOLQTYUnitInfo, Parent.InvoiceLine.Lookups.DutiableUQList, DutiableUnitsOfQty);

					var tariffUQ = tariff.ZZ1_ZZ8_UQ2;
					if (tariffUQ != ZString.Empty && tariffUQ != SGConstants.LPA && Parent.SG_TotalDutiableWGTVOLQTYUnit != tariffUQ)
					{
						Parent.SG_TotalDutiableWGTVOLQTYUnitInfo.AddMessageError("Unit of Quantity should comply with customs reporting requirements. Valid Unit of Quantity is: " + Parent.InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
					}
				}
			}

			ValidateSG_TotalDutiableWGTVOLQTY();
		}

		#endregion

		#region Check Current Lot No

		protected override void CheckSG_LotNo()
		{
			base.CheckSG_LotNo();
			if (IsStorageInBondedWarehouse
				|| Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKT && IsStorageInLicencedPremise && IsLiqourOrTobaccoOrMotorVehicle)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_LotNoInfo, "Current Lot Number");
			}
		}

		bool IsStorageInBondedWarehouse => Declaration.PlaceOfStorage?.IsBondedWarehouse() ?? false;

		bool IsStorageInLicencedPremise => Declaration.PlaceOfStorage?.IsLicencedPremise() ?? false;

		bool IsLiqourOrTobaccoOrMotorVehicle
		{
			get { return InvoiceLine.SG_TariffCommodityType == CommodityTypeList.Codes.Vehicle || InvoiceLine.SG_TariffCommodityType == CommodityTypeList.Codes.Tobacco || InvoiceLine.SG_TariffCommodityType == CommodityTypeList.Codes.Alcohol; }
		}

		#endregion

		protected override void CheckSG_PercAlcohol()
		{
			base.CheckSG_PercAlcohol();
			var tariff = Tariff;
			if (tariff != null)
			{
				var uom = tariff.ZZ1_ZZ8_UQ2;
				if (uom == SGConstants.LPA)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_PercAlcoholInfo, "Alcohol Percentage");
				}
			}

			CompareValidation.CheckNumberNotNegative(Parent.SG_PercAlcoholInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_PercAlcoholInfo, 100);
		}

		protected override void CheckSG_TobaccoMultiplier()
		{
			base.CheckSG_TobaccoMultiplier();
			CompareValidation.CheckNumberNotNegative(Parent.SG_TobaccoMultiplierInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_TobaccoMultiplierInfo, 100);
		}

		protected override void CheckSG_DutyPercentageRate()
		{
			base.CheckSG_DutyPercentageRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_DutyPercentageRateInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_DutyPercentageRateInfo, 100);
		}

		protected override void CheckSG_ExcisePercentageRate()
		{
			base.CheckSG_ExcisePercentageRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_ExcisePercentageRateInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_ExcisePercentageRateInfo, 100);
		}

		protected override void CheckSG_DutyUnitRate()
		{
			base.CheckSG_DutyUnitRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_DutyUnitRateInfo);
		}

		protected override void CheckSG_ExciseUnitRate()
		{
			base.CheckSG_ExciseUnitRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_ExciseUnitRateInfo);
		}

		protected override void CheckSG_CertHSCode()
		{
			base.CheckSG_CertHSCode();
			if (OriginCriterionRequiresHSCode(Parent.SG_CertOriginCriterion1) && Parent.SG_CertHSCode.IsEmpty)
			{
				Parent.SG_CertHSCodeInfo.AddMessageError(HSCodeRequired);
			}
		}
		const string HSCodeRequired = "When using this Origin Criterion, HS Code is required.";

		bool OriginCriterionRequiresHSCode(string criterion1)
		{
			return criterion1 == OriginCriterionCodeList.Codes.GSPFormA_W;
		}

		protected override void CheckSG_PercContent()
		{
			base.CheckSG_PercContent();
			CompareValidation.CheckNumberNotNegative(Parent.SG_PercContentInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_PercContentInfo, 100);
			if (OriginCriterionRequiresPercentageContent(Parent.SG_CertOriginCriterion1) && Parent.SG_PercContent.IsEmpty)
			{
				Parent.SG_PercContentInfo.AddWarning(PercentageContentRequired);
			}
		}
		const string PercentageContentRequired = "When using this Origin Criterion, % Content may be required.";

		bool OriginCriterionRequiresPercentageContent(string criterion1)
		{
			return criterion1 == OriginCriterionCodeList.Codes.GSPFormA_PK ||
				   criterion1 == OriginCriterionCodeList.Codes.GSPFormA_Y ||
				   criterion1 == OriginCriterionCodeList.Codes.CEPTFormD_RVC ||         // also OriginCriterionCodeList.Codes.AKFTAFormAK_RVC
				   criterion1 == "ACFTA" ||                                             // OriginCriterionCodeList.Codes.ACFTAFormE_ACC
				   criterion1 == "SINGLE" ||                                            // OriginCriterionCodeList.Codes.ACFTAFormE_SCC && OriginCriterionCodeList.Codes.CECAPrefCO_SCC
				   criterion1 == "ISECA" ||                                             // OriginCriterionCodeList.Codes.CECAPrefCO_ICC
				   criterion1 == OriginCriterionCodeList.Codes.AKFTAFormAK_CTH_RVC ||
				   criterion1 == OriginCriterionCodeList.Codes.AIFTAFormAI_RVC;
		}

		protected override void CheckSG_ESNDPIndicator()
		{
			base.CheckSG_ESNDPIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_ESNDPIndicatorInfo, Parent.Lookups.ESNDPs);
		}

		protected override void CheckSG_TariffCommodityType()
		{
			base.CheckSG_TariffCommodityType();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_TariffCommodityTypeInfo, Parent.Lookups.CommodityTypes);
		}

		protected override void CheckSG_LastSellingPrice()
		{
			base.CheckSG_LastSellingPrice();
			CompareValidation.CheckNumberNotNegative(Parent.SG_LastSellingPriceInfo);
		}

		protected override void CheckSG_CertItemQuantity()
		{
			base.CheckSG_CertItemQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_CertItemQuantityInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.SG_CertItemQuantityInfo, 99999999999.9999m);
			if (Parent.SG_CertItemQuantity == 0 && !Parent.SG_CertItemQuantityUnit.IsEmpty)
			{
				Parent.SG_CertItemQuantityInfo.AddWarning("Item Quantity is required when Unit of Quantity is entered");
			}

			ValidateSG_CertItemQuantityUnit();
		}

		protected override void CheckSG_CertItemQuantityUnit()
		{
			base.CheckSG_CertItemQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_CertItemQuantityUnitInfo, Parent.Lookups.ProductCodeUQList);
			if (Parent.SG_CertItemQuantity > 0 && Parent.SG_CertItemQuantityUnit.IsEmpty)
			{
				Parent.SG_CertItemQuantityUnitInfo.AddWarning("Package type is required when Item Quantity is greater than 0.");
			}

			ValidateSG_CertItemQuantity();
		}

		protected override void CheckSG_CertItemValue()
		{
			var declaration = Parent.InvoiceLine.Declaration;
			var cert1Type = declaration.SG_Cert1Type;
			if (declaration.HasCofO && !cert1Type.IsEmpty)
			{
				base.CheckSG_CertItemValue();
				MandatoryValidation.CheckNotNegative(Parent.SG_CertItemValueInfo);
				if (!SGCertificatesCodeList.IsItemValueAllowed(Parent.InvoiceLine.Declaration.Certificate1Type))
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.SG_CertItemValueInfo, "Item value. Certificate Item value is not applicable for this Certificate Type declaration.");
				}
			}
		}

		//Note 2:
		//a) Mandatory to specify Origin Criterion details if Certificate Type = 1, 16, 17, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31 & 32.
		//c) Not applicable for other Certificate Types.
		protected override void CheckSG_CertOriginCriterion1()
		{
			var declaration = Parent.InvoiceLine.Declaration;
			var cert1Type = declaration.SG_Cert1Type;
			if (declaration.HasCofO && !cert1Type.IsEmpty)
			{
				base.CheckSG_CertOriginCriterion1();
				if (SGCertificatesCodeList.IsOriginCriterionDetailsRequired(Parent.InvoiceLine.Declaration.Certificate1Type))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_CertOriginCriterion1Info, "Origin Criterion. It is mandatory to specify Origin Criterion details for this Certificate Type.");
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.SG_CertOriginCriterion1Info, "Origin Criterion. Origin Criterion details are not applicable for this Certificate Type declaration.");
				}
			}
		}

		protected override void CheckSG_EngineCapacityPower()
		{
			base.CheckSG_EngineCapacityPower();
			CompareValidation.CheckNumberNotNegative(Parent.SG_EngineCapacityPowerInfo);
			if (Parent.SG_EngineCapacityPower == 0 && !Parent.SG_EngineCapacityPowerUnit.IsEmpty)
			{
				Parent.SG_EngineCapacityPowerInfo.AddWarning("Engine Capacity/Power is required when Unit of Power is entered");
			}

			if (Parent.SG_EngineCapacityPower >= 10000)
			{
				Parent.SG_EngineCapacityPowerInfo.AddMessageError("Engine Capacity/Power should be less than 10000");
			}

			ValidateSG_EngineCapacityPowerUnit();
		}

		protected override void CheckSG_EngineCapacityPowerUnit()
		{
			base.CheckSG_EngineCapacityPowerUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_EngineCapacityPowerUnitInfo, Parent.Lookups.EngineCapacityList);
			if (Parent.SG_EngineCapacityPower > 0 && Parent.SG_EngineCapacityPowerUnit.IsEmpty)
			{
				Parent.SG_EngineCapacityPowerUnitInfo.AddWarning("Unit of Power is required when Engine Capacity/Power is greater than 0.");
			}

			ValidateSG_EngineCapacityPower();
		}

		protected override void CheckSG_TextileQuotaQuantity()
		{
			base.CheckSG_TextileQuotaQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.SG_TextileQuotaQuantityInfo);
			if (Parent.SG_TextileQuotaQuantity == 0 && !Parent.SG_TextileQuotaQuantityUnit.IsEmpty)
			{
				Parent.SG_TextileQuotaQuantityInfo.AddWarning("Textile Quota Quantity is required when Unit of Quantity is entered");
			}

			ValidateSG_TextileQuotaQuantityUnit();
		}

		protected override void CheckSG_TextileQuotaQuantityUnit()
		{
			base.CheckSG_TextileQuotaQuantityUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_TextileQuotaQuantityUnitInfo, Parent.Lookups.ProductCodeUQList);
			if (Parent.SG_TextileQuotaQuantity > 0 && Parent.SG_TextileQuotaQuantityUnit.IsEmpty)
			{
				Parent.SG_TextileQuotaQuantityUnitInfo.AddWarning("Package type is required when Textile Quota Quantity is greater than 0.");
			}

			ValidateSG_TextileQuotaQuantity();
		}

		protected override void CheckSG_IsStrategic()
		{
			base.CheckSG_IsStrategic();

			if (Parent.SG_IsStrategic)
			{
				if (Parent.InvoiceLine.Declaration != null && (Parent.InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP || Parent.InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP))
				{
					Parent.SG_IsStrategicInfo.AddMessageError("Strategic goods indicator is only valid for OUT & TNP declarations");
				}
				else // provide generic notification warning in ALL cases when Strategic goods are indicated.
				{
					Parent.SG_IsStrategicInfo.AddWarning("Please note that Strategic Goods control is based on the actual product description and technical specifications of the goods, and not by the HS Code. Traders are advised to refer to the Strategic Goods (Control) Order for up-to-date information in relation to the definitions and scope of goods subject to controls.");
				}
			}

			if (Parent.InvoiceLine.AddInfoValidation != null)
			{
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_CategoryCode();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_StrategicGoodsProductCodeQuantity();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_StrategicGoodsProductCodeQuantityUnit();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_EndUseCode1();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_EndUseCode2();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_EndUseCode3();
				Parent.InvoiceLine.AddInfoValidation.ValidateSG_EndUseDescription();
			}

			if (Parent.InvoiceLine.Declaration != null && Parent.InvoiceLine.Declaration.AddInfoValidation != null)
			{
				Parent.InvoiceLine.Declaration.Validation.ValidateJE_OH_Consignee();
			}
		}

		protected override void CheckSG_EndUseCode1()
		{
			base.CheckSG_EndUseCode1();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_EndUseCode1Info, Parent.Lookups.EndUseCodes1);
		}

		protected override void CheckSG_EndUseCode2()
		{
			base.CheckSG_EndUseCode2();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_EndUseCode2Info, Parent.Lookups.EndUseCodes2);
		}

		protected override void CheckSG_EndUseCode3()
		{
			base.CheckSG_EndUseCode3();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_EndUseCode3Info, Parent.Lookups.EndUseCodes3);
		}

		protected override void CheckSG_CategoryCode()
		{
			base.CheckSG_CategoryCode();
			if (Parent.SG_IsStrategic)
			{
				ListValidation.WarnIfInvalidCode(Parent.SG_CategoryCodeInfo, Parent.Lookups.StrategicGoodsProductCode, "Please refer to Strategic Goods (Control) Order (http://www.customs.gov.sg/stgc) to verify the Strategic Goods Product Code you have entered is correct. ");
			}
		}

		protected override void CheckSG_FirstRegistrationDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.SG_FirstRegistrationDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 5,
				FutureYearsBeforeWarning = 1,
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = 10
			});
		}

		protected override void CheckSG_StrategicGoodsProductCodeQuantityUnit()
		{
			base.CheckSG_StrategicGoodsProductCodeQuantityUnit();
			if (Parent.SG_StrategicGoodsProductCodeQuantity > 0 && Parent.SG_StrategicGoodsProductCodeQuantityUnit.IsEmpty)
			{
				Parent.SG_StrategicGoodsProductCodeQuantityUnitInfo.AddMessageError("Product Qty Unit of Qty must be entered when entering quantity.");
			}

			if (!Parent.SG_StrategicGoodsProductCodeQuantityUnit.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.SG_StrategicGoodsProductCodeQuantityUnitInfo, Parent.Lookups.ProductCodeUQList);
			}
		}

		#region Implementation

		protected JobDeclaration Declaration
		{
			get { return InvoiceLine.Declaration; }
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return Parent.InvoiceLine; }
		}

		protected TariffView Tariff => InvoiceLine?.UniversalTariff;

		protected new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		protected AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected ValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new ValidationHelper()); }
		}
		ValidationHelper validationHelper;

		#endregion
	}
}
