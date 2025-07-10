namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TariffDescriptionDataRow : FlatFileDataRow
	{
		const int NumberOfFields = 2;


		public TariffDescriptionDataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.TariffCode);
			SchemaList.Add(Schema.TariffDescription);
			SetFieldProperties(lineData);
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty TariffCode = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty TariffDescription = new FlatFileFieldProperty(1, 1000);
		}

		#endregion

		#region Properties

		public string TariffCode
		{
			get { return this[Schema.TariffCode.Name]; }
		}

		public string TariffDescription
		{
			get { return this[Schema.TariffDescription.Name]; }
		}

		#endregion
	}
}
