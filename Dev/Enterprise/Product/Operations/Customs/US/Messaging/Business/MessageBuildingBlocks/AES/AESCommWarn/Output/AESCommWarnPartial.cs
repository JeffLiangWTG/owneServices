using CargoWise.Types;
using AESApplicationIdentifierCodeList = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.AES;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output
{
	partial class AESCommWarnAXN : MessageBlock, IAESControlMessageBlockA
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

	partial class AESCommWarnBXN : MessageBlock, IAESControlMessageBlockB
	{
		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return AESApplicationIdentifierCodeList.CommodityShipmentWarningReminder; }
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

	partial class AESCommWarnYXN : MessageBlock, IAESControlMessageBlockY
	{
	}

	partial class AESCommWarnZXN : MessageBlock, IAESControlMessageBlockZ
	{
	}

	partial class AESCommWarnES1XN : MessageBlock, IAESTIRES1MessageBlock
	{
		#region IAESTIRES1MessageBlock Members

		ZString IAESTIRES1MessageBlock.ResponseCode
		{
			get { return ResponseCode; }
		}

		ZString IAESTIRES1MessageBlock.FinalDispositionIndicator
		{
			get { return FinalDispositionIndicator; }
		}

		ZString IAESTIRES1MessageBlock.SeverityIndicator
		{
			get { return SeverityIndicator; }
		}

		ZString IAESTIRES1MessageBlock.NarrativeText
		{
			get { return NarrativeText; }
		}

		ZString IAESTIRES1MessageBlock.AESInternalTransactionNumberITN
		{
			get { return AESInternalTransactionNumberITN; }
		}

		ZString IAESTIRES1MessageBlock.ReasonCode
		{
			get { return ReasonCode; }
		}

		#endregion
	}
}
