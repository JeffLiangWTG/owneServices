using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackableItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCUI_PackableQty()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var item = packingList.PackableItems.AddNew();
			ValidationTestHelper.AssertErrorIfValueIsNegative(item.CUI_PackableQtyInfo);
		}

		public void TestCheckCUI_PackableUQ_Mandatory()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var item = packingList.PackableItems.AddNew();
			item.CUI_PackableQty = 1;
			ValidationTestHelper.AssertErrorIfNotEntered(item.CUI_PackableUQInfo);
			item.CUI_PackableQty = 0;
			item.CUI_PackableUQ = string.Empty;
			AssertNoErrorContaining("No Packable Qty", item.CUI_PackableUQInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCUI_PackableUQ_ListValidation()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var item = packingList.PackableItems.AddNew();

			item.CUI_PackableQty = 1;
			item.CUI_JI = ZGuid.Empty;
			CombineAssertions(() =>
			{
				item.CUI_PackableUQ = "X1";
				AssertHasWarning("Invalid", item.CUI_PackableUQInfo, ListValidation.InvalidCodeMessage);
				item.CUI_PackableUQ = CargoWise.Definitions.RefPackTypeStandardUnits.Codes.Inches;
				AssertNoWarning("Valid", item.CUI_PackableUQInfo, ListValidation.InvalidCodeMessage);
			});
		}

		public void TestCheckCUI_NetWeight()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = ZString.Replicate('A', BaseJobComInvoiceLine.Schema.JI_DescriptionMaxLength);
			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "ACR";
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var item = packingList.PackableItems.AddNew();

			item.CUI_JI = invoiceLine.PK;
			item.CUI_ClusterKey = 1;
			item.CUI_PackableQty = 10m;
			item.CUI_NetWeight = 20m;
			item.CUI_NetWeightUQ = "KG";

			CombineAssertions(() =>
			{
				const string expectedMessage = "The sum of Packed Item Net Weight 4 KG does not balance with the Invoice Line Net Weight 20 KG.";
				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item, 2m);
				item.Validation.ValidateCUI_NetWeight();
				AssertHasWarningContaining("Not matching", item.CUI_NetWeightInfo, "The sum of Packed Item Net Weight");
				AssertEquals("Exact message", true, item.CUI_NetWeightInfo.HasNotification(expectedMessage));
				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item, 8m);
				item.Validation.ValidateCUI_NetWeight();
				AssertNoWarningContaining("No other errors", item.CUI_NetWeightInfo, "The sum of Packed Item Net Weight");
			});
		}
	}
}
