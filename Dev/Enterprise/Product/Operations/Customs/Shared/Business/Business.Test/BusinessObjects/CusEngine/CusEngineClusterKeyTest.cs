using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusEngine))]
sealed class CusEngineClusterKeyParentIsCusVehicleTest : CusEngineClusterKeyTest
{
	protected override EnterpriseBusinessObject NewParentObject()
	{
		var dec = Factory.New<BaseJobDeclaration>();
		var header = dec.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		var vehicle = line.Vehicles.AddNew();
		vehicle.CVH_DataModel = "AU";

		return vehicle;
	}
}

[TestedType(typeof(CusEngine))]
sealed class CusEngineClusterKeyParentIsInvoiceLineTest : CusEngineClusterKeyTest
{
	protected override EnterpriseBusinessObject NewParentObject()
	{
		var dec = Factory.New<BaseJobDeclaration>();
		var header = dec.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();

		return line;
	}
}

public abstract class CusEngineClusterKeyTest : ClusterKeyWorkerMandatoryTest
{
	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var parent = NewParentObject();
		var engine = Factory.NewWithValidTestData<CusEngine>();
		engine.CEG_ParentID = parent.PK;
		engine.CEG_ParentTableCode = parent.TablePrefix;

		return engine;
	}

	protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
	{
		return null;
	}
}
