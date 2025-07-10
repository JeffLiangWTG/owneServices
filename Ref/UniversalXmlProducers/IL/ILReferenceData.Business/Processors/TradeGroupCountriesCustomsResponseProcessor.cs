using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class TradeGroupCountriesCustomsResponseProcessor : BaseCustomsResponseProcessor
	{
		public TradeGroupCountriesCustomsResponseProcessor(IDateTimeProvider dateTimeProvider, ILogger logger) : base(logger)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		protected override string GetDataSourceName(string tableName) => Constants.DataSetNames.TradeGroupCountries;

		protected override string GetOutputFileName(string dataSourceName) => $"{base.GetOutputFileName(dataSourceName)}_{dateTimeProvider.GetUTCNow():yyyyMMddHHmmss}";

		protected override string OutputFolderPath => ApplicationConfig.Instance.DownloadsDirectory;

		IDateTimeProvider dateTimeProvider;
	}
}
