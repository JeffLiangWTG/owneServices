using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Forwarding.Business.AWB.CalculationLogsAnalyzer;

namespace Enterprise.Freight.Forwarding.Business
{
	abstract class AWBRateLineBuilder
	{
		public AWBRateLineBuilder(ExportAWBHeader parent)
		{
			Argument.NotNull(parent, nameof(ExportAWBHeader));
			this.Parent = parent;
		}

		public abstract ExportAWBRateLineCollection BuildRatelines(ExportAWBRateLineCollection awbRatelines);

		protected ExportAWBHeader Parent { get; private set; }

		protected virtual ZString RateLineWeightUnit
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected virtual bool SecurityStatusAWBVisibility => Parent.SecurityStatusAWBVisibility;

		protected void BuildRateline(
			CommonContainer uldContainer,
			ref bool isFirstRateLine,
			decimal grossWeight)
		{
			int ratelineNumber = GetRateLineNumber();
			if (ratelineNumber > 0 && ratelineNumber < ExportAWBHeader.Constants.NumberOfRateLines)
			{
				if (!Parent.ShouldSuppressSlacLines() && Parent.ShouldPopulateSlacLine(uldContainer))
				{
					var scacLine = ShouldSCACOnPreviousLine() ? ratelineNumber - 1 : ratelineNumber;
					PopulateSlacLine(scacLine, uldContainer);
					ratelineNumber = scacLine + 1;
				}

				var rateClass = Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation;
				var rateLine = PopulateLine(ratelineNumber, "", 0m, RateLineWeightUnit, ZString.Empty, rateClass, 0m, 0m);

				using (rateLine.SuspendSettingHasChanges())
				{
					if (isFirstRateLine)
					{
						rateLine.ER_CommodityItemNumber = uldContainer.RefContainer != null ? uldContainer.RefContainer.RC_IATARateClass : ZString.Empty;
						if (!FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.Value)
						{
							rateLine.ER_GrossWeight = grossWeight;
						}
						isFirstRateLine = false;
					}
					else
					{
						rateLine.ER_CommodityItemNumber = ZString.Empty;
						rateLine.ER_GrossWeight = 0;
					}

					Parent.PopulateNatureAndQtyOfGoodsLine(ratelineNumber, uldContainer.JC_ContainerNum);
					Parent.PopulateNatureAndQtyOfGoodsType(ratelineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber);
				}

				OnLinePopulated(ratelineNumber);

				if (SecurityStatusAWBVisibility)
				{
					PopulateULDSecurityStatusLine(uldContainer);
				}
			}
		}

		protected virtual int GetRequiredLinesForContainer(CommonContainer uldContainer) => 0;

		protected virtual void OnLinePopulated(int rateLineNumber)
		{ }

		protected virtual ExportAWBRateLine PopulateLine(
								int lineNumber,
								ZString numberOfPieces,
								ZDecimal grossWeight,
								ZString ratelineWeightUnit,
								ZString commodityItemNumber,
								ZString rateClass,
								ZDecimal chargeableWeight,
								ZDecimal rateCharge)
		{
			if (lineNumber > ExportAWBHeader.Constants.NumberOfRateLines)
			{
				throw new MaximumRatelineCountExceededException();
			}

			var rateLine = Parent.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
			using (rateLine.SuspendSettingHasChanges())
			{
				rateLine.ER_NoOfPiecesOrRCP = numberOfPieces;
				rateLine.ER_GrossWeight = grossWeight;
				rateLine.ER_WeightInLBsOrKGs = ratelineWeightUnit;
				rateLine.ER_CommodityItemNumber = commodityItemNumber;
				rateLine.ER_RateClass = rateClass;
				rateLine.ER_ChargeableWeight = chargeableWeight;
				rateLine.ER_RateChargeOrDiscount = rateCharge;
			}

			rateLine.MarkAsNeedingValidation();

			return rateLine;
		}

		#region Helper Methods

		protected virtual int GetRateLineNumber()
		{
			return Math.Max(Parent.LineNumberOfFirstEmptyRateLine, Parent.LineNumberOfFirstEmptyNatureAndQtyOfGoods);
		}

		protected virtual bool ShouldSCACOnPreviousLine() => false;

		void PopulateULDSecurityStatusLine(CommonContainer uldContainer)
		{
			int rateLineNumber = GetRateLineNumber();

			if (rateLineNumber < ExportAWBHeader.Constants.NumberOfRateLines)
			{
				var rateLine = Parent.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)rateLineNumber];
				if (rateLine != null)
				{
					using (rateLine.SuspendSettingHasChanges())
					{
						Parent.PopulateNatureAndQtyOfGoodsLine(rateLineNumber, Parent.GetULDAviationSecurityStatus(uldContainer));
						Parent.PopulateNatureAndQtyOfGoodsType(rateLineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription);
					}

					OnLinePopulated(rateLineNumber);
				}
			}
		}

		void PopulateSlacLine(int ratelineNumber, CommonContainer uldContainer)
		{
			var slacLine = Parent.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)ratelineNumber];
			using (slacLine.SuspendSettingHasChanges())
			{
				Parent.PopulateNatureAndQtyOfGoodsLine(ratelineNumber, uldContainer.JC_Calc_TotalPackages + " " + ExportAWBHeader.Constants.SLAC);
				Parent.PopulateNatureAndQtyOfGoodsType(ratelineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount);
			}
			OnLinePopulated(ratelineNumber);
		}

		#endregion
	}
}
