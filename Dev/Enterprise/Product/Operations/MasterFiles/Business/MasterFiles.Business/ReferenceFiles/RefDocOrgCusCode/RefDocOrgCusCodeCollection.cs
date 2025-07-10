using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefDocOrgCusCode)]
	public class RefDocOrgCusCodeCollection : ActiveBusinessObjectCollection<RefDocOrgCusCode>
	{
		public RefDocOrgCusCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
