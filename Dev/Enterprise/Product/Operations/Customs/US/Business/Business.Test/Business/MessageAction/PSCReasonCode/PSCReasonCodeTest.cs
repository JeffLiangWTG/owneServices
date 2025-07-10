using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCReasonCode))]
	sealed class PSCReasonCodeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentTypeIndicator()
		{
			var pscReasonCode = new PSCReasonCode(Entry, Collection);
			Assert(pscReasonCode.LineNumberInfo.ReadOnly);
			pscReasonCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			Assert(!pscReasonCode.LineNumberInfo.ReadOnly);
			pscReasonCode.LineNumber = "2";
			pscReasonCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			Assert(pscReasonCode.LineNumberInfo.ReadOnly);
			AssertEquals("LineNumber is cleared", ZString.Empty, pscReasonCode.LineNumber);
		}

		public void TestGetReasonCodes()
		{
			var pscReasonCode = new PSCReasonCode(Entry, Collection);
			pscReasonCode.Reason1 = PSCHeaderReasonList.Codes.H01;
			pscReasonCode.Reason2 = PSCHeaderReasonList.Codes.H04;
			AssertEquals("ReasonCodes. Should not return if empty", 2, pscReasonCode.GetReasonCodes().Count());
		}

		public void TestCopyPSCReasonCodes()
		{
			PSCReasonCode pscReasonCode = new PSCReasonCode(Entry, Collection);
			pscReasonCode.Reason1 = PSCHeaderReasonList.Codes.H01;
			pscReasonCode.Reason2 = PSCHeaderReasonList.Codes.H04;
			pscReasonCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscReasonCode.CopyPSCReasonCodes();
			Factory.Save();
			var cusCodeData = new PSCReasonCusCodeData.Loader(Factory).Load(entry);
			AssertNotNull("Data should be saved", cusCodeData);
			AssertEquals("PSC Reason Codes", PSCHeaderReasonList.Codes.H01 + "," + PSCHeaderReasonList.Codes.H04, cusCodeData.CY_Data);
			CusEntryLine entryLine = Entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			PSCReasonCode pscReasonCode2 = new PSCReasonCode(Entry, Collection);
			pscReasonCode2.Reason2 = PSCHeaderReasonList.Codes.H02;
			pscReasonCode2.Reason5 = PSCHeaderReasonList.Codes.H05;
			pscReasonCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscReasonCode2.LineNumber = "2";
			AssertEquals("Should have been formatted", "002", pscReasonCode2.LineNumber);
			pscReasonCode2.CopyPSCReasonCodes();
			Factory.Save();
			cusCodeData = new PSCReasonCusCodeData.Loader(Factory).Load(entryLine);
			AssertNotNull("Data should be saved", cusCodeData);
			AssertEquals("PSC Reason Codes", PSCHeaderReasonList.Codes.H02 + "," + PSCHeaderReasonList.Codes.H05, cusCodeData.CY_Data);
		}

		protected override BusinessObject GetNewBusinessObject() => new PSCReasonCode(Entry, Collection);

		PSCReasonCodeCollection collection;
		PSCReasonCodeCollection Collection => collection ?? (collection = new PSCReasonCodeCollection(Entry));

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				}

				return entry;
			}
		}
	}
}
