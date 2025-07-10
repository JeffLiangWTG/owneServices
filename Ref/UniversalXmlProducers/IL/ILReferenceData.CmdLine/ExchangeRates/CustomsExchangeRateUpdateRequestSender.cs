using System.Threading.Tasks;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.CmdLine.ExchangeRates;
using CargoWise.xTMessaging.Integration;
using Constants = CargoWise.RefDbRepo.ILReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	class CustomsExchangeRateUpdateRequestSender : BaseSending<ICurrencyRateSearchParam>
	{
		internal static async Task<bool> Run(ILogger logger, string[] cmdLineArguments)
		{
			var customsExchangeRateUpdater = new CustomsExchangeRateUpdateRequestSender();
			return await customsExchangeRateUpdater.Send(logger, cmdLineArguments);
		}

		protected override bool ArgumentParserCore(string[] cmdLineArguments, out ICurrencyRateSearchParam[] requestParams, out string message)
		{
			var res = CustomsExchangeArgumentsParser.TryParse(cmdLineArguments, out ICurrencyRateSearchParam requestParam, out message);
			requestParams = new ICurrencyRateSearchParam[] { requestParam };

			return res;
		}

		protected override string ProviderRequestXmlCore(ICurrencyRateSearchParam requestParam)
		{
			var customsCodesRequest = new ExchangeRatesRequestProvider(requestParam);
			var requestXml = customsCodesRequest.GetRequestXml();
			return requestXml;
		}

		protected override string MessageSubTypeCore() => Constants.MessageSubType.ExRate;
	}
}
