using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IModuleToModule
	{
		bool CanExportData(out ZString errorMessage);
		BusinessObject GetRelatedObject();
		IOrgHeader RecipientOrganisation { get; }
		void AddToRelatedJobs(BusinessObject loadedJob);
	}
}