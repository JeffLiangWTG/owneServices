using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefStlScript : IDataSetStorage
	{
		Guid STL_PK { get; set; }
		string STL_FeatureCode { get; set; }
		string STL_RoleName { get; set; }
		string STL_ModuleName { get; set; }
		string STL_FunctionName { get; set; }
		string STL_FeatureName { get; set; }
		string STL_DataGranularity { get; set; }
		string STL_CompanyCode { get; set; }
		string STL_BranchCode { get; set; }
		string STL_TransactionDateUtc { get; set; }
		string STL_CreatingUserCode { get; set; }
		string STL_GuidReference { get; set; }
		string STL_BillingReference1 { get; set; }
		string STL_BillingReference2 { get; set; }
		string STL_BillingReference3 { get; set; }
		string STL_BillingReference4 { get; set; }
		string STL_AdditionalRefs { get; set; }
		string STL_TransactionCount { get; set; }
		string STL_PreparationScript { get; set; }
		string STL_FromClause { get; set; }
		string STL_WhereClause { get; set; }
		bool STL_WithOptionRecompile { get; set; }
		bool STL_UsedInBilling { get; set; }
		string STL_ActiveOn { get; set; }
		string STL_MinCW1Version { get; set; }
		string STL_MaxCW1Version { get; set; }
		string STL_DateType { get; set; }
		Nullable<System.DateTime> STL_CollectionStartDateUtc { get; set; }
	}
}
