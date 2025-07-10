using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISLicenseCollection))]
	public class APHISLicenseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNewAndIsClearedWhenNot()
		{
			AssertAllowNewAndIsClearedWhenNot("", true);
			AssertAllowNewAndIsClearedWhenNot("%", true);
			var list = new APHISCategoryTypeCodeList();
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts);
			AssertAllowNewAndIsClearedWhenNot(APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts, false);
			foreach (ICodeDescription pair in list)
			{
				AssertAllowNewAndIsClearedWhenNot(pair.Code, true);
			}
		}

		#region Implementation

		void AssertAllowNewAndIsClearedWhenNot(string categoryType, bool isAllowNew)
		{
			Header.US_CategoryType = "!";
			var licenses = Header.Licenses;
			var license = licenses.AddNew();
			Header.US_CategoryType = categoryType;
			AssertEquals("AllowNew", isAllowNew, licenses.AllowNew);
			AssertEquals("license.IsDeleted", !isAllowNew, license.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISLicenseCollection(Header);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		#endregion
	}
}
