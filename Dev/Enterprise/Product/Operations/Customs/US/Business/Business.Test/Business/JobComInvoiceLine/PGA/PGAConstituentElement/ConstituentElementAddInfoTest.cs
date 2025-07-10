using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ConstituentElementAddInfo))]
	public class ConstituentElementAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstituentElement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var pga = invoiceLine.LaceyActLines.AddNew();
			var element = pga.PG04ConstituentElements.AddNew();
			var addInfo = new ConstituentElementAddInfo(element.B7_AddInfoDataInfo);
			AssertEquals(element.PK, addInfo.ConstituentElement.PK);
		}

		public void TestConstituentElementAddInfoValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var pga = invoiceLine.LaceyActLines.AddNew();
			var element = pga.PG04ConstituentElements.AddNew();
			ConstituentElementAddInfo addInfo = new ConstituentElementAddInfo(element.B7_AddInfoDataInfo);
			AssertEquals("ACSConstituentElementAddInfoValidation validation type", typeof(ACSConstituentElementAddInfoValidation), addInfo.Validation.GetType());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			addInfo = new ConstituentElementAddInfo(element.B7_AddInfoDataInfo);
			AssertEquals("ACE>ACEConstituentElementAddInfoValidation validation type", typeof(ACEConstituentElementAddInfoValidation), addInfo.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var pga = invoiceLine.LaceyActLines.AddNew();
			var element = pga.PG04ConstituentElements.AddNew();
			var addInfo = new ConstituentElementAddInfo(element.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
