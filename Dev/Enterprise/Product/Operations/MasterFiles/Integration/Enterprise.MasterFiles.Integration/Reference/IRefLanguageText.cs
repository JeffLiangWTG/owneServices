using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefLanguageText
	{
		ZGuid PK { get; }
		ZString RLT_ColumnName { get; set; }
		ZBool RLT_IsClientOverridden { get; set; }
		ZString RLT_Language { get; set; }
		ZGuid RLT_ParentId { get; set; }
		ZString RLT_ParentTableCode { get; set; }
		ZString RLT_Text { get; set; }
		ZBool RLT_IsSystem { get; set; }
	}
}
