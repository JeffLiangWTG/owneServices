using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListWithInvoiceParentClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invoice = (BaseJobComInvoiceHeader)NewParentObject();
			var cusPackingList = Factory.New<CusPackingList>();
			cusPackingList.CUL_JZ = invoice.PK;
			return cusPackingList;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var invoiceLine = ClusterKeyEntityToTest.Invoice.InvoiceLines.AddNew();
			var packableItem = invoiceLine.CreateNewCusPackableItem();
			ClusterKeyEntityToTest.PackableItems.Add(packableItem);
			return new IClusterKeyWorker[] { packableItem };
		}

		/// <summary>
		/// CusPackingList must have a parent (JobDeclaration or JobComInvoiceHeader) as enforced by Constraint_MustHaveParent.
		/// </summary>
		protected override bool IsFkToParentMandatory() => true;

		new CusPackingList ClusterKeyEntityToTest => (CusPackingList)base.ClusterKeyEntityToTest;
	}
}
