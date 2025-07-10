using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISRoutingAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRoutingTypeList()
		{
			var list = Routing.AddInfoLookups.RoutingTypeList;
			AssertEquals("RoutingTypeList", Factory.GetCachedValue<RoutingTypeList>(), list);
		}

		public void TestCountryList()
		{
			var list = Routing.AddInfoLookups.CountryList;
			AssertEquals("CountryList", typeof(RefCountryCollection), list.GetType());
		}

		#region Implementation

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

		APHISRouting Routing
		{
			get { return routing ?? (routing = Header.Routings.AddNew()); }
		}
		APHISRouting routing;

		#endregion
	}
}
