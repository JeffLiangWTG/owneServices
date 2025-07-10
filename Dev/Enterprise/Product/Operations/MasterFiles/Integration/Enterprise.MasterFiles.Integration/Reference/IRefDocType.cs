using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefDocType
	{
		ZGuid PK { get; }
		ZString RT_ReferenceType { get; set; }
		ZString RT_DocType { get; set; }
		ZString RT_Desc { get; set; }
		MultilingualString RT_DescMultilingual { get; }
		ZBool RT_LogSystemCreatedDocsToEDocs { get; set; }
	}
}
