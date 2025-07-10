using CargoWise.Definitions.Customs;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class Constants
	{
		public static class DataContext
		{
			public const string InvoiceGroup = "InvGrp";
		}

		public const string EntryNumberPlaceHolder = "<PendingCustomsResponse>";
		public const string EntryNumberPlaceHolderType = "<#>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description string")]
		public const string WarehouseAllocationInfoDescription = "Allocation Info";

		public static class AddInfoKeys
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
			public class Declaration
			{
				public const string MasterWayBillNumber = "MasterWayBillNumber";
				public const string InlandModeOfTransport = "InlandModeOfTransport"; // This was converted from an AddInfo originated from EU
				public const string UseOwnerRefAsQuarantineRef = "UseOwnerRefAsQuarantineRef";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldNotHaveConstructors")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
			public class AdditionalBill
			{
				public const string ParentMasterBillNumber = "ParentMasterBillNumber";
			}

			public static class InvoiceLine
			{
				public const string NewOwnerProductCode = "NewOwnerProductCode";
				public const string NewOwnerPartAttribute1 = "NewOwnerPartAttribute1";
				public const string NewOwnerPartAttribute2 = "NewOwnerPartAttribute2";
				public const string NewOwnerPartAttribute3 = "NewOwnerPartAttribute3";
				public const string NewOwnerSerialNumber = "NewOwnerSerialNumber";
				public const string CountryOfDestination = "CountryOfDestination";
				public const string AllocationKey = "AllocationKey";
				public const string NetPrice = "NetPrice";
			}

			public static class Outturn
			{
				public const string IsDamage = "IsDamage";
				public const string IsPillage = "IsPillage";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Allocation strings")]
			public static class AllocationInfo
			{
				public const string AllocationKey = "AllocationKey";
				public const string Quantity = "Quantity";
			}
		}

		public static class Note
		{
			public static class Descriptions
			{
				public const string GoodsDescription = "GoodsDescription";
				public const string MarksAndNumbersDescription = "MarksAndNumbers";
			}
		}

		public static class ReferenceNumberTypes
		{
			public static class Codes
			{
				public const string LocalReferenceNumber = "LRN";
			}

			public static class Descriptions
			{
				public static MultilingualString LocalReferenceNumber => ResString.GetMultilingualString("1E5D2BB6-7F55-484A-B6B5-2465473C2BE7", "Local Reference Number");
			}
		}

		public static class AdditionalReference
		{
			public static class EntryType
			{
				public static class Codes
				{
					public const string ResponsiblePartyID = CustomsAdditionalReferenceTypes.EntryType.Codes.ResponsiblePartyID;
					public const string PrincipalID = CustomsAdditionalReferenceTypes.EntryType.Codes.PrincipalID;
					public const string ControlledPremiseID = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID;
				}

				public static class Descriptions
				{
					public static MultilingualString ResponsiblePartyID { get { return ResString.GetMultilingualString("AdditionalReference|EntryType|ResponsiblePartyID", CustomsAdditionalReferenceTypes.EntryType.Descriptions.ResponsiblePartyID); } }
					public static MultilingualString PrincipalID { get { return ResString.GetMultilingualString("AdditionalReference|EntryType|PrincipalID", CustomsAdditionalReferenceTypes.EntryType.Descriptions.PrincipalID); } }
					public static MultilingualString ControlledPremiseID { get { return ResString.GetMultilingualString("AdditionalReference|EntryType|ControlledPremiseID", CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID); } }
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Field")]
		public static class AddressTypes
		{
			public const string SoldToParty = "SoldToParty";
			public const string Seller = "Seller";
			public const string BuyingAgent = "BuyingAgent";
			public const string SellingAgent = "SellingAgent";
			public const string UltimateConsignee = "UltimateConsignee";
			public const string IntermediateConsignee = "IntermediateConsignee";
			public const string BuyerAddress = "BuyerAddress";
			public const string SupplierAddress = "SupplierAddress";
		}

		public static ZString GetWayBillType(ZString billType)
		{
			switch (billType)
			{
				case BillTypeList.Codes.MasterBill:
					return WayBillTypeList.Codes.Master;
				case BillTypeList.Codes.SubHouseBill:
					return WayBillTypeList.Codes.SubHouse;
				default:
					return WayBillTypeList.Codes.House;
			}
		}

		public static class CATAIRMessage
		{
			public const string ActionPurposeCode = "BS";
			public const string DataProvider = "USCATAIR";
		}

		public static class JobDeclarationUniversalMessaging
		{
			public const string JobDeclarationUniversalMessageNumberPlaceHolder = "__JobDeclarationUniversalMessageNumberPlaceHolder__";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Field")]
		public static class JobDeclarationUniversalDepartment
		{
			public const string Customs = "Customs";
		}

		public static class CustomAttributeKeys
		{
			public const string CustomAttribute1 = "CustomFirstAttribute";
			public const string CustomAttribute2 = "CustomSecondAttribute";
			public const string CustomAttribute3 = "CustomThirdAttribute";
			public const string CustomAttribute4 = "CustomFourthAttribute";
			public const string CustomAttribute5 = "CustomFifthAttribute";
			public const string CustomAttribute6 = "CustomSixthAttribute";
		}
	}
}
