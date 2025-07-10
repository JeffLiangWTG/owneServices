using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class TradeGroupBuilder : TradeGroupBuilderBase
	{
		public TradeGroupBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string DataGrouping => Constants.Common.LocalCountryCode;

		protected override string FilePrefix => "BE_RefCusTradeGroup";

		protected override string XMLWriterDataSource => Constants.DataSources.BeGeographicalAreas;

		protected override bool SupportsMultipleLanguages => true;
	}
}
