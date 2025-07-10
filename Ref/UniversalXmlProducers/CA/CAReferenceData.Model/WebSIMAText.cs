using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class WebSIMAText
	{
		public string CBSAReferenceNumber { get; set; }
		public string Description { get; set; }
		public string DeterminationDate { get; set; }
		public string[] ClassificationNumbers { get; set; }
		public WebSIMADuty[] Duties { get; set; }
	}

	public class WebSIMADuty
	{
		public string DutyType { get; set; }
		public string DutyValue { get; set; }
		public string DutyCurrency { get; set; }
		public string[] CountryOfOriginOrExport { get; set; }
		public string EffectiveDate { get; set; }
	}

	public interface IWebSIMAParserV2
	{
		IEnumerable<WebSIMAText> GetWebSIMAText(string simaUrl, string simaBaseUrl);
		DateTime PublishedDate { get; set; }
	}
}
