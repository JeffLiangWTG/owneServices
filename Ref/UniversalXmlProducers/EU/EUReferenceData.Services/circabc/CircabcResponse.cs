namespace CargoWise.RefDbRepo.EUReferenceData.Services.Circabc
{
	public class CircabcResponse
	{
		public Datum[] data { get; set; }
		public int total { get; set; }
	}

	public class Datum
	{
		public string id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string service { get; set; }
		public string parentId { get; set; }
		public Properties properties { get; set; }
		public Title title { get; set; }
		public Description description { get; set; }
		public Permissions permissions { get; set; }
		public string notifications { get; set; }
		public bool favourite { get; set; }
		public bool hasSubFolders { get; set; }
		public bool hasGuestAccess { get; set; }
	}

	public class Properties
	{
		public string owner { get; set; }
		public string creator { get; set; }
		public string nodeuuid { get; set; }
		public string created { get; set; }
		public string modifier { get; set; }
		public string isVersion { get; set; }
		public string locale { get; set; }
		public string storeprotocol { get; set; }
		public string originalContainerId { get; set; }
		public string name { get; set; }
		public string modified { get; set; }
		public string storeidentifier { get; set; }
		public string nodedbid { get; set; }
		public string title { get; set; }
		public string icon { get; set; }
		public string description { get; set; }
	}

	public class Title
	{
		public string de { get; set; }
		public string fi { get; set; }
		public string pt { get; set; }
		public string bg { get; set; }
		public string lt { get; set; }
		public string lv { get; set; }
		public string hr { get; set; }
		public string fr { get; set; }
		public string hu { get; set; }
		public string sk { get; set; }
		public string sl { get; set; }
		public string ga { get; set; }
		public string sv { get; set; }
		public string mt { get; set; }
		public string el { get; set; }
		public string en { get; set; }
		public string it { get; set; }
		public string es { get; set; }
		public string et { get; set; }
		public string cs { get; set; }
		public string pl { get; set; }
		public string da { get; set; }
		public string ro { get; set; }
		public string nl { get; set; }
	}

	public class Description
	{
		public string en { get; set; }
	}

	public class Permissions
	{
		public string LibAccess { get; set; }
	}
}
