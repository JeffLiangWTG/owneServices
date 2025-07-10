using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBRateLineValidation : ExportAWBRateLineValidation
	{
		public ConsolExportAWBRateLineValidation(ConsolExportAWBRateLine parent) : base(parent)
		{
		}

		public new ConsolExportAWBRateLine Parent
		{
			get { return (ConsolExportAWBRateLine)base.Parent; }
		}

		protected override void CheckER_GrossWeight()
		{
			base.CheckER_GrossWeight();

			if (Parent.ER_LineCount != 1 && Parent.ER_WeightInLBsOrKGs != "")
			{
				if (Parent.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation
					&& Parent.ER_GrossWeight == 0)
				{
					if (!Parent.ER_CommodityItemNumber.IsEmpty && !FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.Value)
					{
						Parent.ER_GrossWeightInfo.AddWarning(Res.GetString("87202b44-f264-471a-b49b-debc65cdea62",
							"You have not entered a tare weight for your ULD, which is a requirement as per the IATA Rules. If you choose to send your FWB data without the tare weight, there is a chance that the airline may reject your message. We suggest you provide a weight."));
					}
				}
				else
				{
					MandatoryValidation.WarnIfIsZero(Parent.ER_GrossWeightInfo);
				}
			}
		}

		protected override void CheckER_ChargeableWeight()
		{
			base.CheckER_ChargeableWeight();

			if (Parent.ER_ChargeableWeight > 0)
			{
				var consolExportAWBHeader = Parent.Master as ConsolExportAWBHeader;
				if (consolExportAWBHeader != null)
				{
					if (consolExportAWBHeader.ConsolChargeableWeight != Parent.ER_ChargeableWeight)
					{
						var unitName = Parent.ER_WeightInLBsOrKGs == Constants.AWB.RateLineUQ.Kilos ? Constants.Weight.Kilograms : Constants.Weight.Pounds;
						Parent.ER_ChargeableWeightInfo.AddWarning(Res.GetString("B2D5CE77-AB2E-43D9-B0DD-03EC1AB87B1B", "Favorable weight {0}{1}, calculated from Autorating using Higher Break Lower Rate, is used as the Chargeable Weight for printing AWB. Refer to Achieved Quantities tab per the Weight and Volume utilization of the Consol.", Parent.ER_ChargeableWeight, unitName));
					}
				}
			}
		}

		protected override void CheckER_WeightInLBsOrKGs()
		{
			base.CheckER_WeightInLBsOrKGs();

			if (Parent.ER_LineCount != 1 && Parent.ER_GrossWeight != 0)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ER_WeightInLBsOrKGsInfo);
			}
		}

		protected override void CheckER_NoOfPiecesOrRCP()
		{
			base.CheckER_NoOfPiecesOrRCP();

			if (!Parent.ER_NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty && (!Parent.ER_NoOfPiecesOrRCP.IsLettersOnlyOrEmpty || Parent.ER_NoOfPiecesOrRCP.Length != 3))
			{
				Parent.ER_NoOfPiecesOrRCPInfo.AddMessageError(Res.GetString("6827d705-bb85-4324-9bb7-e6a41c7e9104", "Number of pieces must be a number up to 4 digits or a 3 letter RCP code"));
			}
		}

		protected override void CheckER_RateClass()
		{
			base.CheckER_RateClass();
			ListValidation.MessageErrorIfInvalidCode(Parent.ER_RateClassInfo, Parent.RateClassList);
		}

		protected override void CheckER_NatureAndQtyOfGoodsType()
		{
			base.CheckER_NatureAndQtyOfGoodsType();

			if (Parent.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation
				&& Parent.ER_NatureAndQtyOfGoodsType != Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber)
			{
				Parent.ER_NatureAndQtyOfGoodsTypeInfo.AddMessageError(Res.GetString("644f5798-c195-4fdf-a054-cb4cdddb3bf8", "{0} must be '{1}' if {2} of '{3}' has been selected.",
					Parent.ER_NatureAndQtyOfGoodsTypeInfo.HumanReadableName,
					Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber,
					Parent.ER_RateClassInfo.HumanReadableName,
					Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation));
			}
		}

		protected override void CheckER_CommodityItemNumber()
		{
			base.CheckER_CommodityItemNumber();

			if (Parent.ER_CommodityItemNumber.IsEmpty)
			{
				switch (Parent.ER_RateClass)
				{
					case (Constants.AWB.RateClass.SpecificCommodityRate):
					case (Constants.AWB.RateClass.UnitLoadDeviceBasicCharge):
						Parent.ER_CommodityItemNumberInfo.AddWarning(Res.GetString("0f121dde-7fa1-469b-bc80-eb47f9067f0e", "The Commodity Item Number is optional for Rate Class '{0}'.", Parent.ER_RateClass));
						return;
					case (Constants.AWB.RateClass.ClassRateReduction):
						Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("62ec9920-2030-ff87-42c1-b3dda8c52e50", "When Rate Class is 'R', the rate reduction percentage is required, preceded by the rate class to which it relates. For example: A 30% reduction of the normal rate would be N70."));
						return;
					case (Constants.AWB.RateClass.ClassRateSurcharge):
						Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("9adaa937-9778-109e-4b43-1217b5038052", "When Rate Class is 'S', the rate surcharge percentage is required, preceded by the rate class to which it relates. For example: A 50% surcharge of the minimal rate would be M150."));
						return;
				}

				if (Parent.ER_RateClass == Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation &&
					Parent.NatureAndQtyOfGoodsType == Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber &&
					Parent.ER_GrossWeight != 0)
				{
					Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("f9447e03-99ca-0d85-485a-568acef726e2", "When Rate Class is 'X', the ULD rate class type is required."));
					return;
				}
			}

			switch (Parent.ER_RateClass)
			{
				case Constants.AWB.RateClass.SpecificCommodityRate:
				case Constants.AWB.RateClass.UnitLoadDeviceBasicCharge:
					if (!Regex.IsMatch(Parent.ER_CommodityItemNumber, @"^\d{4,7}$"))
					{
						Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("6d670ecc-97f3-4ced-bbbe-de79c61267cc", "Enter a Commodity Item Number between 4 and 7 digits long."));
					}
					else
					{
						var query = new ZQuery(RefAirlineCommodityCodeSchema.RAC_Code, Parent.ER_CommodityItemNumber);
						query.AddToFilter(RefAirlineCommodityCodeSchema.RAC_AirlineID, SQLComparisonOperator.Equal, ZString.Empty);
						if (Parent.Factory.LoadTop1<RefAirlineCommodityCode>(query) == null)
						{
							Parent.ER_CommodityItemNumberInfo.AddWarning(Res.GetString("095FBD0A-D05B-465C-B038-D26EC226AA06", "This code does not match IATA Specific Commodity."));
						}
					}
					break;
				case Constants.AWB.RateClass.ClassRateReduction:
				case Constants.AWB.RateClass.ClassRateSurcharge:
					if (!(Regex.IsMatch(Parent.ER_CommodityItemNumber, @"^[A-Z]\d{1,3}$")
						&& Parent.RateClassList.ContainsCode(Parent.ER_CommodityItemNumber.Substring(0, 1))))
					{
						Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("3318711f-5b82-b0a1-43fb-d83e52618f69", "As Rate Class is 'R' or 'S', enter the letter from Rate Class, followed by the percentage rate that applies to that Rate (up to three numeric characters)."));
					}
					break;
				case Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation:
					ListValidation.MessageErrorIfInvalidCode(Parent.ER_CommodityItemNumberInfo, (CodeDescriptionPairList)Parent.ER_CommodityItemNumberList);
					break;
				default:
					if (!Parent.ER_CommodityItemNumber.IsEmpty && !Regex.IsMatch(Parent.ER_CommodityItemNumber, "^[0-9]{4}[0-9]{0,3}$|^[0-9][A-Z]{0,2}$|^[A-Z][0-9]{0,3}$"))
					{
						//allowable format n[4..7], n(a)(a), an[..3]
						Parent.ER_CommodityItemNumberInfo.AddMessageError(Res.GetString("E5D45451-A2A4-455F-97BE-97AE38C80782", "Allowable formats:\r\n 4-7 numerics (Commodity Item Number)\r\n 1 numeric followed by 2 optional alpha characters (ULD Rate Class Type)\r\n 1 alpha character followed by 3 optional numerics (Rate Class Code + Class Rate Percentage)"));
					}
					break;
			}
		}
	}
}
