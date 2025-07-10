using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderPackagePivot))]
	sealed class InvoiceHeaderPackagePivotClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();

			var bill = dec.Bills.AddNew();
			var packingGroup = bill.PackingGroups.AddNew();
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;

			var invoicePackagePivot = Factory.New<InvoiceHeaderPackagePivot>();
			invoicePackagePivot.CHZ_JE = dec.PK;
			invoicePackagePivot.CHZ_CW = package.PK;
			invoicePackagePivot.CHZ_JZ = dec.Invoices.AddNew().PK;

			return invoicePackagePivot;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();
	}
}
