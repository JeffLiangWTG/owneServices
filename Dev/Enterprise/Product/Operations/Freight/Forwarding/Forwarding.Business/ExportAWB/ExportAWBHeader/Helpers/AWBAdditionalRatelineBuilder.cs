using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Freight.Forwarding.Business
{
	class AWBAdditionalRateLineBuilder : AWBRateLineBuilder
	{
		public AWBAdditionalRateLineBuilder(
				ExportAWBHeader parent,
				ZString dischargePortCountryCode,
				bool[] linesPopulated) : base(parent)
		{
			this.linesPopulated = linesPopulated;
		}

		readonly bool[] linesPopulated;

		public override ExportAWBRateLineCollection BuildRatelines(ExportAWBRateLineCollection awbRatelines)
		{
			var containerGroupByType = from container in Parent.ULDContainers group container by container.JC_RC;

			foreach (var containerGroup in containerGroupByType)
			{
				var isFirstRateLine = true;
				var totalTareWeight = containerGroup.Sum(x => x.JC_TareWeight);
				foreach (CommonContainer uldContainer in containerGroup)
				{
					BuildRateline(uldContainer, ref isFirstRateLine, totalTareWeight);
				}
			}

			return awbRatelines;
		}

		protected override ZString RateLineWeightUnit
		{
			get { return (Parent.Consol != null && Parent.Consol.JK_TotalShipmentWeightUnit == Core.Constants.Weight.Pounds) ? "L" : "K"; }
		}

		protected override void OnLinePopulated(int rateLineNumber)
		{
			linesPopulated[rateLineNumber - 1] = true;
		}
	}
}
