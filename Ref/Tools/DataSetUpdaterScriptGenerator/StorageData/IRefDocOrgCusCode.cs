using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefDocOrgCusCode : IDataSetStorage
	{
		Guid DOC_PK { get; set; }
		string DOC_RN_NKRegulatingCountry { get; set; }
		string DOC_RN_NKCodeCountry { get; set; }
		string DOC_CodeType { get; set; }
		string DOC_DocumentType { get; set; }
		byte DOC_Priority { get; set; }
		string DOC_Notes { get; set; }
		string DOC_ShortLabel { get; set; }
		string DOC_LongLabel { get; set; }
		string DOC_Description { get; set; }
		string DOC_Direction { get; set; }
	}
}
