using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IMeasure
	{
		string Sid { get; set; }                     //measure key
		string MeasureType { get; set; }
		string MeasureClass { get; set; }
		string TradeGroup { get; set; }
		List<string> ExcludedTradeGroups { get; set; }
		string ApplicationTerritory { get; set; }
		string TaxCode { get; set; }
		string TaxCodeDescription { get; set; }
		string Regulation { get; set; }
		DateTime StartDate { get; set; }
		DateTime EndDate { get; set; }
		string QuotaNumber { get; set; }
		string SupplementaryCode { get; set; }
		string Nomenclature { get; set; }
		string Direction { get; set; }
		List<string> Renvois { get; set; }
		List<string> Preferences { get; set; }
		List<Component> Components { get; set; }
		List<Condition> Conditions { get; set; }
	}
}
