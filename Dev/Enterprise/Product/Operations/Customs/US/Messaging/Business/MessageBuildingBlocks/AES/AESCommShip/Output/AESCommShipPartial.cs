using CargoWise.Types;
using AESApplicationIdentifierCodeList = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.AES;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output
{
	partial class AESCommShipAXT : MessageBlock, IAESControlMessageBlockA
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

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("B")]
	public partial class AESCommShipBXT : Input.AESCommShipBXPBase, IAESControlMessageBlockB
	{
		/// <summary>
		/// In the event no B record was submitted in the input batch, an AES generated B record will be returned.
		/// With the exception of the ‘Record Identifier’ and the ‘AES Generated Record Indicator’, all AES generated B record data elements will contain spaces.
		/// The ‘AES Generated Record Indicator’ will be 1
		/// </summary>
		[MessageBlockString(1, 80, "M")] // should be space when empty instead of '0'
		public ZString AESGeneratedRecordIndicator;

		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return AESApplicationIdentifierCodeList.CommodityShipmentResponse; }
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
	[OutputBlock("CL2")]
	public partial class AESCommShipCL2XP : Input.AESCommShipCL2XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("EV1")]
	public partial class AESCommShipEV1XP : Input.AESCommShipEV1XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("N01")]
	public partial class AESCommShipN01XP : Input.AESCommShipN01XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("N02")]
	public partial class AESCommShipN02XP : Input.AESCommShipN02XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("N03")]
	public partial class AESCommShipN03XP : Input.AESCommShipN03XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("ODT")]
	public partial class AESCommShipODTXP : Input.AESCommShipODTXPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("SC1")]
	public partial class AESCommShipSC1XP : Input.AESCommShipSC1XPBase
	{
		/// <summary>
		/// In the event no SC1 record was submitted in the input batch, an AES generated SC1 record will be returned.
		/// With the exception of the ‘Record Identifier’ and the ‘AES Generated Record Indicator’, all AES generated SC1 record data elements will contain spaces.
		/// The ‘AES Generated Record Indicator’ will be 1
		/// </summary>
		[MessageBlockString(1, 80, "M")] // should be space when empty instead of '0'
		public ZString AESGeneratedRecordIndicator;
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("SC2")]
	public partial class AESCommShipSC2XP : Input.AESCommShipSC2XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	[OutputBlock("SC3")]
	public partial class AESCommShipSC3XP : Input.AESCommShipSC3XPBase
	{
	}

	[ApplicationIdentifier(AESApplicationIdentifierCodeList.CommodityShipmentResponse, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipYXT : Input.AESCommShipYXPBase, IAESControlMessageBlockY
	{
	}

	partial class AESCommShipZXT : MessageBlock, IAESControlMessageBlockZ
	{
	}

	partial class AESCommShipES1XT : MessageBlock, IAESTIRES1MessageBlock
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
