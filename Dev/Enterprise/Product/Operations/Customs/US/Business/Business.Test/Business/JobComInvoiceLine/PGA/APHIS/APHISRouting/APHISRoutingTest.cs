using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISRouting))]
	public class APHISRoutingTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISRouting>
	{
		public void TestIAPHISRoutingMembers()
		{
			var routing = Header.Routings.AddNew();
			routing.US_Type = "T1";
			routing.US_Country = "C1";
			routing.US_State = "S1";
			IAPHISRouting iRouting = routing;
			AssertEquals("RoutingType", "T1", iRouting.RoutingType);
			AssertEquals("RoutingCountry", "C1", iRouting.RoutingCountry);
			AssertEquals("RoutingState", "S1", iRouting.RoutingState);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			var routing = aphisHeader.Routings.AddNew();
			routing.US_Country = "A";
			return routing;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Routings.AddNew();
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
