namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class TariffUnit : Scraper
	{
		public TariffUnit(string url) : base(url)
		{
		}

		public override void Load() { }

		public override void Locate() => Node = Document.DocumentNode.SelectNodes("//table[@class='detail2']")[1];

		public override void Decode()
		{
			var rows = Node.SelectNodes("tr");
			I = Decode(rows[0].SelectNodes("td")[0].InnerText.Trim());
			II = Decode(rows[1].SelectNodes("td")[0].InnerText.Trim());
		}

		public override void Pack() => Record.AddProperties(I, II);

		string I { get; set; }

		string II { get; set; }

		public static DynamicCsvRecord Header { get => new DynamicCsvRecord(nameof(I), nameof(II)); }
	}
}
