using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderHeaderFlatFileDataRow : OrderFlatFileDataRow
	{
		public OrderHeaderFlatFileDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		#region Schema

		public class OrderHeaderSchema : Schema
		{
			public const int RecordStructureAndVersion = 1;
			public const int SendingEmailURL = 2;
			public const int OrderNumber = 3;
			public const int SupplierInvoiceNumber = 4;
			public const int SupplierInvoiceDate = 5;
			public const int TransportMode = 6;
			public const int ContainerMode = 7;
			public const int OrderDate = 8;
			public const int ExWorksRequiredBy = 9;
			public const int DeliveryRequiredBy = 10;
			public const int OrderStatus = 11;
			public const int OrderCurrency = 12;
			public const int TotalOrderAmount = 13;
			public const int EstimatedExchangeRate = 14;

			public const int __NotInUsed1 = 15;

			public const int INCOTerm = 16;
			public const int AdditionalTerms = 17;
			public const int OrderGoodsDescription = 18;
			public const int TottalPacks = 19;
			public const int PackType = 20;

			public const int __NotInUsed3 = 21;

			public const int Volume = 22;
			public const int UnitOfVolum = 23;

			public const int __NotInUsed4 = 24;

			public const int Weight = 25;
			public const int UnitOfWeight = 26;
			public const int ConfirmationNumber = 27;
			public const int ConfirmationDate = 28;
			public const int CountryOfOrigin = 29;

			public const int __NotInUsed8 = 30;
			public const int __NotInUsed9 = 31;
			public const int __NotInUsed10 = 32;

			public const int SupplierCode = 33;
			public const int SupplierContact = 34;
			public const int SupplierCompanyName = 35;
			public const int SupplierAddress1 = 36;
			public const int SupplierAddress2 = 37;
			public const int SupplierCity = 38;
			public const int SupplierState = 39;
			public const int SupplierPostCode = 40;
			public const int SupplierISOCountryCode = 41;
			public const int SupplierCountryName = 42;
			public const int SupplierUNLOCO = 43;
			public const int SupplierPhone = 44;
			public const int SupplierEmailAddress = 45;

			public const int __NotInUsed11 = 46;
			public const int __NotInUsed12 = 47;

			public const int BuyerCode = 48;
			public const int BuyerContact = 49;
			public const int BuyerCompanyName = 50;
			public const int BuyerAddress1 = 51;
			public const int BuyerAddress2 = 52;
			public const int BuyerCity = 53;
			public const int BuyerState = 54;
			public const int BuyerPostCode = 55;
			public const int BuyerISOCountryCode = 56;
			public const int BuyerCountryName = 57;
			public const int BuyerUNLOCO = 58;
			public const int BuyerPhone = 59;
			public const int BuyerEmailAddress = 60;
			public const int HouseBill = 61;
			public const int DepartureVesselFlight = 62;
			public const int DepartureVoyageFlight = 63;
			public const int GoodsOrigin = 64;
			public const int GoodsDestination = 65;
			public const int GoodsAvaliableAt = 66;
			public const int GoodsDeliveredTo = 67;
			public const int LoadPort = 68;
			public const int DischargePort = 69;
			public const int SendingAgent = 70;
			public const int ReceivingAgent = 71;

			public const int __NotInUsed24 = 72;

			public const int CustomDate1 = 73;
			public const int CustomDate2 = 74;
			public const int CustomAttrib1 = 75;
			public const int CustomAttrib2 = 76;
			public const int CustomAttrib3 = 77;
			public const int CustomAttrib4 = 78;
			public const int CustomAttrib5 = 79;
			public const int CustomFlag1 = 80;
			public const int CustomFlag2 = 81;
			public const int CustomFlag3 = 82;
			public const int CustomFlag4 = 83;
			public const int CustomFlag5 = 84;
			public const int CustomDecimal1 = 85;
			public const int CustomDecimal2 = 86;
			public const int CustomDecimal3 = 87;
			public const int CustomDecimal4 = 88;
			public const int CustomDecimal5 = 89;
			public const int CustomContact1 = 90;
			public const int CustomContact2 = 91;
			public const int __NotInUsed44 = 92;

			public const int GoodsHandlingNotes = 93;
			public const int DGAdditionalHandlingNotes = 94;
			public const int SpecialInstructions = 95;
			public const int DeliveryInstructions = 96;
		}

		#endregion

		#region Fields

		public ZString RecordStructureAndVersion
		{
			get { return GetField(OrderHeaderSchema.RecordStructureAndVersion); }
		}

		public ZString SendingEmailURL
		{
			get { return GetField(OrderHeaderSchema.SendingEmailURL); }
		}

		public ZString OrderNumber
		{
			get { return GetField(OrderHeaderSchema.OrderNumber); }
		}

		public ZString SupplierInvoiceNumber
		{
			get { return GetField(OrderHeaderSchema.SupplierInvoiceNumber); }
		}

		public ZDateTime SupplierInvoiceDate
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.SupplierInvoiceDate, "yyyyMMdd"); }
		}

		public ZString TransportMode
		{
			get { return GetField(OrderHeaderSchema.TransportMode); }
		}

		public ZString ContainerMode
		{
			get { return GetField(OrderHeaderSchema.ContainerMode); }
		}

		public ZDateTime OrderDate
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.OrderDate, "yyyyMMdd"); }
		}

		public ZDateTime ExWorksRequiredBy
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.ExWorksRequiredBy, "yyyyMMdd"); }
		}

		public ZDateTime DeliveryRequiredBy
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.DeliveryRequiredBy, "yyyyMMdd"); }
		}

		public ZString OrderStatus
		{
			get { return GetField(OrderHeaderSchema.OrderStatus); }
		}

		public ZString OrderCurrency
		{
			get { return GetField(OrderHeaderSchema.OrderCurrency); }
		}

		public ZDecimal TotalOrderAmount
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.TotalOrderAmount); }
		}

		public ZDecimal EstimatedExchangeRate
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.EstimatedExchangeRate); }
		}

		public ZString INCOTerm
		{
			get { return GetField(OrderHeaderSchema.INCOTerm); }
		}

		public ZString AdditionalTerms
		{
			get { return GetField(OrderHeaderSchema.AdditionalTerms); }
		}

		public ZString OrderGoodsDescription
		{
			get { return GetField(OrderHeaderSchema.OrderGoodsDescription); }
		}

		public ZDecimal TottalPacks
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.TottalPacks); }
		}

		public ZString PackType
		{
			get { return GetField(OrderHeaderSchema.PackType); }
		}

		public ZDecimal Volume
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.Volume); }
		}

		public ZString UnitOfVolum
		{
			get { return GetField(OrderHeaderSchema.UnitOfVolum); }
		}

		public ZDecimal Weight
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.Weight); }
		}

		public ZString UnitOfWeight
		{
			get { return GetField(OrderHeaderSchema.UnitOfWeight); }
		}

		public ZString ConfirmationNumber
		{
			get { return GetField(OrderHeaderSchema.ConfirmationNumber); }
		}

		public ZDateTime ConfirmationDate
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.ConfirmationDate, "yyyyMMdd"); }
		}

		public ZString CountryOfOrigin
		{
			get { return GetField(OrderHeaderSchema.CountryOfOrigin); }
		}

		public ZString SupplierCode
		{
			get { return GetField(OrderHeaderSchema.SupplierCode); }
		}

		public ZString SupplierContact
		{
			get { return GetField(OrderHeaderSchema.SupplierContact); }
		}

		public ZString SupplierCompanyName
		{
			get { return GetField(OrderHeaderSchema.SupplierCompanyName); }
		}

		public ZString SupplierAddress1
		{
			get { return GetField(OrderHeaderSchema.SupplierAddress1); }
		}

		public ZString SupplierAddress2
		{
			get { return GetField(OrderHeaderSchema.SupplierAddress2); }
		}

		public ZString SupplierCity
		{
			get { return GetField(OrderHeaderSchema.SupplierCity); }
		}

		public ZString SupplierState
		{
			get { return GetField(OrderHeaderSchema.SupplierState); }
		}

		public ZString SupplierPostCode
		{
			get { return GetField(OrderHeaderSchema.SupplierPostCode); }
		}

		public ZString SupplierISOCountryCode
		{
			get { return GetField(OrderHeaderSchema.SupplierISOCountryCode); }
		}

		public ZString SupplierCountryName
		{
			get { return GetField(OrderHeaderSchema.SupplierCountryName); }
		}
		public ZString SupplierUNLOCO
		{
			get { return GetField(OrderHeaderSchema.SupplierUNLOCO); }
		}

		public ZString SupplierPhone
		{
			get { return GetField(OrderHeaderSchema.SupplierPhone); }
		}

		public ZString SupplierEmailAddress
		{
			get { return GetField(OrderHeaderSchema.SupplierEmailAddress); }
		}

		public ZString BuyerCode
		{
			get { return GetField(OrderHeaderSchema.BuyerCode); }
		}
		public ZString BuyerContact
		{
			get { return GetField(OrderHeaderSchema.BuyerContact); }
		}

		public ZString BuyerCompanyName
		{
			get { return GetField(OrderHeaderSchema.BuyerCompanyName); }
		}

		public ZString BuyerAddress1
		{
			get { return GetField(OrderHeaderSchema.BuyerAddress1); }
		}

		public ZString BuyerAddress2
		{
			get { return GetField(OrderHeaderSchema.BuyerAddress2); }
		}

		public ZString BuyerCity
		{
			get { return GetField(OrderHeaderSchema.BuyerCity); }
		}

		public ZString BuyerState
		{
			get { return GetField(OrderHeaderSchema.BuyerState); }
		}

		public ZString BuyerPostCode
		{
			get { return GetField(OrderHeaderSchema.BuyerPostCode); }
		}

		public ZString BuyerISOCountryCode
		{
			get { return GetField(OrderHeaderSchema.BuyerISOCountryCode); }
		}

		public ZString BuyerCountryName
		{
			get { return GetField(OrderHeaderSchema.BuyerCountryName); }
		}

		public ZString BuyerUNLOCO
		{
			get { return GetField(OrderHeaderSchema.BuyerUNLOCO); }
		}

		public ZString BuyerPhone
		{
			get { return GetField(OrderHeaderSchema.BuyerPhone); }
		}

		public ZString BuyerEmailAddress
		{
			get { return GetField(OrderHeaderSchema.BuyerEmailAddress); }
		}

		public ZString HouseBill
		{
			get { return GetField(OrderHeaderSchema.HouseBill); }
		}

		public ZString DepartureVesselFlight
		{
			get { return GetField(OrderHeaderSchema.DepartureVesselFlight); }
		}

		public ZString DepartureVoyageFlight
		{
			get { return GetField(OrderHeaderSchema.DepartureVoyageFlight); }
		}

		public ZString GoodsOrigin
		{
			get { return GetField(OrderHeaderSchema.GoodsOrigin); }
		}

		public ZString GoodsDestination
		{
			get { return GetField(OrderHeaderSchema.GoodsDestination); }
		}

		public ZString GoodsAvaliableAt
		{
			get { return GetField(OrderHeaderSchema.GoodsAvaliableAt); }
		}

		public ZString GoodsDeliveredTo
		{
			get { return GetField(OrderHeaderSchema.GoodsDeliveredTo); }
		}

		public ZString LoadPort
		{
			get { return GetField(OrderHeaderSchema.LoadPort); }
		}

		public ZString DischargePort
		{
			get { return GetField(OrderHeaderSchema.DischargePort); }
		}

		public ZString SendingAgent
		{
			get { return GetField(OrderHeaderSchema.SendingAgent); }
		}

		public ZString ReceivingAgent
		{
			get { return GetField(OrderHeaderSchema.ReceivingAgent); }
		}

		public ZDateTime CustomDate1
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.CustomDate1, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate2
		{
			get { return GetFieldAsZDateTime(OrderHeaderSchema.CustomDate2, "yyyyMMdd"); }
		}

		public ZString CustomAttrib1
		{
			get { return GetField(OrderHeaderSchema.CustomAttrib1); }
		}

		public ZString CustomAttrib2
		{
			get { return GetField(OrderHeaderSchema.CustomAttrib2); }
		}

		public ZString CustomAttrib3
		{
			get { return GetField(OrderHeaderSchema.CustomAttrib3); }
		}

		public ZString CustomAttrib4
		{
			get { return GetField(OrderHeaderSchema.CustomAttrib4); }
		}

		public ZString CustomAttrib5
		{
			get { return GetField(OrderHeaderSchema.CustomAttrib5); }
		}

		public bool CustomFlag1
		{
			get { return GetField(OrderHeaderSchema.CustomFlag1) == "Y"; }
		}

		public bool CustomFlag2
		{
			get { return GetField(OrderHeaderSchema.CustomFlag2) == "Y"; }
		}

		public bool CustomFlag3
		{
			get { return GetField(OrderHeaderSchema.CustomFlag3) == "Y"; }
		}

		public bool CustomFlag4
		{
			get { return GetField(OrderHeaderSchema.CustomFlag4) == "Y"; }
		}

		public bool CustomFlag5
		{
			get { return GetField(OrderHeaderSchema.CustomFlag5) == "Y"; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.CustomDecimal1); }
		}

		public ZDecimal CustomDecimal2
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.CustomDecimal2); }
		}
		public ZDecimal CustomDecimal3
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.CustomDecimal3); }
		}

		public ZDecimal CustomDecimal4
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.CustomDecimal4); }
		}

		public ZDecimal CustomDecimal5
		{
			get { return GetFieldAsZDecimal(OrderHeaderSchema.CustomDecimal5); }
		}

		public ZString CustomContact1
		{
			get { return GetField(OrderHeaderSchema.CustomContact1); }
		}

		public ZString CustomContact2
		{
			get { return GetField(OrderHeaderSchema.CustomContact2); }
		}

		public ZString GoodsHandlingNotes
		{
			get { return GetField(OrderHeaderSchema.GoodsHandlingNotes); }
		}

		public ZString DGAdditionalHandlingNotes
		{
			get { return GetField(OrderHeaderSchema.DGAdditionalHandlingNotes); }
		}

		public ZString SpecialInstructions
		{
			get { return GetField(OrderHeaderSchema.SpecialInstructions); }
		}

		public ZString DeliveryInstructions
		{
			get { return GetField(OrderHeaderSchema.DeliveryInstructions); }
		}

		#endregion
	}
}
