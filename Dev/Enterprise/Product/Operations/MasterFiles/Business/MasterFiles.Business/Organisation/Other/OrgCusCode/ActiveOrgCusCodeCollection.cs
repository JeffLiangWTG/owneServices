using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.OrgCusCode)]
	public class ActiveOrgCusCodeCollection : ActiveBusinessObjectCollection<OrgCusCode>
	{
		public ActiveOrgCusCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
