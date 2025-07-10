using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineComponentInventory))]
	sealed class JobComInvLineComponentInventoryClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invLine = (BaseJobComInvoiceLine)NewParentObject();
			var componentPart = Factory.New<OrgSupplierPart>();
			componentPart.OP_PartNum = "PARTNUM";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_JI = invLine.PK;
			return inventory;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invLine = Factory.New<BaseJobComInvoiceLine>();
			invLine.JI_JZ = invoice.PK;
			return invLine;
		}
	}
}
