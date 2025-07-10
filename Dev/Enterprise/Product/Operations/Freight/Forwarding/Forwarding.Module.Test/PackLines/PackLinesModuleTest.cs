using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PackLinesModule))]
	class PackLinesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PackLines;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var packLine = factory.NewWithValidTestData<ForwardingPackLine>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			packLine.JL_JS = shipment.PK;
			packLine.JL_FreightMode = FreightConstants.OuterPackType;

			factory.Save();

			base.AddTestObjects(collection);
		}
	}
}
