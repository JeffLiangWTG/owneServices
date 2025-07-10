using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISLicense))]
	public class APHISLicenseTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISLicense>
	{
		public void TestIAPHISLicenseMembers()
		{
			var license = Header.Licenses.AddNew();
			license.US_Date = new ZDateTime(2015, 6, 1);
			license.US_DateQualifier = "Q";
			license.US_StateDescription = "RIGHT HERE";
			license.US_RN_CountryCode = "LT";
			license.US_Number = "N2";
			license.US_Quantity = 10m;
			license.US_Type = "T1";
			license.US_UnitOfMeasure = "KG";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "CUSTOMS BROKER";
			org.OH_RL_NKClosestPort = "USLAX";
			var customsBroker = org.MainAddress;
			customsBroker.OA_Address1 = "CB ADDRESS 1";
			customsBroker.OA_Address2 = "CB ADDRESS 2";
			customsBroker.OA_RL_NKRelatedPortCode = "USLAX";
			customsBroker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32-32", Core.Constants.CountryCodes.UnitedStates);

			IAPHISLicense iLicense = license;
			AssertEquals("Date", new ZDate(2015, 6, 1), iLicense.Date);
			AssertEquals("DateQualifier", "Q", iLicense.DateQualifier);
			AssertEquals("LocationDescription", "RIGHT HERE", iLicense.LocationDescription);
			AssertEquals("LocationType", "LT", iLicense.Location);
			AssertEquals("Number", "N2", iLicense.Number);
			AssertEquals("Quantity", 10m, iLicense.Quantity);
			AssertEquals("TransactionType(Not used)", ZString.Empty, iLicense.TransactionType);
			AssertEquals("Type", "T1", iLicense.Type);
			AssertEquals("UnitOfMeasure", "KG", iLicense.UnitOfMeasure);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			var license = aphisHeader.Licenses.AddNew();
			license.US_Quantity = 1.0m;
			return license;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Licenses.AddNew();
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
