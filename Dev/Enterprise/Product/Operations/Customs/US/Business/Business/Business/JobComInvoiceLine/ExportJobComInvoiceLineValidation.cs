using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (Parent.JI_CustomsQuantity > 99999999m)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(MaxValueExceeded);
			}

			if (Parent.US_IsUsedVehicle && Parent.JI_CustomsQuantity > 1)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(UsedVehicleQtyExceeded);
			}

			var declaration = Parent.Declaration;

			if (Parent.JI_CustomsUnitQty == Parent.JI_WeightUQ)
			{
				if (Parent.JI_CustomsQuantity > Parent.JI_Weight && declaration != null && !declaration.IsHandCarry)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(ExportWeightImbalance);
				}
			}
			else if (Parent.CanConvertCustomsQtyToKG && Parent.CustomsQuantityInKG > Parent.GrossWeightInKG)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(ExportEquivalentWeightImbalance);
			}
		}

		internal const string MaxValueExceeded = "The quantity to be reported exceeds the maximum allowed, (99,999,999), for this data element.";
		internal const string UsedVehicleQtyExceeded = "Qty for Used Vehicles must be 1. Reporting of multiple used vehicles requires a separate line for each vehicle.";
		internal const string ExportWeightImbalance = "Customs Quantity cannot exceed Gross Weight.";
		internal const string ExportEquivalentWeightImbalance = "Customs Quantity cannot exceed Gross Weight, (converted to equivalent Unit of Measure).";

		protected override ZString NoCustomsQuantityRequired
		{
			get { return "You have entered quantity without its unit."; }
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			var parent = Parent;
			var scheduleBTariff = parent.TariffExpirationDateWinin30Days;
			if (parent.UseScheduleB && scheduleBTariff != null && parent.JI_CustomsUnitQty != scheduleBTariff.ZZ1_ZZ8_UQ1)
			{
				parent.JI_CustomsUnitQtyInfo.AddMessageError(CustomsUnitQtyDoesNotMatchTariffUQ);
			}
		}
		internal const string CustomsUnitQtyDoesNotMatchTariffUQ = "The Customs UQ is not equal to the unit of quantity specified in the Schedule B tariff book.";

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo, Parent.Lookups.InvoiceUQList, (NoResString)InvoiceUQShouldBeInList);
		}
		internal const string InvoiceUQShouldBeInList = "Please enter a valid Invoice UQ Code. The code you have selected is not in the Invoice UQ codes List.";

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_NetWeightUQInfo, Parent.Lookups.WeightUQList, (NoResString)NetWeightUQShouldBeInList);
		}
		internal const string NetWeightUQShouldBeInList = "Please enter a valid Net Weight UQ Code. The code you have selected is not in the Net Weight UQ codes List.";

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.CheckNotNegative(Parent.JI_LinePriceInfo);

			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_LinePriceInfo);
			}

			if (Parent.JI_Tariff.StartsWith("88"))
			{
				if (Parent.JI_LinePrice > maxValueChapter88Goods)
				{
					Parent.JI_LinePriceInfo.AddMessageError(ValueExceedsLimitChapter88Goods);
				}
			}
			else if (Parent.JI_LinePrice > maxValueNonChapter88Goods)
			{
				Parent.JI_LinePriceInfo.AddMessageError(ValueExceedsLimitNonChapter88Goods);
			}

			ValidateJI_Weight();
		}
		internal const int maxValueChapter88Goods = 999999999;
		internal const int maxValueNonChapter88Goods = 499999999;
		internal const string ValueExceedsLimitChapter88Goods = "The Value of Goods cannot exceed the maximum threshold of $999,999,999 for any one commodity line item regardless of the Schedule B/HTS Number.";
		internal const string ValueExceedsLimitNonChapter88Goods = "The Value of Goods cannot be greater than $499,999,999 for any one commodity line item unless the Schedule B/HTS Number is in Chapter 88.";

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();

			var declaration = Parent.Declaration;
			if (Parent.IsWeightRequired() && declaration != null && !declaration.IsHandCarry)
			{
				if (Parent.EffectiveGrossWeight.InKilogramsSafe <= 0m)
				{
					Parent.JI_WeightInfo.AddMessageError(WeightRequired);
				}
				else if (declaration.IsSea && Parent.EffectiveGrossWeight.InKilogramsSafe > MaxVesselThreshold)
				{
					Parent.JI_WeightInfo.AddMessageError(VesselWeightThresholdExceeded);
				}
				else if (declaration.IsTruck && Parent.EffectiveGrossWeight.InKilogramsSafe > MaxTruckThreshold)
				{
					Parent.JI_WeightInfo.AddMessageError(TruckWeightThresholdExceeded);
				}
				else if (declaration.IsRail && Parent.EffectiveGrossWeight.InKilogramsSafe > MaxRailThreshold)
				{
					Parent.JI_WeightInfo.AddMessageError(RailWeightThresholdExceeded);
				}

				ValidateJI_CustomsQuantity();
			}
			else if (declaration != null && declaration.IsHandCarry && Parent.JI_Weight > 0)
			{
				Parent.JI_WeightInfo.AddMessageError(JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);
			}
		}

		internal const string WeightRequired = "Please enter a weight.";
		internal const string VesselWeightThresholdExceeded = "Shipment weight exceeds max Vessel threshold. The maximum Shipping Weight allowed for a vessel shipment is 200,000,000 kilograms.";
		internal const string TruckWeightThresholdExceeded = "Shipment Weight exceeds max Truck threshold. The maximum Shipping Weight allowed for a truck shipment is 25,000,000 kilograms.";
		internal const string RailWeightThresholdExceeded = "Shipment Weight exceeds max Rail threshold. The maximum Shipping Weight allowed for a rail shipment is 30,000,000 kilograms.";
		internal const int MaxVesselThreshold = 200000000;
		internal const int MaxTruckThreshold = 25000000;
		internal const int MaxRailThreshold = 30000000;

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();

			ValidateJI_Weight();
		}

		protected override bool IsTariffMandatory
		{
			get { return !(Parent.IsLimitedReportingExportCode) && base.IsTariffMandatory; }
		}

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			var useScheduleB = Parent.UseScheduleB;
			if ((useScheduleB ? Parent.ScheduleBTariff : Parent.ExportTariff) == null)
			{
				TariffValidator.ValidateWhenTariffViewIsNull(Parent, Parent.JI_TariffInfo, useScheduleB ? Universal.Constants.TariffTypes.ScheduleB : Universal.Constants.TariffTypes.Export, Parent.EffectiveDateForDutyRate);
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();

			var descriptionRequired = Parent.IsExportNMFSDeclared || Parent.IsATFDeclared || Parent.IsFWSDeclared;
			if (descriptionRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
			}

			var licenseType = Parent.US_LicenseType;
			var eccn = Parent.US_ECCN;
			if ((licenseType == "C67" || licenseType == "C68") && eccnsForDescriptionStartChecking.Contains(eccn) && !Parent.JI_Description.StartsWith(".Z"))
			{
				Parent.JI_DescriptionInfo.AddWarning(InvalidBeginningForGoodDescription);
			}
		}
		readonly ZString[] eccnsForDescriptionStartChecking = { "3A001", "4A003", "4A004", "4A005", "5A002", "5A004", "5A992", "5D002", "5D992" };
		internal const string InvalidBeginningForGoodDescription = "The Goods Description must begin with the characters .Z for License Type C67 and C68 if any of the following ECCNs are being used: 3A001, 4A003, 4A004, 4A005, 5A002, 5A004, 5A992, 5D002, or 5D992.";

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (Parent.NeedsSecondCustomsQuantity)
			{
				if (Parent.JI_CustomsSecondQuantity.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsSecondQuantityInfo);
				}
				else if (Parent.JI_CustomsSecondQuantity > 99999999m)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(MaxValueExceeded);
				}

				if (Parent.Declaration != null && !Parent.Declaration.IsHandCarry &&
				 Parent.JI_CustomsSecondUnitQty == AESUnitOfMeasureList.Codes.Kilograms && Parent.JI_CustomsSecondQuantity > Parent.GrossWeightInKG)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(QtyCannotExceedShipmentWeight);
				}
			}
			else if (Parent.JI_CustomsSecondQuantity > 0 && Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				Parent.JI_CustomsSecondQuantityInfo.AddMessageError(NoSecondQtyRequired);
			}
		}

		internal const string NoSecondQtyRequired = "You have entered quantity without its unit.";
		internal const string QtyCannotExceedShipmentWeight = "Quantity 2 cannot exceed the shipping weight, (Gross Weight).";

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			var parent = Parent;
			var scheduleBTariff = parent.TariffExpirationDateWinin30Days;
			if (parent.UseScheduleB && scheduleBTariff != null && parent.JI_CustomsSecondUnitQty != scheduleBTariff.ZZ1_ZZ8_UQ2)
			{
				parent.JI_CustomsSecondUnitQtyInfo.AddMessageError(SecondUQDoesNotMatchTariffUQ);
			}
		}
		internal const string SecondUQDoesNotMatchTariffUQ = "The Second UQ is not equal to the unit of quantity specified in the Schedule B tariff book.";
	}
}
