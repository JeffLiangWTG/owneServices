using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Freight.Forwarding.Business
{
	class AWBCalculationLogRateLineBuilder : AWBRateLineBuilder
	{
		public AWBCalculationLogRateLineBuilder(
				ExportAWBHeader exportAWBHeader,
				ZString containerCode,
				int rateLineNumber,
				bool isFirstSCACOnPreviousLine) : base(exportAWBHeader)
		{
			this.containerCode = containerCode;
			this.RateLineNumber = rateLineNumber;
			this.isFirstSCACOnPreviousLine = isFirstSCACOnPreviousLine;
		}

		readonly ZString containerCode;
		public int RateLineNumber { get; private set; }
		readonly bool isFirstSCACOnPreviousLine;
		bool isFirstContainerLine;

		protected override int GetRateLineNumber() => RateLineNumber + 1;

		protected override bool ShouldSCACOnPreviousLine() => isFirstContainerLine && isFirstSCACOnPreviousLine;

		public override ExportAWBRateLineCollection BuildRatelines(ExportAWBRateLineCollection awbRatelines)
		{
			isFirstContainerLine = true;
			var uldContainers = GetULDContainers(containerCode);
			var tareWeightInKg = uldContainers.Sum(x => x.JC_TareWeight);
			var totalTareWeight = GetConvertedValue(tareWeightInKg, Core.Constants.Weight.Kilograms);

			foreach (var uldContainer in uldContainers)
			{
				BuildRateline(uldContainer, ref isFirstContainerLine, totalTareWeight);
			}
			return null;
		}

		protected override bool SecurityStatusAWBVisibility
		{
			get { return Parent.SecurityStatusAWBVisibility; }
		}

		protected IEnumerable<CommonContainer> GetULDContainers(ZString containerCode)
		{
			return Parent.ULDContainers != null ? Parent.ULDContainers.Where(x => x.RefContainer != null && x.RefContainer.RC_Code == containerCode) : Enumerable.Empty<CommonContainer>();
		}

		protected override int GetRequiredLinesForContainer(CommonContainer uldContainer)
		{
			return Parent.ShouldPopulateSlacLine(uldContainer) ? 2 : 1;
		}

		protected override void OnLinePopulated(int rateLineNumber)
		{
			RateLineNumber = rateLineNumber;
		}

		#region Helper Methods

		ZDecimal GetConvertedValue(ZDecimal sourceValue, ZString sourceWeightUnit)
		{
			ZString targetWeightUnit = Core.Constants.Weight.IsImperial(sourceWeightUnit) ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
			return Core.Constants.Weight.Convert(sourceValue, sourceWeightUnit, targetWeightUnit);
		}

		#endregion
	}
}
