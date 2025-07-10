using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgDebtorGroupController))]
	sealed class OrgDebtorGroupControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgDebtorGroup group = factory.New<OrgDebtorGroup>();
			factory.Save();

			return group;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgDebtorGroup;
		}
	}
}
