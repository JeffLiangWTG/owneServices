using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.UY.Manifest.Business
{
	internal class DAEWrappersHelper
	{
		internal static ZString GetDAEObjectOriginal(AsycudaManifestHeader header)
		{
			var lastOriginalMessage = FindLastOriginalMessage(header);
			if (lastOriginalMessage != null)
			{
				return lastOriginalMessage.EM_MessageText;
			}
			return null;
		}

		static UYMessage FindLastOriginalMessage(AsycudaManifestHeader header)
		{
			return header.Messages.Cast<UYMessage>()
				.Where(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit
				&& x.EM_ApplicationCode == ApplicationCodeList.Codes.UYCustoms
				&& x.EM_Status == EDIMessage.Status.Sent).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}
	}
}
