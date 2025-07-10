using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CommonImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public CommonImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region JI_CustomsQuantity

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();

			if (Parent.JI_CustomsQuantity < 0m)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}
		}

		protected void CheckJI_CustomsQuantityBoundary(CargoWise.ComponentModel.INotificationType notificationType)
		{
			if (Parent.ImportTariff != null)
			{
				new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsQuantityInfo, Parent.JI_CustomsUnitQty,
					() => Parent.ImportTariff.RequiresFirstQuantity(), QuantityCode.FirstQuantity, notificationType);
			}
		}

		#endregion

		protected override void CheckJI_Description()
		{
			if (Parent.JI_PartNo_CanBeSetByCustomer)
			{
				base.CheckJI_Description();
			}

			if (IsDescriptionRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
			}
		}

		protected virtual bool IsDescriptionRequired
		{
			get { return true; }
		}

		#region JI_Weight

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.CheckNotNegative(Parent.JI_WeightInfo);

			if (Parent.GrossWeightInKG < Parent.NetWeightInKG)
			{
				Parent.JI_WeightInfo.AddMessageError(NetWeightBiggerThenGrossWeight);
			}

			if (ShouldValidateGrossWeight && Parent.JI_Weight == ZDecimal.Zero && (Parent.JI_LinePrice > 0 || Parent.JI_CustomsValue > 0))
			{
				Parent.JI_WeightInfo.AddMessageError(WeightIsRequired);
			}

			var declaration = Parent.Declaration;
			if (declaration != null && !declaration.IsRecon && !declaration.IsDrawback)
			{
				if (Parent.JI_Weight > 0)
				{
					if (Parent.JI_CustomsUnitQty == Parent.JI_WeightUQ)
					{
						if (Parent.JI_CustomsQuantity > Parent.JI_Weight)
						{
							Parent.JI_WeightInfo.AddWarning(WeightImbalance);
						}
					}
					else if (Parent.CanConvertCustomsQtyToKG && Parent.CustomsQuantityInKG > Parent.GrossWeightInKG)
					{
						Parent.JI_WeightInfo.AddWarning(EquivalentWeightImbalance);
					}
				}
			}

			ValidateJI_CustomsQuantity();
		}
		internal const string NetWeightBiggerThenGrossWeight = "Gross Weight can not be less then Net Weight.";
		internal const string WeightIsRequired = "Weight is required if Invoice Price is entered, (or Customs Value > 0).";
		internal const string WeightImbalance = "Gross Weight is less than Customs Quantity.";
		internal const string EquivalentWeightImbalance = "Gross Weight is less than Customs Quantity, (converted to equivalent Unit of Measure).";

		protected virtual bool ShouldValidateGrossWeight
		{
			get { return true; }
		}

		#endregion

		#region JI_LinePrice

		protected void CheckJI_LinePriceBoundary()
		{
			var cusEntryLine = Parent.CusEntryLine;
			if (cusEntryLine != null && cusEntryLine.Header != null && cusEntryLine.IsRelevantForLinePriceBoundaryCheck)
			{
				var valueQuantityBoundValidator = Parent.Factory.GetCachedValue<ValueQuantityBoundValidator>();
				var supEntryLine = !Parent.HasEmptySupTariff ? Parent.GetEntryLineFor(cusEntryLine.Header.CH_MessageType, true) : null;

				var totalValue = cusEntryLine.CL_CustomsValue;

				if (supEntryLine != null)
				{
					totalValue += supEntryLine.CL_CustomsValue;
				}
				else
				{
					var parentLine = Parent.ParentTariffLine;
					if (parentLine != null && parentLine.ImportTariff != null && parentLine.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
					{
						totalValue += parentLine.CusEntryLine != null ? parentLine.CusEntryLine.CL_CustomsValue : ZDecimal.Zero;
					}
				}

				if (Parent.ImportSupTariff != null && supEntryLine != null)
				{
					valueQuantityBoundValidator.ValidateUnitPriceLowerAndUpperBound(Parent.ImportSupTariff, supEntryLine, totalValue, Parent.JI_LinePriceInfo, "supplementary tariff");
				}
				valueQuantityBoundValidator.ValidateUnitPriceLowerAndUpperBound(Parent.ImportTariff, cusEntryLine, totalValue, Parent.JI_LinePriceInfo, "tariff");
			}
		}

		#endregion

		#region JI_Tariff

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			if (Parent.ImportTariff == null)
			{
				Parent.JI_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			var iorWrapper = Parent.IORWrapper;
			if (iorWrapper != null)
			{
				iorWrapper.RestrictedTariffs.CheckRestrictedCode(Parent.JI_TariffInfo);
			}
		}

		protected void CheckMIDAgainstCountryOfOrigin()
		{
			var tariff = Parent.ImportTariff;
			if (tariff != null && tariff.Applies(TariffRuleList.Codes.TextileEntryMID, Parent.EffectiveDateForDutyRate))
			{
				if (!Parent.US_UC_NKCountryOfOrigin.IsEmpty &&
					Parent.ManufacturerDetails != null &&
					Parent.ManufacturerDetails.MatchedCustomsRegoNumber.Left(2) != Parent.US_UC_NKCountryOfOrigin)
				{
					if (CanadaProvinceTerritoryCodes.IsCanadianProvince(Parent.ManufacturerDetails.MatchedCustomsRegoNumber.Left(2)) &&
						(CanadaProvinceTerritoryCodes.IsCanadianProvince(Parent.US_UC_NKCountryOfOrigin) ||
						Parent.US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Canada))
					{
						Parent.JI_TariffInfo.AddWarning(MIDNotMatchProvence);
					}
					else
					{
						Parent.JI_TariffInfo.AddMessageError(MIDNotMatchCountry);
					}
				}
			}
		}
		internal const string MIDNotMatchCountry = "For a Textile Entry the MID must match the Country of Origin.";
		internal const string MIDNotMatchProvence = "For a Textile Entry the MID must match the Country of Origin. The Canadian MID being used does not match the Canadian province of origin";
		#endregion

		#region JI_OA_ManufacturerAddress

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();
			if (ShouldCheckJI_OA_ManufacturerAddress)
			{
				OrganisationValidation.ValidateManufacturerIDForAddress
					(
						Parent.JI_OA_ManufacturerAddressInfo,
						ForceMIDToBeEntered,
						NotificationType.MessageError
					);
			}
		}

		protected virtual bool ForceMIDToBeEntered
		{
			get { return InvoiceLine.ManufacturerFallBackToSupplierNumber.IsEmpty; }
		}

		protected virtual bool ShouldCheckJI_OA_ManufacturerAddress
		{
			get { return true; }
		}

		#endregion

		#region JI_CustomsSecondQuantity

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();

			if (InvoiceLine.JI_CustomsSecondQuantity < 0m)
			{
				InvoiceLine.JI_CustomsSecondQuantityInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}
		}

		protected void CheckJI_CustomsSecondQuantityAgainstTariff()
		{
			if (Parent.ImportTariff != null)
			{
				var notificationType = Parent.IsChildLine ? NotificationType.Warning : NotificationType.MessageError;
				new ImportStatQtyValidator().Validate(Parent, Parent.JI_CustomsSecondQuantityInfo, Parent.JI_CustomsSecondUnitQty,
					() => Parent.ImportTariff.RequiresSecondQuantity(), QuantityCode.SecondQuantity, notificationType);
			}
		}

		#endregion

	}
}
