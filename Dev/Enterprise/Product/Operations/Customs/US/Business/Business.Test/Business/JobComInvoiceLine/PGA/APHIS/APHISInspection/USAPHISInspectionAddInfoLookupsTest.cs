using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISInspectionAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInspectionStatusList()
		{
			var list = Inspection.AddInfoLookups.InspectionStatusList;
			AssertEquals("InspectionStatusList", InspectionStatusList.GetListForAPHIS(Factory), list);
		}

		public void TestPortCodes()
		{
			var list = Inspection.AddInfoLookups.PortCodes;
			AssertNotNull(list);

			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			list = Inspection.AddInfoLookups.PortCodes;
			var refUNLOCOCollection = (RefUNLOCOCollection)list;
			AssertNotNull(refUNLOCOCollection);

			inspection.US_TestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection;
			list = Inspection.AddInfoLookups.PortCodes;
			var uscregionDistrictPortCollection = (ZZRefCusCodeListCombinedCollection)list;
			AssertNotNull(uscregionDistrictPortCollection);
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

		APHISInspection Inspection
		{
			get { return inspection ?? (inspection = Header.Inspections.AddNew()); }
		}
		APHISInspection inspection;

		#endregion
	}
}
