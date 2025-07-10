using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class NonPersistentEDICommunicationMode : IEDICommunicationsMode
	{
		public ZGuid EK_ECC_CommunicationPartyConfig { get; set; }
		public ZString EK_CommunicationsTransport { get; set; }
		public ZString EK_CommsDirection { get; set; }
		public ZString EK_Destination { get; set; }
		public ZString EK_FileFormat { get; set; }
		public ZString EK_Filename { get; set; }
		public ZDateTime EK_LastFailed { get; set; }
		public ZString EK_LocalPartyVanID { get; set; }
		public ZString EK_MessagePurpose { get; set; }
		public ZString EK_RelatedPartyVanID { get; set; }
		public ZString EK_ServerAddressSubject { get; set; }
		IOrgHeader IMessageDestinationSource.Organisation => Organisation;
		public OrgHeader Organisation { get; set; }
		public ZString EK_LoginName { get; set; }
		public ZString EK_Password { get; set; }
		public ZInt EK_PortNumber { get; set; }
		public ZBool EK_PublishInternalMilestones { get; set; }
	}
}
