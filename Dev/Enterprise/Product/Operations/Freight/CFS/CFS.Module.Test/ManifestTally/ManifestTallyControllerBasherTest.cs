using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ManifestTallyController))]
	sealed class ManifestTallyControllerBasherTest : ZControllerBasherTest
	{
		public void TestCFSContextService()
		{
			AssertEquals("CFS controllers must set CFSContextService on factory", FreightDomainContext.CFS, Controller.Factory.GetFreightDomainContext());
		}

		public void TestShipmentsChildEditable()
		{
			ManifestTallyController controller = new ManifestTallyController();
			TallyContainer container = controller.Factory.New<TallyContainer>();
			Assert(container.IsRegisteredEditableChildObject(container.PackUnpackShipments));
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			Factory.Save();
			return container;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ManifestTally;
		}

		#endregion
	}
}
