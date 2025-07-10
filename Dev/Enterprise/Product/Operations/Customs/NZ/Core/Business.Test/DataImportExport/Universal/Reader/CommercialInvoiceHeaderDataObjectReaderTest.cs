using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestUpdateInvoiceQuantityWithOversizeValue()
		{
			var largeValue = (ZDecimal)int.MaxValue + 10.0m;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
				{
					new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceQuantity = largeValue,
					}
				}));
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.NewZealand);
			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, logger, helper, declaration.TopGroupInvoice);
			var exception = AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
			AssertEquals("Should include error information", $"Value '{largeValue}' of 'Invoice Quantity' is too large. It cannot be greater than {int.MaxValue}", exception.Message);
		}
	}
}
