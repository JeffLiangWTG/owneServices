using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.DataTransfer.ForwardAir
{
	internal class ForwardAirRatesFlatFileDataRow : FlatFileDataRow
	{
		public ForwardAirRatesFlatFileDataRow(string[] dataRow, BusinessObjectFactory factory)
			: base(dataRow)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region Properties

		public ZString Origin
		{
			get
			{
				RefUNLOCO origin = RefUNLOCO.LoadFromIATA(Factory, GetField(Schema.Origin));
				return origin != null ? origin.Code : ZString.Empty;
			}
		}

		public ZString Destination
		{
			get
			{
				RefUNLOCO destination = RefUNLOCO.LoadFromIATA(Factory, GetField(Schema.Destination));
				return destination != null ? destination.Code : ZString.Empty;
			}
		}

		public int Days
		{
			get { return GetFieldAsZInt(Schema.Days); }
		}

		public ZDecimal W100
		{
			get { return GetFieldAsZDecimal(Schema.W100, 2); }
		}

		public ZDecimal W500
		{
			get { return GetFieldAsZDecimal(Schema.W500, 2); }
		}

		public ZDecimal W1000
		{
			get { return GetFieldAsZDecimal(Schema.W1000, 2); }
		}

		public ZDecimal W3000
		{
			get { return GetFieldAsZDecimal(Schema.W3000, 2); }
		}

		public ZDecimal W5000
		{
			get { return GetFieldAsZDecimal(Schema.W5000, 2); }
		}

		public ZDecimal W7500
		{
			get { return GetFieldAsZDecimal(Schema.W7500, 2); }
		}

		public ZDecimal Minimum
		{
			get { return GetFieldAsZDecimal(Schema.Minimum, 2); }
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const int Origin = 1;
			public const int Destination = 2;
			public const int Days = 5;
			public const int W100 = 13;
			public const int W500 = 14;
			public const int W1000 = 15;
			public const int W3000 = 16;
			public const int W5000 = 17;
			public const int W7500 = 18;
			public const int Minimum = 19;
		}

		#endregion
	}
}
