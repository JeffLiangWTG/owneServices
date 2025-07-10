using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitModule))]
	class WhsItemDispatchTransportationUnitModuleTest : WhsTransitModuleTest<WhsItemDispatchTransportationUnitModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsItemDispatchTransportationUnitFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsItemDispatchTransportationUnitFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemDispatchTransportationUnitCollection);

		protected override string ExpectedWarehouseSchema => WhsItemDispatchTransportationUnitFilterBusinessObject.Schema.Warehouse;

		protected override bool ExpectedAllowView => true;
		protected override bool ExpectedAllowEdit => true;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsItemDispatchTransportationUnit;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemDispatchTransportationUnit;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var dtu = (WhsItemDispatchTransportationUnit)factory.NewWithValidTestData(businessObjectType);
			dtu.WDH_WW_Warehouse = TWInCurrentBranch.PK;

			return dtu;
		}
	}
}
