using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IFTZConcurrence
	{
		ZDecimal FTZConcurrenceQty { get; set; }
		BusinessObjectFactory Factory { get; }
	}

	public interface IFTZCommonHeader : IMessageAttacheeWithCBPSenderReference, IFTZConcurrence
	{
		void AddMessage(MQEDIMessage message);
		void SetMessageStatus(ZString subType, ZString status);
		bool HasBeenLodgedAtCustoms { get; }
		bool DirectDeliveryIndicator { get; }
		bool IncludePTTInAdmission { get; }

		//FT10, FZ11
		ZString ZoneID { get; }
		ZInt CalendarYear { get; }

		//Control Number	8AN	13-20	M	
		//Open format, controlled by Applicant.  
		//The zone ID + calendar year + control number = the standardized admission number.
		ZString ControlNumber { get; }

		//FT40, 41, 42, 43 for FTZ
		// and for PTT
		ZString FTZAdmissionNumber { get; }
		IEnumerable<IFTZBillCommon> Bills { get; }

		IEnumerable<IFTZBillCommon> LowestBills { get; }
	}

	public interface IFTZBillCommon : IFTZConcurrence
	{
		ZDecimal Quantity { get; }
		ZString FIRMSCode { get; }

		//FT41 or FZ10 for Action Code "I"(goods arrival)
		IEnumerable<IITNumber> ITNumbers { get; }

		//FT42 and FZ10 PTT Carrier
		ZString IRSIdentifier { get; }

		//FT43 or FZ11(PTT)
		IEnumerable<IContainer> Containers { get; }

		ZDecimal CU_NoOfPacks { get; }
		ZString CU_BillNum { get; }

		ZString CU_PackType { get; }

		IEnumerable<IFTZITAndSplitDetail> ITAndSplitDetails { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IFTZITAndSplitDetail : IFTZConcurrence
	{
	}

	public interface IFTZHeader : IFTZCommonHeader, IFTZConcurrence
	{
		bool IsWaitingForResponse { get; }
		ZString PortCode { get; }
		ZString ABIRoutingCode { get; }
		ZString IRSIdentifier { get; }
		ZString AdmissionType { get; }

		//FT10
		ZString FirmsIdentifier { get; }
		ZString ImporterOfRecordID { get; }

		//FT11
		ZString ContactName { get; }
		ZString ContactPhone { get; }
		IEnumerable<ZString> ReasonCodes { get; }

		//FT12
		ZString Remarks { get; }

		//FT20
		IEnumerable<IFTZConveyance> Conveyances { get; }
	}

	//FT20
	public interface IFTZConveyance
	{
		ZString TransportMode { get; }

		ZString CarrierSCAC { get; }
		ZString ConveyanceName { get; }
		ZString VoyageNumber { get; }
		ZDate ExportDate { get; }
		ZDate ImportDate { get; }
		ZString PortOfUnlading { get; }
		ZDate EstimatedDateOfArrival { get; }
		IEnumerable<IFTZBill> Bills { get; }
	}

	public interface IFTZBill : IFTZBillCommon
	{
		ZString BillOfLading { get; }
		ZString HouseBill { get; }
		ZString CountryOfExport { get; }
		ZString ForeignLoadPort { get; }

		//FT50, 51, 60
		IEnumerable<IFTZLine> Lines { get; }
	}

	public interface IFTZLine
	{
		//FT50
		ZString Tariff { get; }
		ZString SpecialProgramsIndicatorPrimary { get; }
		ZString LongSPICode { get; }
		ZString SpecialProgramsIndicatorCountry { get; }
		ZString SecondarySPI { get; }
		ZString CountryOfOrigin { get; }
		ZDecimal Quantity1 { get; }
		ZString UQ1 { get; }
		ZDecimal Quantity2 { get; }
		ZString UQ2 { get; }
		ZString QuotaCategory { get; }
		ZString PNDisclaimer { get; }

		//FT51
		ZDecimal Weight { get; }
		ZDecimal Value { get; }
		ZDecimal Charges { get; }
		ZString ZoneStatus { get; }
		ZDecimal HMF { get; }

		//should be clarified
		//FT60
		ZString ImporterReferenceID { get; }
		ZString ManufacturerReferenceID { get; }
		ZString MiscPermitQualifer { get; }
		ZString MiscPermitNumber { get; }

		//FT61
		ZString Remarks { get; }

		ZInt LineNumber { get; }
	}

	public interface IITNumber
	{
		ZString ITNumber { get; }
		ZInt PackageQuantity { get; }
	}

	public interface IFZEventHeader : IFTZCommonHeader
	{
		ZString AirportCode { get; }
		ZString DeliveryCode { get; }
		ZDecimal Quantity { get; }
		ZBool IsAir { get; }
	}

	public interface IFZEventBill : IFTZBillCommon
	{
		//FZ10
		ZString BillOfLading { get; }
		ZString Remarks { get; }
		ZString ConcurRemarks { get; }

		//FZ11
		IEnumerable<IConveyanceOrSplitDetails> AirShipmentDetails { get; }

		//FZ12
		ZString PIDUniqueIdentifier { get; }
	}
}
