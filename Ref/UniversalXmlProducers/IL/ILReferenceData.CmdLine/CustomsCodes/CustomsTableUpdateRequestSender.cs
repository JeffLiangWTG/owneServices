using System.Threading.Tasks;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.xTMessaging.Integration;
using Constants = CargoWise.RefDbRepo.ILReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	class CustomsTableUpdateRequestSender : BaseSending<ISYSTBL_NG_9000_MSG_SystemTableRequest>
	{
		internal static async Task<bool> Run(ILogger logger, string[] cmdLineArguments)
		{
			var customsTableUpdater = new CustomsTableUpdateRequestSender();
			return await customsTableUpdater.Send(logger, cmdLineArguments);
		}
		protected override bool ArgumentParserCore(string[] cmdLineArguments, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] requestParam, out string message)
			=> CustomsTableArgumentsParser.TryParse(cmdLineArguments,
				out requestParam,
				out message);

		protected override string ProviderRequestXmlCore(ISYSTBL_NG_9000_MSG_SystemTableRequest requestParam)
		{
			var customsCodesRequest = new CustomsCodesRequestProvider(requestParam);
			var requestXml = customsCodesRequest.GetSystemTableRequest();
			return requestXml;
		}

		protected override string MessageSubTypeCore() => Constants.MessageSubType.CustomCodes;

	}
}
