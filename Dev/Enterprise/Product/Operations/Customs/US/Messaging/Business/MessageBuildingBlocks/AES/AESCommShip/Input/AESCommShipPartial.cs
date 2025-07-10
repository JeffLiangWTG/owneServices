using CargoWise.Types;
using AESApplicationIdentifierCodeList = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.AES;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	partial class AESCommShipAXP : MessageBlock, IAESControlMessageBlockA
	{
		#region IControlMessageBlockA Members

		ZString IControlMessageBlockA.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
		}

		ZString IControlMessageBlockA.FilerID
		{
			get { return FilerID; }
			set { FilerID = value; }
		}

		#endregion

		#region IAESControlMessageBlockA Members

		ZString IAESControlMessageBlockA.BatchControlNumber
		{
			get { return BatchControlNumber; }
			set { BatchControlNumber = value; }
		}

		#endregion
	}

	partial class AESCommShipBXP : AESCommShipBXPBase, IAESControlMessageBlockB
	{
		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return AESApplicationIdentifierCodeList.CommodityShipment; }
			set { }
		}

		#endregion

		#region IAESControlMessageBlockB Members

		ZString IAESControlMessageBlockB.USPPIID
		{
			get { return USPPIID; }
			set { USPPIID = value; }
		}

		ZString IAESControlMessageBlockB.USPPIIDType
		{
			get { return USPPIIDType; }
			set { USPPIIDType = value; }
		}

		ZString IAESControlMessageBlockB.USPPIName
		{
			get { return USPPIName; }
			set { USPPIName = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("PGA")]
	partial class AESCommShipPGAXP : MessageBlock
	{
	}

	partial class AESCommShipYXP : AESCommShipYXPBase, IAESControlMessageBlockY
	{
	}

	partial class AESCommShipZXP : MessageBlock, IAESControlMessageBlockZ
	{
	}
}
