using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListWithDeclarationParentClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyWhenChangingParentTable()
		{
			// Set a Declaration as PackingList parent
			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("PackingList ClusterKey", 0, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Declaration (current parent) ClusterKey", 0, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice ClusterKey", 0, invoice.JZ_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After 1st Save (Declaration as parent)", () =>
			{
				AssertEquals("PackingList ClusterKey", 1, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Declaration (current parent) ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice ClusterKey", 1, invoice.JZ_ClusterKey);
			});

			// Set new stand-alone Invoice as PackingList parent
			var standAloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			ClusterKeyEntityToTest.CUL_JE = ZGuid.Empty;
			ClusterKeyEntityToTest.CUL_JZ = standAloneInvoice.PK;
			Factory.Save();

			CombineAssertions("After setting FK to an Invoice parent.", () =>
			{
				AssertEquals("PackingList ClusterKey", 2, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Declaration ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Standalone Invoice (current parent) ClusterKey", 2, standAloneInvoice.JZ_ClusterKey);
			});

			// Set a Declaration-attached Invoice as PackingList parent
			ClusterKeyEntityToTest.CUL_JZ = invoice.PK;
			Factory.Save();

			CombineAssertions("After setting FK to another Invoice parent.", () =>
			{
				AssertEquals("PackingList ClusterKey", 1, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Declaration ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice (current parent) ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Standalone Invoice ClusterKey", 2, standAloneInvoice.JZ_ClusterKey);
			});

			// Set another Declaration as PackingList parent
			var anotherDeclaration = Factory.New<BaseJobDeclaration>();
			ClusterKeyEntityToTest.CUL_JE = anotherDeclaration.PK;
			ClusterKeyEntityToTest.CUL_JZ = ZGuid.Empty;
			Factory.Save();

			CombineAssertions("After setting FK to a another Declaration parent.", () =>
			{
				AssertEquals("PackingList ClusterKey", 3, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Original Declaration ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Standalone Invoice ClusterKey", 2, standAloneInvoice.JZ_ClusterKey);
				AssertEquals("Another Declaration (current parent) ClusterKey", 3, anotherDeclaration.JE_ClusterKey);
			});

			// Set original Declaration as PackingList parent
			ClusterKeyEntityToTest.CUL_JE = declaration.PK;
			Factory.Save();

			CombineAssertions("After setting FK back to original Declaration parent.", () =>
			{
				AssertEquals("PackingList ClusterKey", 1, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Original Declaration (current parent) ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Standalone Invoice ClusterKey", 2, standAloneInvoice.JZ_ClusterKey);
				AssertEquals("Another Declaration ClusterKey", 3, anotherDeclaration.JE_ClusterKey);
			});

			// Set original Declaration-attached Invoice as PackingList parent
			ClusterKeyEntityToTest.CUL_JE = ZGuid.Empty;
			ClusterKeyEntityToTest.CUL_JZ = invoice.PK;
			Factory.Save();

			CombineAssertions("After setting FK an Invoice parent attached to original Declaration .", () =>
			{
				AssertEquals("PackingList ClusterKey", 1, ClusterKeyEntityToTest.CUL_ClusterKey);
				AssertEquals("Original Declaration ClusterKey", 1, declaration.JE_ClusterKey);
				AssertEquals("Dec-Attached Invoice (current parent) ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Standalone Invoice ClusterKey", 2, standAloneInvoice.JZ_ClusterKey);
				AssertEquals("Another Declaration ClusterKey", 3, anotherDeclaration.JE_ClusterKey);
			});
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var parentDec = (BaseJobDeclaration)NewParentObject();
			var cusPackingList = Factory.New<CusPackingList>();
			cusPackingList.CUL_JE = parentDec.PK;
			return cusPackingList;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			return declaration;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var invoiceLine = ClusterKeyEntityToTest.Declaration.Invoices.First().InvoiceLines.AddNew();
			var packableItem = invoiceLine.CreateNewCusPackableItem();
			ClusterKeyEntityToTest.PackableItems.Add(packableItem);
			return new IClusterKeyWorker[] { packableItem };
		}

		/// <summary>
		/// CusPackingList must have a parent (JobDeclaration or JobComInvoiceHeader) as enforced by Constraint_MustHaveParent.
		/// </summary>
		protected override bool IsFkToParentMandatory() => true;

		BaseJobDeclaration declaration;
		BaseJobComInvoiceHeader invoice;

		new CusPackingList ClusterKeyEntityToTest => (CusPackingList)base.ClusterKeyEntityToTest;
	}
}
