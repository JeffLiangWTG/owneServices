using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public interface ITradeSingleWindowComponents
	{
	}

	/// <summary>
	/// Additional Information
	///		free text:									Additional document reference number				an..512
	///		additional statement code:					coded												an..17
	///		additional statement text:					reason for override or manual processing			an..512
	///		additional statement type:					code value											an..3
	/// </summary>
	public interface IAdditionalInformation
	{
		ZString FreeText { get; }
		ZString ManualOverrideText { get; }
		ZString AdditionalStatementText { get; }
		IEnumerable<ITSWAttachment> SupportingDocuments { get; }
		ZBool QueueForManifesting { get; }
		ZBool SendManifest { get; }
	}

	public interface ITSWAttachment
	{
		ZGuid UniqueIdentifier { get; }
		ZString FileName { get; }
		ZString DocType { get; }
		ZBool FileTooBig { get; }
	}

	public interface IOrganisationSimple
	{
		ZString Name { get; }
		ZString CustomsClientCode { get; }
		ZString CustomsSupplierCode { get; }
		IEnumerable<IContact> Contacts { get; }
	}

	public interface IOrganisation : IOrganisationSimple
	{
		ZString City { get; }
		ZString CountryCode { get; }
		ZString CountryRegion { get; }
		ZString Address { get; }
		ZString PostCode { get; }
		ZString ContactPerson { get; }
		IEnumerable<ICommunication> Communications { get; }
	}

	public interface IContact
	{
		ZString ContactName { get; }
		IEnumerable<ICommunication> Communications { get; }
	}

	public interface ICommunication
	{
		ZString ContactDetail { get; }
		ZString ContactType { get; }
	}

	public interface ITransportEquipment
	{
		ZString ContainerNumber { get; }
		ZString ContainerMode { get; }
		ZString Status { get; }
		ZString Size { get; }
		IEnumerable<ZString> SealNumbers { get; }
		ZString SealingParty { get; }
		ZString AttachedEquipmentCode { get; }
		ZString StowPosition { get; }
		ZBool IsPallet { get; }
		ZInt MessageSequence { get; set; }
		IEnumerable<ZGuid> RelatedPackages { get; }
		ZGuid StuffingLocation { get; }
		ZGuid PK { get; }
	}

	public interface IAssociatedTransportDocument
	{
		ZString BillNumber { get; }
		ZString BillType { get; }
		IEnumerable<ZGuid> RelatedEquipment { get; }
		IEnumerable<ZGuid> RelatedPackages { get; }
		ZInt MessageSequence { get; set; }
		ZGuid PK { get; }
	}

	public interface IMasterBillTransportDocument : IAssociatedTransportDocument
	{
		IEnumerable<ZGuid> ChildBills { get; }
	}

	public interface ICurrency
	{
		ZString CurrencyCode { get; }
		ZDecimal ExchangeRate { get; }
		ZString ExchangeRateIndicator { get; }
	}

	public interface IDeclarant
	{
		ZString DeclarantID { get; }
		IEnumerable<ICommunication> Communications { get; }
		ZString DeclarantPinEncrypted { get; }
	}

	public interface IDutyTaxFee
	{
		ZDecimal Amount { get; }
		ZString DutyTaxFeeType { get; }
		ZString CurrencyCode { get; }
	}

	public interface IPackaging
	{
		ZString ShippingMarks { get; }
		ZInt NumberOfPackages { get; }
		ZString PackageType { get; }
		ZString PackingMaterialDesc { get; }
		ZDecimal PackageVolumeInMTQ { get; }
		ZInt MessageSequence { get; set; }
		ZString RelatedHB { get; }
		ZString RelatedContainer { get; }
		ZGuid PK { get; }
	}

	public interface ICommodity
	{
		ZString CommodityNumber { get; }
		ZString CommodityType { get; }
	}
}
