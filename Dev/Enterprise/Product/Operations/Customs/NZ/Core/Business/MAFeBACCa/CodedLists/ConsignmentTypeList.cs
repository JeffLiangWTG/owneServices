using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class ConsignmentTypeList : CodeDescriptionEnumList<BACCApplicationTypeDetailsConsignmentType>
	{
		public static class Codes
		{
			public const string AOPackage = "AOP";
			public const string CommercialCargo = "CCG";
			public const string CustomsInvoicedParcel = "CIP";
			public const string EmptyContainers = "EMC";
			public const string ExpressParcel = "EPP";
			public const string Magazine = "MGZ";
			public const string Newspaper = "NWS";
			public const string PrivateCargo = "PRC";
			public const string RegisteredParcel = "RGP";
			public const string TradeSample = "TRS";
		}

		public static class Descriptions
		{
			public const string AOPackage = "AO Package";
			public const string CommercialCargo = "Commercial Cargo";
			public const string CustomsInvoicedParcel = "Customs Invoiced Parcel";
			public const string EmptyContainers = "Empty Containers";
			public const string ExpressParcel = "Express Parcel";
			public const string Magazine = "Magazine";
			public const string Newspaper = "Newspaper";
			public const string PrivateCargo = "Private Cargo";
			public const string RegisteredParcel = "Registered Parcel";
			public const string TradeSample = "Trade Sample";
		}

		public ConsignmentTypeList()
		{
			AddPair(Codes.AOPackage, Descriptions.AOPackage, BACCApplicationTypeDetailsConsignmentType.AOPackage);
			AddPair(Codes.CommercialCargo, Descriptions.CommercialCargo, BACCApplicationTypeDetailsConsignmentType.CommercialCargo);
			AddPair(Codes.CustomsInvoicedParcel, Descriptions.CustomsInvoicedParcel, BACCApplicationTypeDetailsConsignmentType.CustomsInvoicedParcel);
			AddPair(Codes.EmptyContainers, Descriptions.EmptyContainers, BACCApplicationTypeDetailsConsignmentType.EmptyContainers);
			AddPair(Codes.ExpressParcel, Descriptions.ExpressParcel, BACCApplicationTypeDetailsConsignmentType.ExpressParcel);
			AddPair(Codes.Magazine, Descriptions.Magazine, BACCApplicationTypeDetailsConsignmentType.Magazine);
			AddPair(Codes.Newspaper, Descriptions.Newspaper, BACCApplicationTypeDetailsConsignmentType.Newspaper);
			AddPair(Codes.PrivateCargo, Descriptions.PrivateCargo, BACCApplicationTypeDetailsConsignmentType.PrivateCargo);
			AddPair(Codes.RegisteredParcel, Descriptions.RegisteredParcel, BACCApplicationTypeDetailsConsignmentType.RegisteredParcel);
			AddPair(Codes.TradeSample, Descriptions.TradeSample, BACCApplicationTypeDetailsConsignmentType.TradeSample);
		}
	}
}
