using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseGroupHeaderInvoiceLineViewCollection))]
	sealed class BaseSubInvoiceLineViewTestCase : BusinessObjectCollectionViewTestCase<BaseGroupHeaderInvoiceLineViewCollection>
	{
		protected override BaseGroupHeaderInvoiceLineViewCollection GetCollectionToTest()
		{
			Setup();
			return topGroupHeader.AllJobComInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Setup();
			BaseJobComInvoiceLine line = Factory.New<BaseJobComInvoiceLine>();
			line.JI_JZ = invoice1.PK;
			return line;
		}

		bool isSetup;
		void Setup()
		{
			if (!isSetup)
			{
				testDec = Factory.New<BaseJobDeclaration>();
				topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
				subGroupHeader1 = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
				subGroupHeader1.JZ_InvoiceNumber = "Sub1";
				testDec.ActiveGroupHeader.SwapGroup(subGroupHeader1);
				invoice1 = subGroupHeader1.JobComInvoiceHeaders.AddNew();
				invoice1.JZ_InvoiceNumber = "1";
				isSetup = true;
			}
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroupHeader;
		BaseJobComInvoiceGroupHeader subGroupHeader1;
		BaseJobComInvoiceHeader invoice1;
	}
}
