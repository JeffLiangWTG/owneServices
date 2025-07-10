using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	partial class FTZFT10 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	partial class FTZFT10_01 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	partial class FTZFT11 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	partial class FTZFT12 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	partial class FTZFT20 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("40")]
	public partial class FTZFT40 : MessageBlock, IFTBillOfLading, IFTZNF40
	{
		#region IFTBillOfLading Members

		ZString IFTBillOfLading.BillOfLadingOrAirWaybill
		{
			get => BillOfLadingOrAirWaybill;
			set => BillOfLadingOrAirWaybill = value;
		}

		ZString IFTBillOfLading.HouseBill
		{
			get => HouseBill;
			set => HouseBill = value;
		}

		ZDecimal IFTBillOfLading.Quantity
		{
			get => Quantity;
			set => Quantity = value;
		}

		ZString IFTBillOfLading.CountryOfExport
		{
			get => CountryOfExport;
			set => CountryOfExport = value;
		}

		ZString IFTBillOfLading.ForeignLoadPort
		{
			get => ForeignLoadPort;
			set => ForeignLoadPort = value;
		}

		ZString IFTBillOfLading.FIRMSIdentifier
		{
			get => FIRMSIdentifier;
			set => FIRMSIdentifier = value;
		}

		#endregion

		#region IFTZNF40 Members

		ZString IFTZNF40.BillOfLadingOrAirWaybill => BillOfLadingOrAirWaybill;

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("40", "01")]
	public partial class FTZFT40_01 : MessageBlock, IFTBillOfLading, IFTZNF40
	{
		#region IFTBillOfLading Members

		ZString IFTBillOfLading.BillOfLadingOrAirWaybill
		{
			get => BillOfLadingOrAirWaybill;
			set => BillOfLadingOrAirWaybill = value;
		}

		ZString IFTBillOfLading.HouseBill
		{
			get => HouseBill;
			set => HouseBill = value;
		}

		ZDecimal IFTBillOfLading.Quantity
		{
			get => Quantity;
			set => Quantity = value;
		}

		ZString IFTBillOfLading.CountryOfExport
		{
			get => CountryOfExport;
			set => CountryOfExport = value;
		}

		ZString IFTBillOfLading.ForeignLoadPort
		{
			get => ForeignLoadPort;
			set => ForeignLoadPort = value;
		}

		ZString IFTBillOfLading.FIRMSIdentifier
		{
			get => ZString.Empty;
			set { }
		}

		#endregion

		#region IFTZNF40 Members

		ZString IFTZNF40.BillOfLadingOrAirWaybill => BillOfLadingOrAirWaybill;

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("41")]
	public partial class FTZFT41 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("42")]
	public partial class FTZFT42 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("43")]
	public partial class FTZFT43 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("50")]
	public partial class FTZFT50 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("51")]
	public partial class FTZFT51 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("60")]
	public partial class FTZFT60 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("61")]
	public partial class FTZFT61 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	public partial class FTZFZ10 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	public partial class FTZFZ11 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	public partial class FTZFZ12 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("13")]
	public partial class FTZFZ13 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	[OutputBlock("14")]
	public partial class FTZFZ14 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)]
	public partial class FTZFZ20 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF10 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF20 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF30 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF31 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF35 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)]
	public partial class FTZSF36 : MessageBlock { }
}
