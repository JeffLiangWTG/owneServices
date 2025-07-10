using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class TradeGroupBuilder : TradeGroupBuilderBase
	{
		public TradeGroupBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "GB_RefCusTradeGroup";

		protected override string XMLWriterDataSource => "GB Trade Group";

		protected override string DataGrouping => Constants.DefaultValues.GBDataGrouping;
	}
}
