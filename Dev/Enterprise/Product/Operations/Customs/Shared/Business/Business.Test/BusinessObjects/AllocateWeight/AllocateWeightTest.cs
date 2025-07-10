using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AllocateWeight))]
	sealed class AllocateWeightTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
			AssertEquals("Net Weight Unit Default should be KG", Core.Constants.Weight.Kilograms, allocateWeight.NetWeightUnit);
			AssertEquals("Gross Weight Unit Default should be KG", Core.Constants.Weight.Kilograms, allocateWeight.GrossWeightUnit);
			AssertEquals("Method Default should be Price", AllocateWeightMethodList.Codes.Price, allocateWeight.AllocateWeightMethod);
		}

		public void TestSelectedLinesNumbersFormatString()
		{
			InvoiceHeader.JZ_InvoiceNumber = "INV2";
			for (int i = 0; i < 8; i++)
			{
				InvoiceHeader.JobComInvoiceLines.AddNew();
			}
			var allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => c.JI_LineNo != 5));
			AssertEquals("INV2: 1-4, 6-8", allocateWeight.SelectedLinesNumbersFormatString);

			allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => c.JI_LineNo <= 2 || c.JI_LineNo > 5));
			AssertEquals("INV2: 1, 2, 6-8", allocateWeight.SelectedLinesNumbersFormatString);

			allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => c.JI_LineNo == 1 || c.JI_LineNo == 3 || c.JI_LineNo == 5 || c.JI_LineNo == 7));
			AssertEquals("INV2: 1, 3, 5, 7", allocateWeight.SelectedLinesNumbersFormatString);

			var invoiceHeader1 = Declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV1";
			for (int i = 0; i < 8; i++)
			{
				invoiceHeader1.JobComInvoiceLines.AddNew();
			}

			allocateWeight = new AllocateWeight(Declaration.Factory, Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => (c.JI_LineNo != 5 && c.JI_Calc_Invoice == "INV2") || ((c.JI_LineNo == 1 || c.JI_LineNo == 3 || c.JI_LineNo == 5 || c.JI_LineNo == 7) && c.JI_Calc_Invoice == "INV1")));
			AssertEquals("INV1: 1, 3, 5, 7; INV2: 1-4, 6-8", allocateWeight.SelectedLinesNumbersFormatString);

			allocateWeight = new AllocateWeight(Declaration.Factory, Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => ((c.JI_LineNo <= 2 || c.JI_LineNo > 5) && c.JI_Calc_Invoice == "INV2") || ((c.JI_LineNo == 1 || c.JI_LineNo == 3 || c.JI_LineNo == 5 || c.JI_LineNo == 7) && c.JI_Calc_Invoice == "INV1")));
			AssertEquals("INV1: 1, 3, 5, 7; INV2: 1, 2, 6-8", allocateWeight.SelectedLinesNumbersFormatString);

			allocateWeight = new AllocateWeight(Declaration.Factory, Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => ((c.JI_LineNo < 2 || c.JI_LineNo > 5) && c.JI_Calc_Invoice == "INV2") || ((c.JI_LineNo < 5 || c.JI_LineNo == 7) && c.JI_Calc_Invoice == "INV1")));
			AssertEquals("INV1: 1-4, 7; INV2: 1, 6-8", allocateWeight.SelectedLinesNumbersFormatString);

			allocateWeight = new AllocateWeight(Declaration.Factory, Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Where(c => ((c.JI_LineNo < 2 || c.JI_LineNo > 5) && c.JI_Calc_Invoice == "INV2") || (c.JI_LineNo == 6 && c.JI_Calc_Invoice == "INV1")));
			AssertEquals("INV1: 6; INV2: 1, 6-8", allocateWeight.SelectedLinesNumbersFormatString);
		}

		public void TestNetWeightUnitAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AllocateWeight), "NetWeightUnit", true, attrib => attrib.ListDataSourceMember == "Lookups.WeightUQList");
		}

		public void TestGrossWeightUnitAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AllocateWeight), "GrossWeightUnit", true, attrib => attrib.ListDataSourceMember == "Lookups.WeightUQList");
		}

		public void TestAllocateWeightMethodAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AllocateWeight), "AllocateWeightMethod", true, attrib => attrib.ListDataSourceMember == "Lookups.AllocateWeightMethodList");
		}

		public void TestLookupsType()
		{
			var allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
			AssertType<AllocateWeightLookups>(allocateWeight.Lookups);
		}

		public void TestIsPrice()
		{
			var allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
			Assert(allocateWeight.IsPrice);

			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Quantity;
			Assert(!allocateWeight.IsPrice);
		}

		public void TestIsQuantity()
		{
			var allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
			Assert(!allocateWeight.IsQuantity);

			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Quantity;
			Assert(allocateWeight.IsQuantity);
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					declaration.Invoices.AddNew();
					Factory.Save();
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		BaseJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return Declaration.Invoices[0];
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
		}
	}
}
