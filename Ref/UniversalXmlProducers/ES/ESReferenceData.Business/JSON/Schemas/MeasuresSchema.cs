using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class MeasuresSchema : JsonlSchema
{
	public string goods_id { get; set; } = string.Empty;
	public string flow { get; set; } = string.Empty;
	public string geographical_area { get; set; } = string.Empty;
	public IEnumerable<string> excluded_geographical_area { get; set; }
	public string measure_type { get; set; } = string.Empty;
	public IEnumerable<string> footnotes { get; set; }
	public DateTime start_date { get; set; } = DateTime.MinValue;
	public DateTime? end_date { get; set; } = DateTime.MaxValue;
	public string legal_reference { get; set; } = string.Empty;
	public string duty { get; set; } = string.Empty;
	public string conditions { get; set; } = string.Empty;
	public string codadd { get; set; } = string.Empty;
	public string order_number_id { get; set; } = string.Empty;
}
