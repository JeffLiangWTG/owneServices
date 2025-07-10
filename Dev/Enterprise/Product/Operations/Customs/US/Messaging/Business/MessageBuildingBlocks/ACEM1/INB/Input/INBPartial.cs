using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage, Constants.ACE)]
	[OutputBlock("FD01")]
	public partial class INBFD01BTAPriorNotice : MessageBlock, IINBFD01BTAPriorNotice
	{
		#region IINBFD01BTAPriorNotice members

		ZInt IINBFD01BTAPriorNotice.FDALineNumber { get { return FDALineNumber; } }

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage, Constants.ACE)]
	[OutputBlock("FD02")]
	public partial class INBFD02BTAPriorNotice : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage, Constants.ACE)]
	[OutputBlock("FD03")]
	public partial class INBFD03BTAPriorNotice : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage, Constants.ACE)]
	[OutputBlock("FD04")]
	public partial class INBFD04BTAPriorNotice : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.OtherGovernmentAgencyMessage, Constants.ACE)]
	[OutputBlock("FD05")]
	public partial class INBFD05BTAPriorNotice : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("10")]
	public partial class INBQP10 : MessageBlock, IINBQP10
	{
		ZString IINBQP10.USPortOfDestination
		{
			get { return USPortOfDestination; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("20")]
	public partial class INBQP20 : MessageBlock, IINBQP20 { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("30")]
	public partial class INBQP30 : MessageBlock, IINBQP30
	{
		#region IINBQP30 Members

		ZString IINBQP30.MasterBillNumber
		{
			get { return IssuerSequenceOfMasterBillOfLading; }
		}

		ZString IINBQP30.SequenceNumber
		{
			get { return SequenceNumber; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("33")]
	public partial class INBQP33 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("40")]
	public partial class INBQP40 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("65")]
	public partial class INBQP65 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("70")]
	public partial class INBQP70 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("32")]
	public partial class INBQP32 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("50")]
	public partial class INBQP50 : MessageBlock, IQPFirstAddressSegment
	{
		#region IQPFirstAddressSegment Members

		ZString IQPFirstAddressSegment.CompanyName
		{
			get { return ForeignShipperName; }
			set { ForeignShipperName = value; }
		}

		ZString IQPFirstAddressSegment.AddressLine1
		{
			get { return ForeignShipperAddressLine1; }
			set { ForeignShipperAddressLine1 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("51")]
	public partial class INBQP51 : MessageBlock, IQPSecondAddressSegment
	{
		#region IQPSecondAddressSegment Members

		ZString IQPSecondAddressSegment.AddressLine2
		{
			get { return ForeignShipperAddressLine2; }
			set { ForeignShipperAddressLine2 = value; }
		}

		ZString IQPSecondAddressSegment.AddressLine3
		{
			get { return ForeignShipperAddressLine3; }
			set { ForeignShipperAddressLine3 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("52")]
	public partial class INBQP52 : MessageBlock, IQPPhoneAddressSegment
	{
		#region IQPPhoneAddressSegment Members

		ZString IQPPhoneAddressSegment.PhoneNumber
		{
			get { return ForeignShipperTelephoneOrTelexNumber; }
			set { ForeignShipperTelephoneOrTelexNumber = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("55")]
	public partial class INBQP55 : MessageBlock, IQPFirstAddressSegment
	{
		#region IQPFirstAddressSegment Members

		ZString IQPFirstAddressSegment.CompanyName
		{
			get { return ConsigneeName; }
			set { ConsigneeName = value; }
		}

		ZString IQPFirstAddressSegment.AddressLine1
		{
			get { return ConsigneeAddressLine1; }
			set { ConsigneeAddressLine1 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("56")]
	public partial class INBQP56 : MessageBlock, IQPSecondAddressSegment
	{
		#region IQPSecondAddressSegment Members

		ZString IQPSecondAddressSegment.AddressLine2
		{
			get { return ConsigneeAddressLine2; }
			set { ConsigneeAddressLine2 = value; }
		}

		ZString IQPSecondAddressSegment.AddressLine3
		{
			get { return ConsigneeAddressLine3; }
			set { ConsigneeAddressLine3 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("57")]
	public partial class INBQP57 : MessageBlock, IQPPhoneAddressSegment
	{
		#region IQPPhoneAddressSegment Members

		ZString IQPPhoneAddressSegment.PhoneNumber
		{
			get { return ConsigneeTelephoneOrTelexNumber; }
			set { ConsigneeTelephoneOrTelexNumber = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("60")]
	public partial class INBQP60 : MessageBlock, IQPFirstAddressSegment
	{
		#region IQPFirstAddressSegment Members

		ZString IQPFirstAddressSegment.CompanyName
		{
			get { return NotifyPartyName; }
			set { NotifyPartyName = value; }
		}

		ZString IQPFirstAddressSegment.AddressLine1
		{
			get { return NotifyPartyAddressLine1; }
			set { NotifyPartyAddressLine1 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("61")]
	public partial class INBQP61 : MessageBlock, IQPSecondAddressSegment
	{
		#region IQPSecondAddressSegment Members

		ZString IQPSecondAddressSegment.AddressLine2
		{
			get { return NotifyPartyAddressLine2; }
			set { NotifyPartyAddressLine2 = value; }
		}

		ZString IQPSecondAddressSegment.AddressLine3
		{
			get { return NotifyPartyAddressLine3; }
			set { NotifyPartyAddressLine3 = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[OutputBlock("62")]
	public partial class INBQP62 : MessageBlock, IQPPhoneAddressSegment
	{
		#region IQPPhoneAddressSegment Members

		ZString IQPPhoneAddressSegment.PhoneNumber
		{
			get { return NotifyPartyTelephoneOrTelexNumber; }
			set { NotifyPartyTelephoneOrTelexNumber = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[OutputBlock("71")]
	public partial class INBQP71 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[OutputBlock("72")]
	public partial class INBQP72 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[OutputBlock("75")]
	public partial class INBQP75 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, Constants.ACE)]
	[OutputBlock("76")]
	public partial class INBQP76 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[OutputBlock("10")]
	public partial class INBWP10 : MessageBlock, IINBWP10
	{
		#region IINBWP10 members

		ZString IINBWP10.InbondNumber
		{
			get { return InbondNumber; }
		}

		ZString IINBWP10.ActionCode
		{
			get { return ActionCode; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, Constants.ACE)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Constants.ACE)]
	[OutputBlock("20")]
	public partial class INBWP20 : MessageBlock, IINBWP20
	{
		#region IINBWP20 members

		ZString IINBWP20.BondedCarrierID
		{
			get { return BondedCarrierID; }
		}

		ZString IINBWP20.InbondCarrierCode
		{
			get { return InbondCarrierCode; }
		}

		ZString IINBWP20.PortOfArrival
		{
			get { return PortOfArrival; }
		}

		#endregion
	}
}

