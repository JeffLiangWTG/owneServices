using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public class MeasureInformation
	{
		public MeasureInformation(string tradeGroup, string additionalCode)
		{
			TradeGroup = Argument.NotNull(tradeGroup, nameof(tradeGroup));
			AdditionalCode = Argument.NotNull(additionalCode, nameof(additionalCode));
		}

		public string TradeGroup { get; }

		public string AdditionalCode { get; }
	}
}
