using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagLink : IBusiness
	{
		ZGuid PK { get; }
		ZGuid TGL_ParentId { get; set; }
		ZString TGL_ParentTableCode { get; set; }
		ZString TGL_Description { get; set; }
		ZGuid TGL_TGM_Magnitude { get; set; }
		ZDecimal TGL_Magnitude { get; set; }
		ZGuid TagDefinitionPk { get; set; }

		ZDateTime TGL_SystemCreateTimeUtc { get; set; }
		ZString TGL_SystemCreateUser { get; set; }
		ZDateTime TGL_SystemLastEditTimeUtc { get; set; }
		ZString TGL_SystemLastEditUser { get; set; }
		ZDecimal EffectiveNudge { get; }
		ITagMagnitude TagMagnitude { get; }
	}
}
