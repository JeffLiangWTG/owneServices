using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class BaseGroupInvoiceAllInvoiceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderActiveCollection>
	{
		public void TestAllInvoicesAfterGroupHeaderIsMoved()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup1.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice3 = subGroup2.JobComInvoiceHeaders.AddNew();

			AssertEquals("All invoices", 3, declaration.Invoices.Count);
			AssertEquals(3, topGroup.AllJobComInvoiceHeaders.Count);
			AssertEquals(1, subGroup1.AllJobComInvoiceHeaders.Count);
			AssertEquals(1, subGroup2.AllJobComInvoiceHeaders.Count);

			((IGroupInvoiceOrInvoice)subGroup2).Move(topGroup, subGroup1);
			AssertEquals(3, topGroup.AllJobComInvoiceHeaders.Count);
			AssertEquals(2, subGroup1.AllJobComInvoiceHeaders.Count);
			AssertEquals(1, subGroup2.AllJobComInvoiceHeaders.Count);
		}

		public void TestHasInvoicesWithValuationDateOverride()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = groupHeader.JobComInvoiceGroupHeaders.AddNew();

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();

			AssertEquals("HasInvoicesWithValuationDateOverride for GroupHeader", false, groupHeader.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);
			AssertEquals("HasInvoicesWithValuationDateOverride for SubGroup", false, subGroup.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);

			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 1);
			AssertEquals("HasInvoicesWithValuationDateOverride for GroupHeader", true, groupHeader.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);
			AssertEquals("HasInvoicesWithValuationDateOverride for SubGroup", false, subGroup.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);

			invoice1.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 1);
			AssertEquals("HasInvoicesWithValuationDateOverride for GroupHeader", true, groupHeader.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);
			AssertEquals("HasInvoicesWithValuationDateOverride for SubGroup", true, subGroup.AllJobComInvoiceHeaders.HasInvoicesWithValuationDateOverride);
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return TestDec.JobComInvoiceGroupHeaders[0].AllJobComInvoiceHeaders;
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = BaseJobDeclaration.New(Factory);
				}

				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceGroupHeader groupHeader = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader result = Factory.New<BaseJobComInvoiceHeader>();
			result.JZ_JE = TestDec.PK;
			result.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			return result;
		}
	}
}
