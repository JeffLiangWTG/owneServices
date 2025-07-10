using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListModule))]
	public class WhsItemDispatchLoadListModuleTest : WhsTransitModuleTest<WhsItemDispatchLoadListModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsItemDispatchLoadListFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsItemDispatchLoadListFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemDispatchLoadListCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsItemDispatchLoadList;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemDispatchLoadList;

		protected override string ExpectedWarehouseSchema => WhsItemDispatchLoadListFilterBusinessObject.Schema.Warehouse;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var dll = (WhsItemDispatchLoadList)factory.NewWithValidTestData(businessObjectType);
			dll.WDL_WW_Warehouse = TWInCurrentBranch.PK;

			return dll;
		}
	}
}
