using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderModule))]
	public class WhsItemTransferHeaderModuleTest : WhsTransitModuleTest<WhsItemTransferHeaderModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsItemTransferHeaderFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsItemTransferHeaderFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsItemTransferHeaderCollection);

		protected override string ExpectedWarehouseSchema => WhsItemTransferHeaderFilterBusinessObject.Schema.Warehouse;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsItemTransferHeader;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsItemTransferHeader;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var trf = (WhsItemTransferHeader)factory.NewWithValidTestData(businessObjectType);
			trf.WTH_WW_Warehouse = TWInCurrentBranch.PK;

			return trf;
		}
	}
}
