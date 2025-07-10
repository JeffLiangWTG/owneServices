using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public interface IModuleToModuleSender
	{
		PublishToUniversalResult CreateJob<T>(T businessEntity)
			where T : BusinessObject, IWorkflowProvider, IModuleToModule;
	}

	public interface IDeclarationModuleToModuleSender : IModuleToModuleSender
	{
	}
}