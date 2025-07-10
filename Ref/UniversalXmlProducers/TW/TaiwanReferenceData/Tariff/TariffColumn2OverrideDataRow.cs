using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TariffColumn2OverrideDataRow : FlatFileDataRow
	{
		const int NumberOfFields = 7;

		public TariffColumn2OverrideDataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.TariffCode);
			SchemaList.Add(Schema.CountryCode);
			SchemaList.Add(Schema.ProvisionalEndDate);
			SchemaList.Add(Schema.ProvisionalStartDate);
			SchemaList.Add(Schema.ProvisionalSpecificRate);
			SchemaList.Add(Schema.ProvisionalAdValoremRate);
			SchemaList.Add(Schema.OrderNumber);
			SetFieldProperties(lineData);
		}

		#region Schema

		public class Schema
		{
			public static readonly FlatFileFieldProperty TariffCode = new FlatFileFieldProperty(0, 8);
			public static readonly FlatFileFieldProperty CountryCode = new FlatFileFieldProperty(1, 2);
			public static readonly FlatFileFieldProperty ProvisionalEndDate = new FlatFileFieldProperty(2, 8);
			public static readonly FlatFileFieldProperty ProvisionalStartDate = new FlatFileFieldProperty(3, 8);
			public static readonly FlatFileFieldProperty ProvisionalSpecificRate = new FlatFileFieldProperty(4, 10);
			public static readonly FlatFileFieldProperty ProvisionalAdValoremRate = new FlatFileFieldProperty(5, 10);
			public static readonly FlatFileFieldProperty OrderNumber = new FlatFileFieldProperty(6, 5);
		}

		#endregion

		protected override void SetFieldProperties(string lineData)
		{
			var dataAy = lineData.Split(',');
			if (dataAy.Length != DataRow.Length)
			{
				return;
			}

			for (var index = 0; index < DataRow.Length; index++)
			{
				DataRow[index] = dataAy[index].Trim();
			}
		}

		#region Properties

		public string TariffCode
		{
			get { return this[Schema.TariffCode.Name]; }
			set { SetField(Schema.TariffCode, value); }
		}

		public string CountryCode
		{
			get { return this[Schema.CountryCode.Name]; }
			set { SetField(Schema.CountryCode, value); }
		}

		public decimal ProvisionalSpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.ProvisionalSpecificRate.Name, 5); }
			set { SetFixedDecimalField(Schema.ProvisionalSpecificRate, value, 5); }
		}

		public decimal ProvisionalAdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.ProvisionalAdValoremRate.Name, 5); }
			set { SetFixedDecimalField(Schema.ProvisionalAdValoremRate, value, 5); }
		}

		public DateTime ProvisionalEndDate
		{
			get { return GetFieldAsDateTime(Schema.ProvisionalEndDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.ProvisionalEndDate, value, "yyyyMMdd"); }
		}

		public DateTime ProvisionalStartDate
		{
			get { return GetFieldAsDateTime(Schema.ProvisionalStartDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.ProvisionalStartDate, value, "yyyyMMdd"); }
		}

		public string OrderNumber
		{
			get { return this[Schema.OrderNumber.Name]; }
			set { SetField(Schema.OrderNumber, value); }
		}

		#endregion
	}
}
