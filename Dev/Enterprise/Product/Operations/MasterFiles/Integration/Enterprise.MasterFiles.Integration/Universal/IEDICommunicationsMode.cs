using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IEDICommunicationsMode : IMessageDestinationSource
	{
		ZGuid EK_ECC_CommunicationPartyConfig { get; set; }
		ZString EK_CommunicationsTransport { get; }
		ZString EK_Destination { get; }
		ZString EK_FileFormat { get; }
		ZString EK_Filename { get; }
		ZDateTime EK_LastFailed { get; set; }
		ZString EK_LocalPartyVanID { get; }
		ZString EK_MessagePurpose { get; }
		ZString EK_RelatedPartyVanID { get; }
		ZString EK_ServerAddressSubject { get; }
		ZString EK_LoginName { get; }
		ZString EK_Password { get; }
		ZInt EK_PortNumber { get; }
		ZBool EK_PublishInternalMilestones { get; }
	}
}
