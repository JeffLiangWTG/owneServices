using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefGlbReleaseNote : IDataSetStorage
	{
		Guid ZGF_PK { get; set; }
		bool ZGF_IsValid { get; set; }
		string ZGF_Category { get; set; }
		string ZGF_RN_NKCountryForReleaseNote { get; set; }
		string ZGF_Summary { get; set; }
		string ZGF_URL { get; set; }
		DateTime ZGF_ReleaseNoteDate { get; set; }
		string ZGF_Section { get; set; }
		string ZGF_MinVersion { get; set; }
		Guid ZGF_QuickStartPK { get; set; }
	}
}
