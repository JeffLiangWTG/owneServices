using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFDAAddInfo))]
	public class FDAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFDA()
		{
			FDA fda = Factory.New<FDA>();
			USFDAAddInfo addInfo = new USFDAAddInfo(fda.B7_AddInfoDataInfo);
			AssertEquals(fda.PK, addInfo.Parent.PK);
		}

		[ExpectNoExceptions]
		public void TestSettingUS_FDAValueMarksInvoiceLineForValidation()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_LinePrice = 1000m;

			Factory.Save();

			FDA fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAValue = 100m;
			AssertEquals(true, invoiceLine.ShouldValidateOnSave);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			FDA fda = Factory.New<FDA>();
			USFDAAddInfo addInfo = new USFDAAddInfo(fda.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
