using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefStlFieldMapping : IDataSetStorage
	{
		Guid SFM_PK { get; set; }
		string SFM_FeatureCode { get; set; }
		string SFM_BillableCount { get; set; }
		string SFM_Reference1 { get; set; }
		string SFM_Reference2 { get; set; }
		string SFM_Reference3 { get; set; }
		string SFM_Reference4 { get; set; }
		string SFM_Reference5 { get; set; }
		string SFM_Category { get; set; }
		string SFM_PriceItemCode { get; set; }
		string SFM_ServiceOccuredUTC { get; set; }
		string SFM_ClientStaffCode { get; set; }
	}
}
