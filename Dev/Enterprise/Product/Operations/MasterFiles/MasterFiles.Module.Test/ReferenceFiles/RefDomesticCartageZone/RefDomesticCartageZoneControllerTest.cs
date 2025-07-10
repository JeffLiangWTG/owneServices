using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDomesticCartageZoneController))]
	sealed class RefDomesticCartageZoneControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefDomesticCartageZone;
		}

		[ExpectException(typeof(ModuleGuiNotSupportedException))]
		public override void TestViewForm()
		{
			Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase());
		}
	}
}
