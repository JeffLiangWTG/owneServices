using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProcessTaskFilterBusinessObjectForTest : ProcessTaskFilterBusinessObject
	{
		public ProcessTaskFilterBusinessObjectForTest()
		{
			QueryObjectType = typeof(ProcessTask);
		}
	}
}
