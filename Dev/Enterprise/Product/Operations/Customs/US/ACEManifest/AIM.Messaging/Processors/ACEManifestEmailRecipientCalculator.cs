using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public sealed class ACEManifestEmailRecipientCalculator : EmailRecipientCalculator
	{
		public ACEManifestEmailRecipientCalculator(ZString sendMode, ZGuid groupSendPK, GlbStaff userToNotify, ZGuid alternativeGroupPKIfNoRecipientFound)
			: base(sendMode, groupSendPK, userToNotify, alternativeGroupPKIfNoRecipientFound)
		{
		}

		protected override bool IsSystemCommunication => false;
		protected override IOutgoingMailManager OutgoingMailManager => Env.OutgoingMailManager;
	}
}
