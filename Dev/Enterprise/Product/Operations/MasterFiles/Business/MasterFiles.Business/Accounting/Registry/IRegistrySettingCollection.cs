using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IRegistrySettingCollection
	{
		BusinessObject AddNew();
		void RunPreSaveValidation();
	}
}
