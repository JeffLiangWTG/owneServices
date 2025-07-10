namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TariffAttributeDataRow : FlatFileDataRow
	{
		const int NumberOfFields = 3;

		public TariffAttributeDataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.TariffCode);
			SchemaList.Add(Schema.TariffAttributeName);
			SchemaList.Add(Schema.TariffAttributeValue);
			SetFieldProperties(lineData);
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty TariffCode = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty TariffAttributeName = new FlatFileFieldProperty(1, 16);
			public static readonly FlatFileFieldProperty TariffAttributeValue = new FlatFileFieldProperty(2, 1000);
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

		public string TariffAttributeName
		{
			get { return this[Schema.TariffAttributeName.Name]; }
			set { SetField(Schema.TariffAttributeName, value); }
		}

		public string TariffAttributeValue
		{
			get { return this[Schema.TariffAttributeValue.Name]; }
			set { SetField(Schema.TariffAttributeValue, value); }
		}

		#endregion
	}
}
