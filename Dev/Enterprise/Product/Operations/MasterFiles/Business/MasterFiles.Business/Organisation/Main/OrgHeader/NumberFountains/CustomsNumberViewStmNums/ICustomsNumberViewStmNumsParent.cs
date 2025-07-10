using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ICustomsNumberViewStmNumsParent : IBusiness
	{
		ZGuid PK { get; }
		string TablePrefix { get; }
		void RegisterEditableChildObject(IBusiness bizO);
		void UnRegisterEditableChildObject(IBusiness bizO);
		CustomsNumberViewStmNumsBusinessProvider CustomsNumberProvider { get; }
		string CustomsNumberProviderKey { get; }
	}
}
