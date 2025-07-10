using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CusPersonController))]
	sealed class CusPersonControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CusPerson;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "John Locke";
			Factory.Save();
			return person;
		}
	}
}
