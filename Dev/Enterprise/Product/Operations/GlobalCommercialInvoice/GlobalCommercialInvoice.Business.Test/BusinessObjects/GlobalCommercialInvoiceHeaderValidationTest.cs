using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	public class GlobalCommercialInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		const string EmptyValueMessage = "Please enter a value.";

		public void TestGIH_InvoiceNumber_HasNoDuplicate_WarningMessageNotExists()
		{
			var (_, headerCollection, _) = CreateTestInvoiceNumberValidationObjects();

			AssertDuplicateInvoiceNumberWarnings(headerCollection, 0);
		}

		public void TestGIH_InvoiceNumber_HasDuplicate_WarningMessageExists()
		{
			var (_, headerCollection, _) = CreateTestInvoiceNumberValidationObjects();

			headerCollection[0].GIH_InvoiceNumber = "INV001";
			headerCollection[2].GIH_InvoiceNumber = "INV001";

			AssertDuplicateInvoiceNumberWarnings(headerCollection, 2);

			headerCollection[0].GIH_InvoiceNumber = "INV000";

			AssertDuplicateInvoiceNumberWarnings(headerCollection, 0);
		}

		public void TestGIH_InvoiceNumber_HasNoDuplicateAndInsertDuplicated_WarningMessageExists()
		{
			var (shipment, headerCollection, _) = CreateTestInvoiceNumberValidationObjects();

			headerCollection[0].GIH_InvoiceNumber = "INV001";
			headerCollection[2].GIH_InvoiceNumber = "INV003";
			headerCollection.CreateInvoiceHeader(shipment).GIH_InvoiceNumber = "INV001";

			AssertDuplicateInvoiceNumberWarnings(headerCollection, 2);
		}

		public void TestGIH_InvoiceNumber_HasDuplicateAndDelete_WarningMessageNotExists()
		{
			var (_, headerCollection, headers) = CreateTestInvoiceNumberValidationObjects();

			headerCollection[0].GIH_InvoiceNumber = "INV001";
			headerCollection[2].GIH_InvoiceNumber = "INV001";
			headerCollection[0].Delete();

			AssertDuplicateInvoiceNumberWarnings(headerCollection, 0);
			AssertEquals(message: "Marked Deleted", expected: true, actual: headers[0].IsDeleted);
		}

		public void TestGIH_InvoiceNumber_WithEmptyValue_ShowError()
		{
			var header = Factory.NewWithValidTestData<GlobalCommercialInvoiceHeader>();
			header.GIH_InvoiceNumber = string.Empty;
			AssertHasErrors(EmptyValueMessage, header.GIH_InvoiceNumberInfo);

			header.GIH_InvoiceNumber = new string('X', AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceNumberMaxLength);
			AssertNoErrors(header.GIH_InvoiceNumberInfo);
		}

		public void TestGIH_InvoiceDate_WithEmptyValue_ShowError()
		{
			var header = Factory.NewWithValidTestData<GlobalCommercialInvoiceHeader>();
			header.GIH_InvoiceDate = ZDate.Empty;
			AssertHasErrors(EmptyValueMessage, header.GIH_InvoiceDateInfo);

			header.GIH_InvoiceDate = new ZDate(2025, 12, 28);
			AssertNoErrors(header.GIH_InvoiceDateInfo);
		}

		public void TestGIH_Description_WithEmptyValue_IsValid()
		{
			var header = Factory.NewWithValidTestData<GlobalCommercialInvoiceHeader>();
			header.GIH_Description = string.Empty;
			AssertNoErrors(header.GIH_DescriptionInfo);

			header.GIH_Description = new string('X', AutoGlobalCommercialInvoiceHeader.Schema.GIH_DescriptionMaxLength);
			AssertNoErrors(header.GIH_DescriptionInfo);
		}

		(BusinessObject shipment, GlobalCommercialInvoiceHeaderCollection headerCollection, GlobalCommercialInvoiceHeader[] headers) CreateTestInvoiceNumberValidationObjects()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var headers = headerCollection.CreateInvoiceHeader(shipment, 3);

			return (shipment, headerCollection, headers);
		}

		void AssertDuplicateInvoiceNumberWarnings(GlobalCommercialInvoiceHeaderCollection headerCollection, int number)
		{
			const string duplicateWarningMessage = "This invoice number already exists in this job";

			AssertEquals(
				number == 0 ? "No Warning" : "Has Warning",
				number,
				headerCollection.Count(h => h.GIH_InvoiceNumberInfo.HasWarning(duplicateWarningMessage)));
		}
	}
}
