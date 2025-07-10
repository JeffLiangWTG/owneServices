using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs.CA.DIF;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business.DIS
{
	public interface IDISHostProvider
	{
		IDISHost DISHost { get; }
	}

	public interface IDISHost
	{
		BusinessObjectFactory Factory { get; }
		ZGuid PK { get; }
		ZBool ShowDISFeatures { get; }
		event EventHandler DISFeatureVisibilityChanged;
		ZBool DISEditable { get; }

		ZGuid BranchPK { get; }
		ZGuid CompanyPK { get; }

		IEnumerable<string> ApplicationCodes { get; }
		IHaveRequiredDocuments RequiredDocumentsProvider { get; }
		IEnumerable<IeDoc> EDocs { get; }

		ZString JobNumber { get; }
		IControllerIDProvider ControllerIDProvider { get; }
		ZString ImporterName { get; }

		bool NeedToDoPreFormAction();
		bool DoPreFormAction();
		IEnumerable<ZString> ErrorMessages { get; }
		IDISReferenceNumberFountainStrategy DISReferenceNumberFountainStrategy { get; }
		ZString HumanReadable { get; }
	}

	public interface IUSDISHost : IDISHost
	{
		IUSDISDefaultValues ValueProvider { get; }
		ZString MessageSendingWarning { get; }
		ZString MessageSendingError { get; }
		IEnumerable<ZString> FormGroups { get; }
	}

	public interface IUSDISDocumentIDsProvider
	{
		ICodeDescriptionPairList GetDocumentIDList(IUSDISHost host);
		bool HasBeenAccepted(IUSDISHost host, ZString documentLabel);
	}

	public interface IDISHostWrapper
	{
		IBusinessObjectCollection DISDocuments { get; }
	}

	public enum TransactionCategory { None = 0, SingleTransaction, Continuous, Other }
	/// <summary>
	/// JobDeclaration or OrgSupplierPart or OrgHeader or other messaging top biz obj
	/// </summary>
	public interface IUSDISDefaultValues
	{
		ZString PreparerID { get; }
		ZString PreparerSiteCode { get; }

		ZDateTime ArrivalDate { get; }
		ZString PortOfUnlading { get; }
		ZString PortOfEntry { get; }
		bool IsExport { get; }
		TransactionCategory TransactionCategory { get; }

		IEnumerable<ICommercialInvoiceDefault> DefaultInvoiceData { get; }

		IEnumerable<IDISBondDataDefault> DefaultBondData { get; }

		ZString ImporterOfRecordID { get; }

		IEnumerable<IDISTradeTransaction> DefaultTradeTransactions { get; }

		IEnumerable<IDISCBPRequestDefault> DefaultCBPRequests { get; }
	}

	public interface IDISData
	{
	}

	public interface IDISCommodityLine : IDISData
	{
		ZInt EntryLineNumber { get; }
		ZString HTSNumber { get; }
		ZString CommodityDescription { get; }
		ZString CountryOfOrigin { get; }
		ZString PortOfLoading { get; }
		ZString ContainerNumber { get; }
		ZString SealNumber { get; }
		ZDateTime ArrivalDate { get; }
		ZString PortOfUnlading { get; }
		ZString PortOfEntry { get; }

		IEnumerable<IDISTradeParty> TradeParties { get; }
		IDISVehicleAndEngineData VehicleData { get; }
	}

	public enum DISTradePartyType { None = 0, Manufacturer, Exporter, Importer, Shippier, Carrier, Broker, Filer, Consignee, Agent, Buyer, Seller, Facilitator, Other, Unknown }

	public interface IDISTradeParty : IDISData
	{
		ZString ID { get; }
		DISTradePartyType Type { get; }
		ZString Name { get; }
		ZString Address { get; }
	}

	public interface IDISVehicleAndEngineData : IDISData
	{
		ZInt VNELineNumber { get; }
		ZString VIN { get; }
		ZString Manufacturer { get; }
		ZString Model { get; }
		ZString SerialNumber { get; }
		ZString ManufactureMonth { get; }
		ZString ManufactureYear { get; }
		ZString EngineManufacturer { get; }
		ZString EngineModel { get; }
		ZString EngineSerialNumber { get; }
		ZDateTime EngineManufactureDate { get; }
	}

	public enum DISInvoiceType { None = 0, CommercialInvoice, Other }

	public interface IDISInvoice : IDISData
	{
		ZString InvoiceNumber { get; }
		DISInvoiceType InvoiceType { get; }
		IEnumerable<IDISInvoiceLine> InvoiceLines { get; }
	}

	public interface IDISInvoiceLine : IDISData
	{
		ZInt InvoiceLineNumber { get; }
		IDISCommodityLine CommodityDetails { get; }
	}

	public interface ICommercialInvoiceDefault
	{
		ZString InvoiceNumber { get; }
		ZString Description { get; }
		IEnumerable<IDISInvoiceLineDefault> InvoiceLines { get; }
	}

	public interface IDISInvoiceLineDefault : IDISInvoiceLine, IDISCommodityLine
	{
	}

	public interface IDISBondData : IDISData
	{
		BondNameType BondName { get; }
		ZString BondNumber { get; }
		ZString BondType { get; }
		ZString SuretyCode { get; }
		ZString AgentIDNumber { get; }
		ZString Filer { get; }
		ZDecimal BondAmount { get; }
	}

	public interface IDISBondDataDefault : IDISBondData
	{
		ZString Code { get; }
		ZString Description { get; }
	}

	public static class BondDataDefaultCode
	{
		public const string Bond1 = "1";
		public const string Bond2 = "2";
	}

	public enum BondNameType { None = 0, Single, ISFBond, Other }

	public enum TradeTransactionType { None = 0, EntrySummary, Entry, Bill, ISFNumber, Export, FTZAdmission }

	public interface IDISTradeTransaction : IDISData
	{
		TradeTransactionType Type { get; }

		/// <summary>
		/// For Entry and Entry Summary: Entry Number
		/// For Bill: Bill Number
		/// For ISF: ISF Number
		/// For Export Declaration: ITN
		/// For FTZ Admission: Admission Number
		/// </summary>
		ZString Number { get; }

		/// <summary>
		/// For Entry, Entry Summary and ISF: Filer
		/// For Bill: SCAC
		/// </summary>
		ZString FilerOrSCAC { get; }

		/// <summary>
		/// For Entry and Entry Summary: Entry Line Numbers
		/// For Bill: one HouseBillNumber
		/// </summary>
		IEnumerable<ZString> AdditionalNumbers { get; }

		/// <summary>
		/// For Entry and Entry Summary: Reference Number
		/// For Bill: Reference Number
		/// </summary>
		ZString ReferenceNumber { get; }

		/// <summary>
		/// Only For Export Declaration
		/// </summary>
		ZString ShipmentNo { get; }
		/// <summary>
		/// Only For Export Declaration
		/// </summary>
		ZString XTN { get; }
	}

	public enum CBPRequestType { None = 0, ACEActionNumber, ATSDocRequest, OtherCBPRequest }

	public interface IDISCBPRequestDefault
	{
		ZString ID { get; }
		CBPRequestType Type { get; }
		ZDateTime RequestDate { get; }
		ZString Description { get; }
	}

	public interface IDISPreFormActionRegistrar
	{
		IDISPreFormActionRunner GetDISPreFormActionRunner(IDISHost host);
	}

	public interface IDISPreFormActionRunner
	{
		bool Execute();
	}

	public static class DISFormGroupCodes
	{
		public const string NoGroup = "NOGROUP";
	}

	#region CA Customs

	public interface ICADIFHost : IDISHost
	{
		ICADIFDefaultValues ValueProvider { get; }
		ZBool ShouldSendChangeMessageAsAmendment { get; }
		ZString ImporterBusinessNumber { get; }
		OrgHeader BusinessNumberHolder { get; }
		ZDateTime DateOfArrival { get; }
	}

	public interface ICADIFDefaultValues
	{
		ZString DocumentNumber { get; }
		ZDate EffectiveDate { get; }
		ZDate ExpiryDate { get; }
	}

	public interface ICADIFDocumentProvider
	{
		IEnumerable<IDIFDocument> GetDIFDocuments(ICADIFHost host);
		IDIFDocument GetDIFDocument(BusinessObjectFactory factory, ZString referenceNumber, ZString applicationCode, ZGuid companyPK);
	}
	#endregion
}
