using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PSCReasonCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReasonCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var pscCode = new PSCReasonCode(entry, new PSCReasonCodeCollection(entry));
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			var reasonCodes = pscCode.Lookups.ReasonCodeList;
			AssertEquals(typeof(PSCHeaderReasonList), reasonCodes.GetType());
			Assert(reasonCodes.ContainsCode(PSCHeaderReasonList.Codes.H14));
			AssertContains("other than the types indicated by PSC Header Reason Code H01 or H14", reasonCodes.GetDescriptionFromCode(PSCHeaderReasonList.Codes.H02));
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			AssertEquals(typeof(PSCLineReasonList), pscCode.Lookups.ReasonCodeList.GetType());
		}
	}
}
