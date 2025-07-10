using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCreditorGroupController))]
	sealed class OrgCreditorGroupControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgCreditorGroup group = factory.New<OrgCreditorGroup>();
			factory.Save();

			return group;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgCreditorGroup;
		}
	}
}
