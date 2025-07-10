using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowAffectedPropertyProvider
	{
		ZPropertyInfo[] PropertyThatAffectWorkflowChanged { get; }
	}
}
