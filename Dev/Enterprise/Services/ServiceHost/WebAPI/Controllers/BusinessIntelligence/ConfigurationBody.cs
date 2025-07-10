namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public class ConfigurationBody
	{
		public string ReportConfigurationID { get; set; }
		public string ConfigurationName { get; set; }
		public string ColumnConfiguration { get; set; }
		public bool IsDefault { get; set; }
		public string ReportName { get; set; }
		public string VisualName { get; set; }
		public string LastModifiedDateUTC { get; set; }
		public string NewModifiedDateUTC { get; set; }
	}
}