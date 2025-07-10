using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business
{
	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceAlertDetailsModel
	{
		[Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Name { get; set; }

		[Newtonsoft.Json.JsonProperty("code", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Code { get; set; }

		[Newtonsoft.Json.JsonProperty("description", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Description { get; set; }

		[Newtonsoft.Json.JsonProperty("nomenclatureWide", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public bool NomenclatureWide { get; set; }

		[Newtonsoft.Json.JsonProperty("commoditySpecific", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public bool CommoditySpecific { get; set; }

		[Newtonsoft.Json.JsonProperty("isActive", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public bool IsActive { get; set; }

		[Newtonsoft.Json.JsonProperty("contentUrl", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string ContentUrl { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCountryAlertDetailsResponseModel
	{
		[Newtonsoft.Json.JsonProperty("countryCode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string CountryCode { get; set; }

		[JsonProperty("exportAlerts", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<ComplianceAlertDetailsModel> ExportAlerts { get; set; }

		[JsonProperty("importAlerts", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<ComplianceAlertDetailsModel> ImportAlerts { get; set; }
	}
}
