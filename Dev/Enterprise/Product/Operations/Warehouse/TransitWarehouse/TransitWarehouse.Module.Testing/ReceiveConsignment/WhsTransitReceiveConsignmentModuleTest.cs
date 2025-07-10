using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitReceiveConsignmentModule))]
	public class WhsTransitReceiveConsignmentModuleTest : WhsTransitModuleTest<WhsTransitReceiveConsignmentModule>
	{
		protected override bool ExpectedAllowEdit => true;
		protected override bool ExpectedAllowView => true;

		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsTransitReceiveConsignmentFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsTransitReceiveConsignmentFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemReceiveConsignmentCollection);

		protected override string ExpectedWarehouseSchema => WhsTransitReceiveConsignmentFilterBusinessObject.Schema.Warehouse;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsTransitReceiveConsignment;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemReceiveConsignment;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var rcn = (WhsItemReceiveConsignment)factory.NewWithValidTestData(businessObjectType);
			rcn.WRC_WW_IntendedWarehouse = TWInCurrentBranch.PK;

			return rcn;
		}
	}
}
