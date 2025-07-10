using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.US.Business
{
	sealed class ATF6ADocDataObject : DocDataObject
	{
		public Address ImporterOfRecord { get; set; }
		public ZString OwnerRef { get; set; }
		public ZString TotalNoOfPacks { get; set; }
		public ZString TotalNoOfPacksPackageTypeDescription { get; set; }
		public ZString EntryNumber { get; set; }
		public ZString DeclarationNumber { get; set; }
		public ZString CountryManufactured { get; set; }
		public ZString EntryValue { get; set; }
		public ZString PortOfEntryAndDesc { get; set; }
		public ZBool IsWarehouse { get; set; }
		public ZBool IsConsumption { get; set; }
		public ZBool IsInformal { get; set; }
		public ZString EntryReleaseDate { get; set; }
		public IReadOnlyCollection<ATFGroupDocDataObject> ATFGroups { get; set; }
	}

	sealed class ATFGroupDocDataObject : DocDataObject
	{
		public ZString PermitNumber { get; set; }
		public ZString SellerName { get; set; }
		public ZString SellerDetails { get; set; }
		public ZString ForeignExporterName { get; set; }
		public ZString ForeignExporterDetails { get; set; }
		public ZString FFLNoAndAECANo { get; set; }
		public ZString FFLAndAECAExpirationDates { get; set; }
		public ZBool IsFirearms { get; set; }
		public ZBool IsImplementsOfWar { get; set; }
		public ZBool IsAmmunition { get; set; }
		public ZBool IsCargoAsDescribed { get; set; }
		public ZBool IsContainedOthers { get; set; }
		public ZString Discrepancies { get; set; }
		public IReadOnlyCollection<ATFDetailDocDataObject> ATFDetails { get; set; }
	}

	sealed class ATFDetailDocDataObject : DocDataObject
	{
		public ZString InvoiceNumber { get; set; }
		public ZInt LineNumber { get; set; }
		public ZString ManufacturerName { get; set; }
		public ZString ProductCode { get; set; }
		public ZString CategoryCode { get; set; }
		public ZString CaliberGaugeSize { get; set; }
		public ZString Quantity { get; set; }
		public ZString BarrelLength { get; set; }
		public ZString OverallLength { get; set; }
		public ZString MunitionsListCategory { get; set; }
		public ZString Model { get; set; }
		public ZString SerialNumber { get; set; }
	}
}
