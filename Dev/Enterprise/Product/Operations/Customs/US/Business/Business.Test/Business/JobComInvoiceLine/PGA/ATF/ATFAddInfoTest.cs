using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ATFAddInfo))]
	public class ATFAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var atfLine = invoiceLine.ATFLines.AddNew();
			AssertEquals(typeof(USImportATFAddInfoValidation), atfLine.AddInfoValidation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(USExportATFAddInfoValidation), atfLine.AddInfoValidation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var atf = Factory.New<ATF>();
			var addInfo = new ATFAddInfo(atf.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
