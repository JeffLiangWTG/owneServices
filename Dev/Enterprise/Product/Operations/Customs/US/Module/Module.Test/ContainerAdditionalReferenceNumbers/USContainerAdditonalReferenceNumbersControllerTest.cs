using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USContainerAdditonalReferenceNumbersController))]
	sealed class USContainerAdditonalReferenceNumbersControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new USContainerAdditonalReferenceNumbersController();
			AssertEquals(ModuleIDs.Containers, controller.ModuleID);
		}

		public void TestGetForm()
		{
			var controller = new USContainerAdditonalReferenceNumbersController() as ZControllerInternals;
			var container = Factory.New<ForwardingContainer>();
			using (var form = controller.GetForm(container))
			{
				AssertType<ContainersForm>(form);
			}
		}

		public void TestCheckPoints()
		{
			var controller = new USContainerAdditonalReferenceNumbersController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.None, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(null));
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			return container;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.ContainerAdditionalReferenceNumbers;
	}
}
