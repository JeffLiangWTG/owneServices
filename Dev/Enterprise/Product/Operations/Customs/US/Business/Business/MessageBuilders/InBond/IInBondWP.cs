using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Declaration
	/// </summary>
	public interface IMessageActionHeader
	{
		BusinessObjectFactory Factory { get; }
		IReadOnlyList<IMessageAttacheeInDeclaration> MessageAttachees { get; }
		void RecalculateValidationModesOnDeclaration();
		ZString InBondExportTransportMode { get; }
	}

	/// <summary>
	/// Common interface for Arrive/Export/TOL and BTA submission
	/// </summary>
	public interface IInBondWPHeader : IMessageAttacheeWithCBPSenderReference
	{
		ZString InBondNumber { get; }
		ZString MasterBillIssuerCode { get; }
		ZString MasterBillNumber { get; }
		ZString ContainerNumber { get; }
	}

	/// <summary>
	/// Header to report arrive, export or transfer liability
	/// CusEntryHeader, HouseBill or Container
	/// </summary>
	public interface IInBondArriveExportTOLHeader : IInBondWPHeader
	{
		ZDateTime ArrivalDateTime { get; }
		ZDateTime ExportDateTime { get; }

		ZString ScheduleDPortOfArrival { get; }
		ZString PortOfExport { get; }

		ZString InBondCarrierCode { get; }
		ZString BondedCarrierID { get; }

		ZDateTime TOLDateTime { get; }
		ZString CityName { get; }
		ZString StateCode { get; }
		ZString InBondExportTransportMode { get; }
		ZString InBondImportTransportMode { get; }
		ZString ExportConveyance { get; }

		ZString JobNumber { get; }
		ZString EntryNumber { get; }

		ZString ArrivalFirmsCode { get; }
		void UpdateLinkBusinessObject(MQEDIMessage message);
	}

	public interface IInbondMessageSendingData
	{
		ZString PortCode { get; }
		ZDateTime DiversionDateTime { get; }
		ZString InBondCarrierCode { get; }
		ZString BondedCarrierID { get; }
	}

	public interface IPriorNoticeProcessor : IMessageAttacheeInDeclaration
	{
		FDA GetOriginalFDALineToAddConfirmationNumber(ZInt fdaLineNumber, ZInt entryLineNumber);
	}

	public interface IFDACorrectionHeader : IPriorNoticeProcessor
	{
		IEnumerable<IFDAEntryLine> EntryLines { get; }
		new ZString ProcessingDistrictPort { get; }
	}

	public interface IFDAEntryLine
	{
		IEnumerable<IPriorNoticeLine> BTALines { get; }

		ZShort EntryLineNo { get; }
		ZString TariffNo { get; }
		ZString FDAIndicator { get; }
	}
}
