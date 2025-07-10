using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class TariffDataItem
	{
		public TariffDataItem()
		{
		}

		public TariffDataItem(bool dummyData)
		{
			DummyData = dummyData;
			TariffItem = string.Empty;
			Hyphens = string.Empty;
			Description = string.Empty;
			Unit = string.Empty;
			StandardRate = string.Empty;
			PreferentialRate = string.Empty;
		}

		public string TariffItem { get; set; }
		public string Hyphens { get; set; }
		public string Description { get; set; }
		public string Unit { get; set; }
		public string StandardRate { get; set; }
		public string PreferentialRate { get; set; }

		[JsonIgnore]
		public float StartXPosition { get; set; }

		[JsonIgnore]
		public bool DummyData { get; }

		[JsonIgnore]
		public bool AllFieldsOtherThanDescriptionEmpty => string.IsNullOrEmpty(TariffItem) && string.IsNullOrEmpty(Hyphens) && string.IsNullOrEmpty(Unit) && string.IsNullOrEmpty(StandardRate) && string.IsNullOrEmpty(PreferentialRate);

		public override string ToString()
		{
			return $"TariffItem: {TariffItem}, Hyphens: {Hyphens}, Description: {Description}, Unit: {Unit}, StandardRate: {StandardRate}, PreferentialRate: {PreferentialRate}";
		}
	}
}
