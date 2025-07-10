using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusVehicle))]
sealed class CusVehicleClusterKeyParentIsInvoiceLineTest : CusVehicleClusterKeyTest
{
	protected override EnterpriseBusinessObject NewParentObject()
	{
		var dec = Factory.New<BaseJobDeclaration>();
		var header = dec.Invoices.AddNew();

		return header.InvoiceLines.AddNew();
	}
}

[TestedType(typeof(CusVehicle))]
sealed class CusVehicleClusterKeyParentIsJobDeclarationTest : CusVehicleClusterKeyTest
{
	protected override EnterpriseBusinessObject NewParentObject()
	{
		return Factory.New<BaseJobDeclaration>();
	}
}

[TestedType(typeof(CusVehicle))]
sealed class CusVehicleClusterKeyParentIsOrgSupplierPartTest : TestCaseWithFactory
{
	public void TestParentIsOrgSupplierPart()
	{
		var part = Factory.NewWithValidTestData<OrgSupplierPart>();
		var vehicle = Factory.NewWithValidTestData<CusVehicle>();
		vehicle.CVH_ParentID = part.PK;
		vehicle.CVH_ParentTableCode = part.TablePrefix;
		vehicle.CVH_DataModel = "AU";

		Factory.Save();

		AssertEquals(0, vehicle.CVH_ClusterKey);
	}
}

public abstract class CusVehicleClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var parent = NewParentObject();
		var vehicle = Factory.NewWithValidTestData<CusVehicle>();
		vehicle.CVH_ParentID = parent.PK;
		vehicle.CVH_ParentTableCode = parent.TablePrefix;
		return vehicle;
	}

	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
	{
		var cusVehicle = (CusVehicle)ClusterKeyEntityToTest;
		var engine = Factory.NewWithValidTestData<CusEngine>();
		engine.CEG_ParentID = cusVehicle.PK;
		engine.CEG_ParentTableCode = cusVehicle.TablePrefix;
		return [engine];
	}
}
