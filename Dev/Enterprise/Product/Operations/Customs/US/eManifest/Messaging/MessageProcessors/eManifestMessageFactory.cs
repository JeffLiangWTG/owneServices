using Enterprise.Edifact;
using Enterprise.Edifact.D08A;
using Enterprise.Edifact.D08A.Messages.CONTRL;
using Enterprise.Edifact.D08A.Messages.CUSCAR;
using Enterprise.Edifact.D08A.Messages.CUSREP;
using Enterprise.Edifact.D08A.Messages.CUSRES;
using Enterprise.Edifact.D08A.Messages.MEDPID;
using Enterprise.Edifact.D08A.Messages.PAXLST;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class eManifestMessageFactory : MessageFactory
	{
		public eManifestMessageFactory()
			: base(new D08AMessageFactory())
		{
			AddRegisteredMessage(new MessageRegistration(typeof(CUSCARMessage), "UN", "D", "03B", "CUSCAR"));
			AddRegisteredMessage(new MessageRegistration(typeof(CUSREPMessage), "UN", "D", "03B", "CUSREP"));
			AddRegisteredMessage(new MessageRegistration(typeof(CONTRLMessage), "UN", "D", "3", "CONTRL"));
			AddRegisteredMessage(new MessageRegistration(typeof(CONTRLMessage), "UN", "D", "03B", "CONTRL"));
			AddRegisteredMessage(new MessageRegistration(typeof(CONTRLMessage), "UN", "D", "00B", "CONTRL"));
			AddRegisteredMessage(new MessageRegistration(typeof(CUSRESMessage), "UN", "D", "03B", "CUSRES"));
			AddRegisteredMessage(new MessageRegistration(typeof(PAXLSTMessage), "UN", "D", "03B", "PAXLST"));
			AddRegisteredMessage(new MessageRegistration(typeof(MEDPIDMessage), "UN", "D", "02A", "MEDPID"));
		}
	}
}
