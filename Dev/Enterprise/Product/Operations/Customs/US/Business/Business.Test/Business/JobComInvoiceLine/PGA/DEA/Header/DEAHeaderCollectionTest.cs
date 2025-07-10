using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAHeaderCollection))]
	public class DEAHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultRegistrationNumber()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Declaration.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			var deaHeader1 = invoiceLine.DEAHeaders.AddNew();
			AssertEquals(ZString.Empty, deaHeader1.US_RegistrationNumber);
			Declaration.IOR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DEA, "123456789", "US");
			var deaHeader2 = invoiceLine.DEAHeaders.AddNew();
			AssertEquals("123456789", deaHeader2.US_RegistrationNumber);
		}

		public void TestDefaultCountryOfShipment()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "US";
			var deaHeader1 = invoiceLine.DEAHeaders.AddNew();
			AssertEquals("US", deaHeader1.US_CountryOfShipment);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			return new DEAHeaderCollection(invoiceLine);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
