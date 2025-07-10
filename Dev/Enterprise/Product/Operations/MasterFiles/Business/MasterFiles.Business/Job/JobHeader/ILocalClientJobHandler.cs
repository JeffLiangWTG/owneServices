
namespace Enterprise.MasterFiles.Business
{
	public interface ILocalClientJobHandler : IJobHeaderParent
	{
		JobHeader JobHeader { get; }
		void CreateJobHeaderWithMutex();
	}
}
