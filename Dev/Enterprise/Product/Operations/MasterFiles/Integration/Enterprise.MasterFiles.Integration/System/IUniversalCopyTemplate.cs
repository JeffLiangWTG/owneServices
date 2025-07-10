using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IUniversalCopyTemplate
	{
		ZString ConfigurationName { get; }
		ZString ConfigurationSource { get; }
		ZGuid NominatedRecordPk { get; }
	}
}
