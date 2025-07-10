using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExchangeRateDataRow : FlatFileDataRow, IExchangeRateData
	{
		const int NumberOfFields = 6;

		public ExchangeRateDataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.Currency);
			SchemaList.Add(Schema.Year);
			SchemaList.Add(Schema.Month);
			SchemaList.Add(Schema.TenDay);
			SchemaList.Add(Schema.InRate);
			SchemaList.Add(Schema.ExRate);
			SetFieldProperties(lineData);
			ProcessDate();
		}

		void ProcessDate()
		{
			switch (TenDay)
			{
				case 1:
					StartDate = new DateTime(Year, Month, 1);
					EndDate = new DateTime(Year, Month, 10, 23, 59, 00);
					break;
				case 2:
					StartDate = new DateTime(Year, Month, 11);
					EndDate = new DateTime(Year, Month, 20, 23, 59, 00);
					break;
				case 3:
					StartDate = new DateTime(Year, Month, 21);
					EndDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 00);
					break;
				default:
					StartDate = DateTime.MinValue;
					EndDate = DateTime.MaxValue;
					break;
			}
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty Currency = new FlatFileFieldProperty(0, 3);
			public static readonly FlatFileFieldProperty Year = new FlatFileFieldProperty(1, 4);
			public static readonly FlatFileFieldProperty Month = new FlatFileFieldProperty(2, 2);
			public static readonly FlatFileFieldProperty TenDay = new FlatFileFieldProperty(3, 1);
			public static readonly FlatFileFieldProperty InRate = new FlatFileFieldProperty(4, 10);
			public static readonly FlatFileFieldProperty ExRate = new FlatFileFieldProperty(5, 10);
		}

		#endregion

		#region Properties

		public string Currency
		{
			get { return this[Schema.Currency.Name]; }
		}

		public int Year
		{
			get { return GetFieldAsInt(Schema.Year.Name); }
		}

		public int Month
		{
			get { return GetFieldAsInt(Schema.Month.Name); }
		}

		public int TenDay
		{
			get { return GetFieldAsInt(Schema.TenDay.Name); }
		}

		public DateTime StartDate { get; private set; }

		public DateTime EndDate { get; private set; }

		public decimal InRate
		{
			get { return GetFieldAsDecimal(Schema.InRate.Name); }
		}

		public decimal ExRate
		{
			get { return GetFieldAsDecimal(Schema.ExRate.Name); }
		}

		#endregion
	}
}
