using CargoWise.Types;
using Enterprise.Customs.SG.V4.MHUB;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util
{
	public class ServerParameters
	{
		public ServerParameters(string getParamResultContent)
		{
			Salt = GetRTKeyFileCommand.ExtractRSATAGContentFromHtmlCumXml(getParamResultContent, "SALT");
			FsUserId = GetRTKeyFileCommand.ExtractRSATAGContentFromHtmlCumXml(getParamResultContent, "FSUSERID");
			FsUserHome = GetRTKeyFileCommand.ExtractRSATAGContentFromHtmlCumXml(getParamResultContent, "FSUSERHOME");
			FsDownload = GetRTKeyFileCommand.ExtractRSATAGContentFromHtmlCumXml(getParamResultContent, "FSDOWNLOAD");
			WsUuid = GetRTKeyFileCommand.ExtractRSATAGContentFromHtmlCumXml(getParamResultContent, "WSUUID");
		}

		public ZString Salt { get; private set; }
		public ZString FsUserId { get; private set; }
		public ZString FsUserHome { get; private set; }
		public ZString FsDownload { get; private set; }

		public string FullDownloadPathCalculated(string loginForFallbackInCaseServerDoesntGiveUsEnough, string soapDownloadDirectory)
		{
			return
			(FsUserHome.IsEmpty || FsUserId.IsEmpty || FsDownload.IsEmpty)
			?
			soapDownloadDirectory.Replace("<<LOGIN>>", loginForFallbackInCaseServerDoesntGiveUsEnough)
			:
			FsUserHome + FsUserId + FsDownload;
		}
		public string WsUuid { get; private set; }
	}
}
