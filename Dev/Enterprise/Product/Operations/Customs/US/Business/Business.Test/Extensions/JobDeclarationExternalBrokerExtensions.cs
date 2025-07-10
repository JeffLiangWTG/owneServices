using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public static class JobDeclarationExternalBrokerExtensions
	{
		public static void SetExternalBrokerForTesting(this JobDeclaration declaration)
		{
			OrgHeader externalBroker = declaration.Factory.NewWithValidTestData<OrgHeader>();
			EDICommunicationsMode mode = externalBroker.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@ema.com";
			mode.EK_ServerAddressSubject = "EmailAsAttchMode";
			mode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;

			declaration.JE_OH_ExternalBroker = externalBroker.PK;
		}
	}
}
