namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public static class Constants
	{
		public static class AddressType
		{
			public const string CBPBroker = "CBPBroker";
			public const string Exporter = "Exporter";
			public const string FDASubmitter = "FDASubmitter";
			public const string FDAShipper = "FDAShipper";
			public const string FDAEstablishmentIdentifier = "FDAEstablishmentIdentifier";
			public const string ForeignExporter = "ForeignExporter";
			public const string FSVPImporter = "FSVPImporter";
			public const string Invoicer = "Invoicer";
			public const string ImporterOfRecord = "ImporterOfRecord";
			public const string Seller = "Seller";
			public const string SoldToParty = Customs.DataTransfer.Universal.Constants.AddressTypes.SoldToParty;
			public const string Transferee = "Transferee";
			public const string Owner = "Owner";
			public const string Applicant = "Applicant";
			public const string AquacultureFacility = "AquacultureFacility";
			public const string Producer = "Producer";
			public const string Grower = "Grower";
			public const string Shipper = "Shipper";
			public const string Laboratory = "Laboratory";
			public const string DeliverToPartyAddress = "DeliverToPartyAddress";
			public const string FDAImporter = "FDAImporter";
			public const string OriginalVehicleManufacturerAddress = "OriginalVehicleManufacturerAddress";
			public const string ShipToParty = "ShipToParty";
			public const string ResponsibleGovernmentOfficial = "ResponsibleGovernmentOfficial";
			public const string Permitted = "Permitted";
			public const string ContactParty = "ContactParty";
			public const string BondContact = "BondContact";
			public const string ForeignPrincipalPartyInInterest = "ForeignPrincipalPartyInInterest";
		}

		public static class AddInfoKeys
		{
			public class AdditionalBill : Customs.DataTransfer.Universal.Constants.AddInfoKeys.AdditionalBill
			{
				public const string ParentMasterBillIssuerSCAC = "ParentMasterBillIssuerSCAC";
				public const string ParentBillIssuerSCAC = "ParentBillIssuerSCAC";
			}

			public class Declaration : Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration
			{
				public const string WayBillIssuerSCAC = "WayBillIssuerSCAC";
				public const string MasterWayBillIssuerSCAC = "MasterWayBillIssuerSCAC";
				public const string StatementNumber = "StatementNumber";
				public const string StatementStatus = "StatementStatus";
				public const string StatementPaidDate = "StatementPaidDate";
				public const string StatementPaymentStatus = "StatementPaymentStatus";
				public const string SchDEntry = "SchDEntry";
				public const string SchDArrival = "SchDArrival";
				public const string FIRM = "US_NKLocationOfGoods";
				public const string NKIssuerSCAC = "UI_NKBillIssuerSCAC";
				public const string NKCarrierSCAC = "UI_NKCarrierSCAC";
				public const string EntryDate = "EntryDate";
				public const string BondDesignationCode = "BondDesignationCode";
			}

			public static class InvoiceHeader
			{
				public const string ReleaseEntryNumber = "ReleaseEntryNumber";
			}

			public static class InvoiceLine
			{
				public const string DrawbackImportEntryNo = "ImportEntryNo";
				public const string DrawbackImportDeclarationLine = "DRWImportEntryLine";
				public const string ParentProductLineNo = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.ParentProductLineNo;
				public const string InvoiceQuantity = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.InvoiceQuantity;
				public const string InvoiceQuantityUnit = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.InvoiceQuantityUnit;
				public const string CustomsQuantity = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.CustomsQuantity;
				public const string CustomsQuantityUnit = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.CustomsQuantityUnit;
				public const string LinePrice = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.LinePrice;
				public const string LineNo = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.LineNo;
				public const string Tariff = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.Tariff;
				public const string CustomsSecondQuantity = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.SecondQty;
				public const string CustomsSecondQuantityUnit = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.SecondCustomsQuantityUnit;
				public const string CustomsThirdQuantity = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.ThirdQty;
				public const string CustomsThirdQuantityUnit = Enterprise.Customs.US.Business.WarehouseCustomsAddInfoWrapper.AddInfoKeys.ThirdCustomsQuantityUnit;
			}

			public static class CusEntryHeader
			{
				public const string LiquidationDate = "LiquidationDate";
				public const string LiquidationType = "LiquidationType";
			}
		}

		public static class CusInBond
		{
			public static class MoveDetail
			{
				public static class NumberTypes
				{
					public const string PreviousInBondNumber = "PIT";
					public const string PreviousInBondNumberDescription = "Previous In-Bond Transit Number";
				}
			}

			public static class MovementHeader
			{
				public static class NumberTypes
				{
					public const string InBondNumberDescription = "In-Bond Transit Number";
				}
			}

			public static class CountryDescrption
			{
				public const string UnitedStates = "United States";
			}

			public static class AddressTypes
			{
				public const string InBondCarrier = "InBondCarrier";
				public const string TransferOfLiabilityCarrier = "TransferOfLiabilityCarrier";
			}
		}

		public static class EBond
		{
			public static class ApplicationReferences
			{
				public const string Accepted = "ACCEPTED";
			}
		}
	}
}
