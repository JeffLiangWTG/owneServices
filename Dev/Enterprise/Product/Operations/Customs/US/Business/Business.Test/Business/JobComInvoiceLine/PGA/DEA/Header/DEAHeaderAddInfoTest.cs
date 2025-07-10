using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAHeaderAddInfo))]
	public class DEAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			AssertEquals(typeof(USImportDEAHeaderAddInfoValidation), deaHeader.AddInfoValidation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(USExportDEAHeaderAddInfoValidation), deaHeader.AddInfoValidation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<DEAHeader>();
			var addInfo = new DEAHeaderAddInfo(header.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
