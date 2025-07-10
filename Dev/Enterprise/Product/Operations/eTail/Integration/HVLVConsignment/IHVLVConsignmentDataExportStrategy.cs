using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eTail.Integration
{
	[Immutable]
	public interface IHVLVConsignmentDataExportStrategy
	{
		bool IsAllowSetForConsignmentDataExportContext(string fieldName);
	}
}
