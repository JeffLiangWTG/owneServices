using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ShipmentReceivalModule))]
	sealed class ShipmentReceivalModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (ShipmentReceivalModule module = new ShipmentReceivalModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("CFSShipmentReceival business context should be returned", BusinessContext.CFSShipmentReceival, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestCannotDelete()
		{
			using (ShipmentReceivalModule module = new ShipmentReceivalModule())
			{
				Assert(!module.AllowDelete);
			}
		}

		public void TestSetGuiProviders()
		{
			using (ShipmentReceivalModuleForTest module = new ShipmentReceivalModuleForTest())
			{
				module.GetNewGridCollection();

				var shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearch();

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ShipmentReceival;
		}

		#region Implementation

		class ShipmentReceivalModuleForTest : ShipmentReceivalModule
		{
			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public void PerformSearch()
			{
				base.PerformSearch();
			}
		}

		#endregion
	}
}
