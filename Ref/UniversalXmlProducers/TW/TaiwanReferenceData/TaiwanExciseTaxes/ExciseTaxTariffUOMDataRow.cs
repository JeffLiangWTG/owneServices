namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExciseTaxTariffUOMDataRow
	{
		ExciseTaxTariffUOMDataRow(string type, string uom)
		{
			ZZ8_Type = type;
			ZZ8_UOM = uom;
		}

		public string ZZ8_Type { get; }

		public string ZZ8_UOM { get; }

		bool IsValid => !string.IsNullOrEmpty(ZZ8_Type);

		public static ExciseTaxTariffUOMDataRow New(string type, string uom)
		{
			var instance = new ExciseTaxTariffUOMDataRow(type, uom);
			return instance.IsValid ? instance : null;
		}
	}
}
