using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ShipmentGatePassModule))]
	sealed class ShipmentGatePassModuleBasherTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (ShipmentGatePassModule module = new ShipmentGatePassModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("GatePass business context should be returned", BusinessContext.GatePass, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestSetGuiProviders()
		{
			using (ShipmentGatePassModuleForTest module = new ShipmentGatePassModuleForTest())
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

		public void TestAllowDelete()
		{
			using (ShipmentGatePassModule module = new ShipmentGatePassModule())
			{
				Assert("Should not allow delete!", !module.AllowDelete);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ShipmentGatePass;
		}

		#region Implementation

		class ShipmentGatePassModuleForTest : ShipmentGatePassModule
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

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var shipment = factory.NewWithValidTestData<GatePassShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_TranshipToOtherCFS = true;

			return shipment;
		}

		#endregion
	}
}
