using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class TradeGroupsCustomsResponseProcessor : BaseCustomsResponseProcessor
	{
		public TradeGroupsCustomsResponseProcessor(IDateTimeProvider dateTimeProvider, ILogger logger) : base(logger)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		protected override string GetDataSourceName(string tableName) => Constants.DataSetNames.TradeGroups;

		protected override string GetOutputFileName(string dataSourceName) => $"{base.GetOutputFileName(dataSourceName)}_{dateTimeProvider.GetUTCNow():yyyyMMddHHmmss}";

		protected override string OutputFolderPath => ApplicationConfig.Instance.DownloadsDirectory;

		readonly IDateTimeProvider dateTimeProvider;
	}
}
