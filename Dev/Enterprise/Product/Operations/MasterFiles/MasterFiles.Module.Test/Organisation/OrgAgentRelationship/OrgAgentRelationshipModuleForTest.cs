using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgAgentRelationshipModuleForTest : OrgAgentRelationshipModule
	{
		public OrgAgentRelationshipModuleForTest()
		{
		}

		public IFilterControl NewFilterControl
		{
			get
			{
				return GetNewFilterControl();
			}
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get
			{
				return GetNewGridCollection();
			}
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get
			{
				return GetNewFilterBusinessObject();
			}
		}

		public ZController NewController
		{
			get
			{
				return GetNewController();
			}
		}
	}
}
