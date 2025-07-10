using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderLineDeliveryFlatFileDataRow : OrderFlatFileDataRow
	{
		public OrderLineDeliveryFlatFileDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		#region Schema

		public class OrderLineDeliverySchema : Schema
		{
			public const int __NotInUsed1 = 1;
			public const int __NotInUsed2 = 2;
			public const int DeliveryPort = 3;
			public const int DeliveryPointAddressShortCode = 4;
			public const int QtyDelivered = 5;
			public const int __NotInUsed3 = 6;
			public const int __NotInUsed4 = 7;

			public const int CustomAttribute1 = 8;
			public const int CustomAttribute2 = 9;
			public const int CustomAttribute3 = 10;
			public const int CustomAttribute4 = 11;
			public const int CustomAttribute5 = 12;

			public const int CustomDate1 = 13;
			public const int CustomDate2 = 14;
			public const int CustomDate3 = 15;
			public const int CustomDate4 = 16;
			public const int CustomDate5 = 17;

			public const int CustomFlag1 = 18;
			public const int CustomFlag2 = 19;
			public const int CustomFlag3 = 20;
			public const int CustomFlag4 = 21;
			public const int CustomFlag5 = 22;

			public const int CustomNumber1 = 23;
			public const int CustomNumber2 = 24;
			public const int CustomNumber3 = 25;
			public const int CustomNumber4 = 26;
			public const int CustomNumber5 = 27;
		}

		#endregion

		#region Fields

		public ZString DeliveryPort
		{
			get { return GetField(OrderLineDeliverySchema.DeliveryPort); }
		}

		public ZString DeliveryPointAddressShortCode
		{
			get { return GetField(OrderLineDeliverySchema.DeliveryPointAddressShortCode); }
		}

		public ZInt QtyDelivered
		{
			get { return GetFieldAsZInt(OrderLineDeliverySchema.QtyDelivered); }
		}

		public ZString CustomAttribute1
		{
			get { return GetField(OrderLineDeliverySchema.CustomAttribute1); }
		}

		public ZString CustomAttribute2
		{
			get { return GetField(OrderLineDeliverySchema.CustomAttribute2); }
		}

		public ZString CustomAttribute3
		{
			get { return GetField(OrderLineDeliverySchema.CustomAttribute3); }
		}

		public ZString CustomAttribute4
		{
			get { return GetField(OrderLineDeliverySchema.CustomAttribute4); }
		}

		public ZString CustomAttribute5
		{
			get { return GetField(OrderLineDeliverySchema.CustomAttribute5); }
		}

		public ZDateTime CustomDate1
		{
			get { return GetFieldAsZDateTime(OrderLineDeliverySchema.CustomDate1, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate2
		{
			get { return GetFieldAsZDateTime(OrderLineDeliverySchema.CustomDate2, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate3
		{
			get { return GetFieldAsZDateTime(OrderLineDeliverySchema.CustomDate3, "yyyyMMdd"); }
		}
		public ZDateTime CustomDate4
		{
			get { return GetFieldAsZDateTime(OrderLineDeliverySchema.CustomDate4, "yyyyMMdd"); }
		}

		public ZDateTime CustomDate5
		{
			get { return GetFieldAsZDateTime(OrderLineDeliverySchema.CustomDate5, "yyyyMMdd"); }
		}

		public bool CustomFlag1
		{
			get { return GetField(OrderLineDeliverySchema.CustomFlag1) == "Y"; }
		}

		public bool CustomFlag2
		{
			get { return GetField(OrderLineDeliverySchema.CustomFlag2) == "Y"; }
		}

		public bool CustomFlag3
		{
			get { return GetField(OrderLineDeliverySchema.CustomFlag3) == "Y"; }
		}

		public bool CustomFlag4
		{
			get { return GetField(OrderLineDeliverySchema.CustomFlag4) == "Y"; }
		}

		public bool CustomFlag5
		{
			get { return GetField(OrderLineDeliverySchema.CustomFlag5) == "Y"; }
		}

		public ZDecimal CustomNumber1
		{
			get { return GetFieldAsZDecimal(OrderLineDeliverySchema.CustomNumber1); }
		}

		public ZDecimal CustomNumber2
		{
			get { return GetFieldAsZDecimal(OrderLineDeliverySchema.CustomNumber2); }
		}

		public ZDecimal CustomNumber3
		{
			get { return GetFieldAsZDecimal(OrderLineDeliverySchema.CustomNumber3); }
		}

		public ZDecimal CustomNumber4
		{
			get { return GetFieldAsZDecimal(OrderLineDeliverySchema.CustomNumber4); }
		}

		public ZDecimal CustomNumber5
		{
			get { return GetFieldAsZDecimal(OrderLineDeliverySchema.CustomNumber5); }
		}

		#endregion
	}
}
