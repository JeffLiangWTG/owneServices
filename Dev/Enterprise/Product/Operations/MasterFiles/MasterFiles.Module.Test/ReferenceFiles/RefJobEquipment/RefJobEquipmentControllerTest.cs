using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefJobEquipmentController))]
	class RefJobEquipmentControllerTest : ZControllerBasherTest
	{
		public void TestImplementation()
		{
			AssertEquals(ModuleIDs.RefJobEquipment, Controller.ModuleID);
			AssertEquals(typeof(JobEquipment), Controller.TypeOfTopLevelBusinessObject);
			AssertEquals(expected: true, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
		protected override ControllerID GetControllerID()
			=> ControllerIDs.RefJobEquipment;
	}
}
