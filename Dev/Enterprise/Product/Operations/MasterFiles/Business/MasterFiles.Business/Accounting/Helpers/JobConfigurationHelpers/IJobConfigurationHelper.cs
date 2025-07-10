using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IJobConfigurationHelper
	{
		CodeDescriptionPairList GetLookupList();
		void Validate();
		bool GetReadOnlyStatus();
	}
}
