using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLinePackagePivot))]
	sealed class InvoiceLinePackagePivotClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();

			var bill = dec.Bills.AddNew();
			var packingGroup = bill.PackingGroups.AddNew();
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;

			var invLinePackagePivot = Factory.New<InvoiceLinePackagePivot>();
			invLinePackagePivot.CHC_JE = dec.PK;
			invLinePackagePivot.CHC_CW = package.PK;
			invLinePackagePivot.CHC_JI = dec.Invoices.AddNew().InvoiceLines.AddNew().PK;

			return invLinePackagePivot;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();
	}
}
