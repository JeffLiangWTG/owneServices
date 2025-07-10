using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Freight.Forwarding.Business
{
	class AWBAdditionalLooseRatelineBuilder : AWBRateLineBuilder
	{
		public AWBAdditionalLooseRatelineBuilder(ExportAWBHeader parent, ZDecimal rateLineGrossWeight, ZString rateLineWeightUnit, ZDecimal rateLineChargeableWeight) : base(parent)
		{
			this.rateLineGrossWeight = rateLineGrossWeight;
			this.rateLineWeightUnit = rateLineWeightUnit;
			this.rateLineChargeableWeight = rateLineChargeableWeight;
		}

		public override ExportAWBRateLineCollection BuildRatelines(ExportAWBRateLineCollection awbRatelines)
		{
			var rateLineNumber = GetRateLineNumber();
			var rateLine = Parent.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)rateLineNumber];

			if (rateLine != null)
			{
				using (rateLine.SuspendSettingHasChanges())
				{
					rateLine.Clear();
					rateLine.ER_NoOfPiecesOrRCP = LooseRateLineNoPieces;
					rateLine.ER_GrossWeight = rateLineGrossWeight;
					rateLine.ER_WeightInLBsOrKGs = rateLineWeightUnit;
					rateLine.ER_ChargeableWeight = rateLineChargeableWeight;

					Parent.PopulateNatureAndQtyOfGoodsLine(rateLineNumber, LoosePackCount + " " + ExportAWBHeader.Constants.SLAC);
					Parent.PopulateNatureAndQtyOfGoodsType(rateLineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount);
				}
				OnLinePopulated(rateLineNumber);
			}

			return awbRatelines;
		}

		int LoosePackCount
		{
			get => Parent.Consol?.UnAllocatedPackLines.Cast<PackLine>().Sum(packLine => packLine.JL_PackageCount) ?? 0;
		}

		ZString LooseRateLineNoPieces
		{
			get
			{
				ZInt result = LoosePackCount;
				result = Math.Min(result, 9999);

				return result.ToString();
			}
		}

		readonly ZDecimal rateLineGrossWeight;
		readonly ZString rateLineWeightUnit;
		readonly ZDecimal rateLineChargeableWeight;
	}
}
