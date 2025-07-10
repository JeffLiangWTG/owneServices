using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class CustomsResponseProcessorProvider
	{
		public static BaseCustomsResponseProcessor GetCustomsResponseProcessor(string tableName, ILogger logger)
			=> tableName switch
			{
				Constants.TableNames.TradeGroups => new TradeGroupsCustomsResponseProcessor(new DateTimeProvider(), logger),
				Constants.TableNames.TradeGroupCountries => new TradeGroupCountriesCustomsResponseProcessor(new DateTimeProvider(), logger),
				_ => new ILCustomsCodesProcessor(logger),
			};
	}
}
