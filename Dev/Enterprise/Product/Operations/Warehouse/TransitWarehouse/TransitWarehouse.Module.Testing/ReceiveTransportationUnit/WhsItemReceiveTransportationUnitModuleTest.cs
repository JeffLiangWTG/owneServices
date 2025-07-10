using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitModule))]
	public class WhsItemReceiveTransportationUnitModuleTest : WhsTransitModuleTest<WhsItemReceiveTransportationUnitModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsItemReceiveTransportationUnitFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsItemReceiveTransportationUnitFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemReceiveTransportationUnitCollection);

		protected override string ExpectedWarehouseSchema => WhsItemReceiveTransportationUnitFilterBusinessObject.Schema.Warehouse;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsItemReceiveTransportationUnit;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemReceiveTransportationUnit;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var rtu = (WhsItemReceiveTransportationUnit)factory.NewWithValidTestData(businessObjectType);
			rtu.WRH_WW_Warehouse = TWInCurrentBranch.PK;

			return rtu;
		}
	}
}
