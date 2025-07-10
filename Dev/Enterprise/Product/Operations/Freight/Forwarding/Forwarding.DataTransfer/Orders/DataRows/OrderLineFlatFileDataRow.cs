using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderLineFlatFileDataRow : OrderFlatFileDataRow
	{
		public OrderLineFlatFileDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		#region Schema

		public class OrderLineSchema : Schema
		{
			public const int ClientOrderLineNumber = 1;
			public const int ClientOrderSubLineNumber = 2;
			public const int SupplierProductCode = 3;
			public const int SupplierProductDescription = 4;
			public const int LocalProductCode = 5;
			public const int LocalProductDescription = 6;
			public const int LineStatus = 7;

			public const int __NotInUsed1 = 8;

			public const int LineDropDate = 9;

			public const int __NotInUsed2 = 10;

			public const int ProductQuantityOrdered = 11;
			public const int ProductUQ = 12;
			public const int InnerPacks = 13;
			public const int OuterPacks = 14;
			public const int ProductUnitPrice = 15;

			public const int __NotInUsed3 = 16;
			public const int __NotInUsed4 = 17;
			public const int __NotInUsed5 = 18;

			public const int ProductLinePrice = 19;

			public const int __NotInUsed6 = 20;
			public const int __NotInUsed7 = 21;

			public const int PartAttribute1 = 22;
			public const int PartAttribute2 = 23;
			public const int PartAttribute3 = 24;
			public const int CustomAttribute1 = 25;
			public const int CustomAttribute2 = 26;
			public const int CustomAttribute3 = 27;
			public const int CustomAttribute4 = 28;
			public const int CustomAttribute5 = 29;
			public const int CustomAttribute6 = 30;
			public const int CustomTextBlob = 31;
			public const int CustomFlag1 = 32;
			public const int CustomFlag2 = 33;
			public const int CustomFlag3 = 34;
			public const int CustomFlag4 = 35;
			public const int CustomFlag5 = 36;
			public const int CustomDate1 = 37;
			public const int CustomDate2 = 38;
			public const int CustomDate3 = 39;
			public const int CustomDate4 = 40;
			public const int CustomDate5 = 41;
			public const int CustomDecimal1 = 42;
			public const int CustomDecimal2 = 43;
			public const int CustomDecimal3 = 44;
			public const int CustomDecimal4 = 45;
			public const int CustomDecimal5 = 46;

			public const int __NotInUsed8 = 47;
			public const int __NotInUsed9 = 48;
			public const int __NotInUsed10 = 49;
			public const int __NotInUsed11 = 50;
			public const int __NotInUsed12 = 51;
			public const int __NotInUsed13 = 52;
			public const int __NotInUsed14 = 53;
			public const int __NotInUsed15 = 54;

			public const int ContainerNumber = 55;
			public const int ContainerPuckingOrder = 56;
			public const int UnitQtyInvoiced = 57;
			public const int UnitQuantityReceived = 58;
			public const int ActualVolume = 59;
			public const int VolumeUnit = 60;
			public const int ActualWeight = 61;
			public const int WeightUnit = 62;

			public const int __NotInUsed16 = 63;
			public const int __NotInUsed17 = 64;
			public const int __NotInUsed18 = 65;
			public const int __NotInUsed19 = 66;
			public const int __NotInUsed20 = 67;
			public const int __NotInUsed21 = 68;
			public const int __NotInUsed22 = 69;
			public const int __NotInUsed23 = 70;

			public const int SpecialInstractions = 71;
			public const int AdditionalInformation = 72;
		}

		#endregion

		#region Fields

		public ZShort ClientOrderLineNumber
		{
			get { return GetFieldAsZShort(OrderLineSchema.ClientOrderLineNumber); }
		}

		public ZShort ClientOrderSubLineNumber
		{
			get { return GetFieldAsZShort(OrderLineSchema.ClientOrderSubLineNumber); }
		}

		public ZString SupplierProductCode
		{
			get { return GetField(OrderLineSchema.SupplierProductCode); }
		}

		public ZString SupplierProductDescription
		{
			get { return GetField(OrderLineSchema.SupplierProductDescription); }
		}

		public ZString LocalProductCode
		{
			get { return GetField(OrderLineSchema.LocalProductCode); }
		}

		public ZString LocalProductDescription
		{
			get { return GetField(OrderLineSchema.LocalProductDescription); }
		}

		public ZString LineStatus
		{
			get { return GetField(OrderLineSchema.LineStatus); }
		}

		public ZDateTime LineDropDate
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.LineDropDate, "yyyyMMdd"); }
		}

		public ZDecimal ProductQuantityOrdered
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.ProductQuantityOrdered); }
		}

		public ZString ProductUQ
		{
			get { return GetField(OrderLineSchema.ProductUQ); }
		}

		public ZDecimal InnerPacks
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.InnerPacks); }
		}

		public ZDecimal OuterPacks
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.OuterPacks); }
		}

		public ZDecimal ProductUnitPrice
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.ProductUnitPrice); }
		}

		public ZDecimal ProductLinePrice
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.ProductLinePrice); }
		}

		public ZString PartAttribute1
		{
			get { return GetField(OrderLineSchema.PartAttribute1); }
		}

		public ZString PartAttribute2
		{
			get { return GetField(OrderLineSchema.PartAttribute2); }
		}

		public ZString PartAttribute3
		{
			get { return GetField(OrderLineSchema.PartAttribute3); }
		}

		public ZString CustomAttribute1
		{
			get { return GetField(OrderLineSchema.CustomAttribute1); }
		}

		public ZString CustomAttribute2
		{
			get { return GetField(OrderLineSchema.CustomAttribute2); }
		}

		public ZString CustomAttribute3
		{
			get { return GetField(OrderLineSchema.CustomAttribute3); }
		}

		public ZString CustomAttribute4
		{
			get { return GetField(OrderLineSchema.CustomAttribute4); }
		}

		public ZString CustomAttribute5
		{
			get { return GetField(OrderLineSchema.CustomAttribute5); }
		}

		public ZString CustomAttribute6
		{
			get { return GetField(OrderLineSchema.CustomAttribute6); }
		}

		public ZString CustomTextBlob
		{
			get { return GetField(OrderLineSchema.CustomTextBlob); }
		}

		public bool CustomFlag1
		{
			get { return GetField(OrderLineSchema.CustomFlag1) == "Y"; }
		}

		public bool CustomFlag2
		{
			get { return GetField(OrderLineSchema.CustomFlag2) == "Y"; }
		}

		public bool CustomFlag3
		{
			get { return GetField(OrderLineSchema.CustomFlag3) == "Y"; }
		}

		public bool CustomFlag4
		{
			get { return GetField(OrderLineSchema.CustomFlag4) == "Y"; }
		}

		public bool CustomFlag5
		{
			get { return GetField(OrderLineSchema.CustomFlag5) == "Y"; }
		}

		public ZDateTime CustomDate1
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.CustomDate1, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate2
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.CustomDate2, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate3
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.CustomDate3, "yyyyMMdd"); }
		}
		public ZDateTime CustomDate4
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.CustomDate4, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate5
		{
			get { return GetFieldAsZDateTime(OrderLineSchema.CustomDate5, "yyyyMMdd"); }
		}

		public ZDecimal CustomDecimal1
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.CustomDecimal1); }
		}

		public ZDecimal CustomDecimal2
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.CustomDecimal2); }
		}
		public ZDecimal CustomDecimal3
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.CustomDecimal3); }
		}

		public ZDecimal CustomDecimal4
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.CustomDecimal4); }
		}

		public ZDecimal CustomDecimal5
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.CustomDecimal5); }
		}

		public ZString ContainerNumber
		{
			get { return GetField(OrderLineSchema.ContainerNumber); }
		}

		public ZInt ContainerPuckingOrder
		{
			get { return GetFieldAsZInt(OrderLineSchema.ContainerPuckingOrder); }
		}

		public ZDecimal UnitQtyInvoiced
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.UnitQtyInvoiced); }
		}

		public ZDecimal UnitQuantityReceived
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.UnitQuantityReceived); }
		}

		public ZDecimal ActualVolume
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.ActualVolume); }
		}

		public ZString VolumeUnit
		{
			get { return GetField(OrderLineSchema.VolumeUnit); }
		}

		public ZDecimal ActualWeight
		{
			get { return GetFieldAsZDecimal(OrderLineSchema.ActualWeight); }
		}

		public ZString WeightUnit
		{
			get { return GetField(OrderLineSchema.WeightUnit); }
		}

		public ZString SpecialInstractions
		{
			get { return GetField(OrderLineSchema.SpecialInstractions); }
		}

		public ZString AdditionalInformation
		{
			get { return GetField(OrderLineSchema.AdditionalInformation); }
		}

		#endregion
	}
}
