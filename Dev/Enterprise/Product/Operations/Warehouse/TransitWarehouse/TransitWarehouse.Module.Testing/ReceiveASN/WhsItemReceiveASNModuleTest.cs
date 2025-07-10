using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemReceiveASNModule))]
	public class WhsItemReceiveASNModuleTest : WhsTransitModuleTest<WhsItemReceiveASNModule>
	{
		protected override bool ExpectedAllowEdit => true;
		protected override bool ExpectedAllowView => true;

		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsItemReceiveASNFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsItemReceiveASNFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemReceiveASNCollection);

		protected override string ExpectedWarehouseSchema => WhsItemReceiveASNFilterBusinessObject.Schema.Warehouse;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsItemReceiveASN;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemReceiveASN;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var asn = (WhsItemReceiveASN)factory.NewWithValidTestData(businessObjectType);
			asn.WRP_WW_IntendedWarehouse = TWInCurrentBranch.PK;

			return asn;
		}
	}
}
