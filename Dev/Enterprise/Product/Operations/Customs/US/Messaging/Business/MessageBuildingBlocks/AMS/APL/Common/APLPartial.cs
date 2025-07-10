
using CargoWise.Types;
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, Constants.ACE)]
	public partial class APLACR : MessageBlock, IAMSControlMessageBlockA, IAMSControlMessageBlockB
	{
		#region IAMSControlMessageBlockA Members

		ZString IAMSControlMessageBlockA.Password
		{
			set { Password = value; }
		}

		#endregion

		#region IControlMessageBlockA Members

		ZString IControlMessageBlockA.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
		}

		ZString IControlMessageBlockA.FilerID
		{
			get { return AMSUserCode; }
			set { AMSUserCode = value; }
		}

		#endregion

		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, Constants.ACE)]
	public partial class APLZCR : MessageBlock, IAMSControlMessageBlockY, IAMSControlMessageBlockZ
	{
	}
}
