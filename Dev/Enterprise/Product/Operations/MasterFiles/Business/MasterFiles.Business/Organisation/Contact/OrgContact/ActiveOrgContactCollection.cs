using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgContacts)]
	public class ActiveOrgContactCollection : ActiveBusinessObjectCollection<OrgContact>
	{
		public ActiveOrgContactCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ActiveOrgContactCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNew => false;
	}
}
