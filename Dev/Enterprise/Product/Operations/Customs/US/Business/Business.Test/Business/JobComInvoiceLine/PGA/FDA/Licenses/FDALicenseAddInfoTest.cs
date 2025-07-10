using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDALicenseAddInfo))]
	sealed class FDALicenseAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new FDALicenseAddInfo(FDA.Licenses.AddNew().B7_AddInfoDataInfo);

		ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					fda = invoiceLine.ACE_FDALines.AddNew();
					fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				}
				return fda;
			}
		}
		ACEFDA fda;
	}
}
