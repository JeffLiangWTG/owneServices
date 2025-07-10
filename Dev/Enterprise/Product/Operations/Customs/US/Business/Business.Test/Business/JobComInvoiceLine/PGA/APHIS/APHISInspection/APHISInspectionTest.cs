using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISInspection))]
	public class APHISInspectionTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISInspection>
	{
		public void TestUS_TestingStatusChanged()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "TEPO";
			org.OH_RL_NKClosestPort = port.RL_Code;
			Declaration.JE_OH_Importer = org.PK;

			var inspection2 = Header.Inspections.AddNew();
			inspection2.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			AssertEquals("TEPO", inspection2.US_Location);

			Declaration.US_SchDArrival = "1101";
			var inspection3 = Header.Inspections.AddNew();
			inspection3.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			AssertEquals("1101", inspection3.US_Location);
		}

		public void TestIAPHISInspectionMembers()
		{
			var inspection = Header.Inspections.AddNew();
			inspection.US_Date = new ZDateTime(2015, 6, 1);
			inspection.US_Location = "L1";
			inspection.US_TestingStatus = "P";

			IAPHISInspection iInspection = inspection;
			AssertEquals("InspectionDate", new ZDate(2015, 6, 1), iInspection.InspectionDate);
			AssertEquals("InspectionLocation", "L1", iInspection.InspectionLocation);
			AssertEquals("InspectionLocationQualifier", "3", iInspection.InspectionLocationQualifier);
			AssertEquals("InspectionTestingStatus", "P", iInspection.InspectionTestingStatus);

			var inspection2 = Header.Inspections.AddNew();
			inspection2.US_Date = new ZDateTime(2017, 7, 7);
			inspection2.US_Location = "3706";
			inspection2.US_TestingStatus = "A";
			IAPHISInspection iInspection2 = inspection2;
			AssertEquals("InspectionDate", new ZDate(2017, 7, 7), iInspection2.InspectionDate);
			AssertEquals("InspectionLocation", "3706", iInspection2.InspectionLocation);
			AssertEquals("InspectionLocationQualifier", "2", iInspection2.InspectionLocationQualifier);
			AssertEquals("InspectionTestingStatus", "A", iInspection2.InspectionTestingStatus);
		}

		public void TestStatus()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();

			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			AssertEquals(1, aphisHeader.Inspections.Count);

			var inspection = Header.Inspections[0];
			AssertEquals("", inspection.US_TestingStatus);
			AssertEquals("", inspection.US_Location);

			inspection.US_Location = ZString.Empty;
			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			AssertEquals(ZString.Empty, inspection.US_Location);
			AssertEquals(InspectionStatusList.Codes.PreviouslyPerformed, inspection.US_TestingStatus);
		}

		public void TestPortType()
		{
			var inspection = Header.Inspections.AddNew();

			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			AssertEquals(InspectionLocationCodeList.Descriptions.UNLOCO, inspection.US_PortTypeDesc);

			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			AssertEquals(InspectionLocationCodeList.Descriptions.ScheduleD, inspection.US_PortTypeDesc);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			var inspection = aphisHeader.Inspections.AddNew();
			inspection.US_Location = "1";
			return inspection;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Inspections.AddNew();
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
