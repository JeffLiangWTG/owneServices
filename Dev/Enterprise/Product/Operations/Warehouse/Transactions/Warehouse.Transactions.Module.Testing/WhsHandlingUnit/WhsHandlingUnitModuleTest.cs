using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Module.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsHandlingUnitModule))]
	public class WhsHandlingUnitModuleTest : HandlingUnitModuleTest<WhsHandlingUnitModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(WhsHandlingUnitFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(WhsHandlingUnitFilterControl);

		protected override Type ExpectedCollectionType => typeof(PkgHandlingUnitCollection);

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsPkgHandlingUnit;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsHandlingUnit;
	}
}
