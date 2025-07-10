using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	class RefDataRepoSender
	{
		public static class ContentType
		{
			public const string Edifact = "EDF";
			public const string Xml = "XML";
		}

		public static void SendIfNeeded(EDIMessage message, ZString subSource, string contentType)
		{
			if (message != null && !SGCustomsDataRegistry.Instance.SendTestMessages.Value)
			{
				CustomsGenericMessageHelper.CreatRefDbRepoMessage(message.Factory, Core.Constants.CountryCodes.Singapore, subSource, contentType, message.EM_MessageText, true);
			}
		}
	}
}
