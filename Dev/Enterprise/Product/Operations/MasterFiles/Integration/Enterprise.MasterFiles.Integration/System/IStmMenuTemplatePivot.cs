using CargoWise.Types;
using Enterprise.ExcelTemplates.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IStmMenuTemplatePivot
	{
		ZGuid PK { get; }
		ZString SI_DocumentTitle { get; set; }
		ZByte SI_Index { get; set; }
		ZBool SI_IsClientSpecific { get; set; }
		ZBool SI_IsSystemDefined { get; set; }
		ZString SI_MenuTemplateFilter { get; set; }
		ZBool SI_PrintByDefault { get; set; }
		ZString SI_PrintCopyType { get; set; }
		ZGuid SI_RT_DocType { get; set; }
		ZGuid SI_SO { get; set; }
		ZGuid SI_SU { get; set; }
		ZShort SI_TrailingLines { get; set; }
		ZString SI_DataStoreName { get; set; }

		IRefDocType DocType { get; }

		IStmMenuItem MenuItem { get; }
		IStmTemplate Template { get; }
	}
}
