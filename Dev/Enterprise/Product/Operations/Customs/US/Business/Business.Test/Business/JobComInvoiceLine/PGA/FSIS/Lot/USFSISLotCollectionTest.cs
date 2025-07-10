using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFSISLotCollection))]
	class USFSISLotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			FSISLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			Assert("Lots collection is not allowed to add new element when issuer country is AU", !FSISLine.Lots.AllowNew);
			FSISLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.UnitedStates;
			Assert("Lots collection is allowed to add new element when issuer country is US", FSISLine.Lots.AllowNew);
			FSISLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.NewZealand;
			Assert("Lots collection is not allowed to add new element when issuer country is NZ", !FSISLine.Lots.AllowNew);
		}

		protected override CargoWise.EntityFramework.BusinessObjectCollection GetCollectionToTest()
		{
			return FSISLine.Lots;
		}

		USInvoiceLineFSISLine FSISLine
		{
			get { return fFSISLine ?? (fFSISLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew()); }
		}
		USInvoiceLineFSISLine fFSISLine;
	}
}
