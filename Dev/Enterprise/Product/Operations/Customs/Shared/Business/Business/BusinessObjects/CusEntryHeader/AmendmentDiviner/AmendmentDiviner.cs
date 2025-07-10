/** TODO: ToBeCleaned : Obsoleted Code #Victor 20160210 Dead Code Reported
using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business
{
	public abstract class AmendmentDiviner
	{
		public AmendmentDiviner(CusEntryHeader OriginalHeader, CusEntryHeader ModifiedHeader)
		{
			if (OriginalHeader == null)
			{
				throw new ArgumentNullException("OriginalHeader");
			}
			if (ModifiedHeader == null)
			{
				throw new ArgumentNullException("ModifiedHeader");
			}
			this.OriginalHeader = OriginalHeader;
			this.ModifiedHeader = ModifiedHeader;
		}

		public bool DivineIfAnAmendmentIsRequired()
		{
			return !DoMessagesForBothEntriesMatch(OriginalHeader, ModifiedHeader);
		}

		public CusEntryLineAmendment[] DivineLineAmendments()
		{
			ArrayList Result = new ArrayList();

			foreach (CusEntryLine Line in ModifiedHeader.MergedLines)
			{
				CusEntryLine OriginalVersionOfLine = (CusEntryLine) OriginalHeader.MergedLines.FindByLineNumber(Line.CL_LineNumber);
				if (OriginalVersionOfLine == null)
				{
					Result.Add(GetNewAmendment(OriginalVersionOfLine, Line)); // new line
				}
				else if (!DoMessagesForBothLinesMatch(OriginalVersionOfLine, Line))
				{
					Result.Add(GetNewAmendment(OriginalVersionOfLine, Line)); // changed something on the line
				}
			}

			foreach (CusEntryLine OriginalLine in OriginalHeader.MergedLines)
			{
				if (ModifiedHeader.MergedLines.FindByLineNumber(OriginalLine.CL_LineNumber) == null) // deleted line
				{
					Result.Add(GetNewAmendment(OriginalLine, null));
				}
			}

			return (CusEntryLineAmendment[]) Result.ToArray(GetCusEntryLineAmendmentType());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		protected virtual Type GetCusEntryLineAmendmentType()
		{
			return typeof(CusEntryLineAmendment);
		}

		protected virtual CusEntryLineAmendment GetNewAmendment(ICusEntryLine OriginalLine, ICusEntryLine ModifiedLine)
		{
			return new CusEntryLineAmendment(OriginalLine, ModifiedLine);
		}

		protected abstract bool DoMessagesForBothEntriesMatch(CusEntryHeader OriginalHeader, CusEntryHeader ModifiedHeader);
		protected abstract bool DoMessagesForBothLinesMatch(ICusEntryLine OriginalLine, ICusEntryLine ModifiedLine);

		protected readonly CusEntryHeader OriginalHeader;
		protected readonly CusEntryHeader ModifiedHeader;

		#region Test
#if DEBUG

		internal class AmendementDivinerTest : TestCaseWithFactory
		{
			public void TestDivineIfAnAmendmentIsRequired()
			{
				DummyDiviner Diviner = new DummyDiviner(CreateBasicEntry(), CreateBasicEntry());
				
				Diviner.EntriesMatch = true;
				Assert("Should not require amendment", !Diviner.DivineIfAnAmendmentIsRequired());

				Diviner.EntriesMatch = false;
				Assert("Should require amendment", Diviner.DivineIfAnAmendmentIsRequired());
			}

			public void TestDivineLineAmendmentsWhenNotSubmittedToCustoms()
			{
				SetupBasicHeadersWith3MergedLinesAndDiviner();

				CusEntryLineAmendment[] Amendments = Diviner.DivineLineAmendments();
				AssertEquals("Number of amendments", 2, Amendments.Length);

				AssertNotNull("Should be an edit due to different message", Amendments[0].OriginalLine);
				AssertNotNull("Should be an edit due to different message", Amendments[0].ModifiedLine);
				AssertNotNull("Should be the new & delete combined into an edit", Amendments[1].OriginalLine);
				AssertNotNull("Should be the new & delete combined into an edit", Amendments[1].ModifiedLine);
			}

			public void TestDivineLineAmendmentsWhenSubmittedToCustoms()
			{

				SetupBasicHeadersWith3MergedLinesAndDiviner();

				// pretend we had submitted this job to customs so can't re-use line number 3
				Header2.MergedLines[2].CL_LineNumber = 4;

				CusEntryLineAmendment[] Amendments = Diviner.DivineLineAmendments();
				AssertEquals("Number of amendments", 3, Amendments.Length);

//				AssertEquals("Should be an edit due to different message", new ZDecimal(1000), Amendments[0].CL_CustomsValue);
//				AssertEquals("Should be the new line", new ZDecimal(3000), Amendments[1].CL_CustomsValue);
//				AssertEquals("Should be the deleted line", new ZDecimal(0), Amendments[2].CL_CustomsValue);
//
				// Modification
				AssertEquals("Original Line 1", Header1.MergedLines[0], Amendments[0].OriginalLine);
				AssertEquals("Amended Line 1", Header2.MergedLines[0], Amendments[0].ModifiedLine);

				// Insert
				AssertNull("Amended Line 2", Amendments[1].OriginalLine);
				AssertEquals("Original Line 2", Header2.MergedLines[2], Amendments[1].ModifiedLine);

				// Delete
				AssertEquals("Amended Line 3", Header1.MergedLines[2], Amendments[2].OriginalLine);
				AssertNull("Original Line 3", Amendments[2].ModifiedLine);
			}

			BaseJobDeclaration Dec1;
			BaseJobDeclaration Dec2;
			BaseJobComInvoiceLine ToBeDeletedLine;
			BaseJobComInvoiceLine NewLine;
			CusEntryHeader Header1;
			CusEntryHeader Header2;
			DummyDiviner Diviner;

			protected void SetupBasicHeadersWith3MergedLinesAndDiviner()
			{
				Dec1 = CreateBasicDeclaration();
				ToBeDeletedLine = Dec1.FilteredInvoiceLines.AddNew();
				ToBeDeletedLine.JI_Tariff = "1.432.12.1";

				AssertEquals("ToBeDeletedLine line is at index 2", ToBeDeletedLine, Dec1.FilteredInvoiceLines[2]);

				Dec2 = (BaseJobDeclaration) Dec1.TemplateCopy();
				Dec2.FilteredInvoiceLines[2].Delete();
				NewLine = Dec2.FilteredInvoiceLines.AddNew();
				NewLine.JI_Tariff = "2.32.12.1";

				Header1 = DoMergeAndGetHeader(Dec1);
				Header2 = DoMergeAndGetHeader(Dec2);

				AssertEquals("Should be 3 merged lines", 3, Header1.MergedLines.Count);
				AssertEquals("Should be 3 merged lines", 3, Header2.MergedLines.Count);

				Header1.MergedLines[0].CL_CustomsValue = 100;
				Header1.MergedLines[1].CL_CustomsValue = 200; 
				Header1.MergedLines[2].CL_CustomsValue = 300; 

				Header2.MergedLines[0].CL_CustomsValue = 1000;
				Header2.MergedLines[1].CL_CustomsValue = 200;
				Header2.MergedLines[2].CL_CustomsValue = 3000;

				Diviner = new DummyDiviner(Header1, Header2);
			}


			class DummyDiviner : AmendmentDiviner
			{
				public DummyDiviner(CusEntryHeader OriginalHeader, CusEntryHeader ModifiedHeader) : base(OriginalHeader, ModifiedHeader)
				{
				}

				public bool EntriesMatch = true;
				protected override bool DoMessagesForBothEntriesMatch(CusEntryHeader OriginalHeader, CusEntryHeader ModifiedHeader)
				{
					return EntriesMatch;
				}

				protected override bool DoMessagesForBothLinesMatch(ICusEntryLine OriginalLine, ICusEntryLine ModifiedLine)
				{
					return OriginalLine.CL_CustomsValue == ModifiedLine.CL_CustomsValue;
				}
			}

			protected CusEntryHeader CreateBasicEntry()
			{ 
				return DoMergeAndGetHeader(CreateBasicDeclaration());
			}

			protected BaseJobDeclaration CreateBasicDeclaration()
			{
				BaseJobDeclaration Declaration = BaseJobDeclaration.New(Factory);
				BaseJobComInvoiceHeader InvoiceHeader = Declaration.Invoices.AddNew();
				BaseJobComInvoiceLine InvoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
				InvoiceLine1.JI_Tariff = "99.99.99";
				BaseJobComInvoiceLine InvoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
				InvoiceLine2.JI_Tariff = "0.303.2";
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				return Declaration;
			}

			protected CusEntryHeader DoMergeAndGetHeader(BaseJobDeclaration Declaration)
			{
				Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(true);
				Declaration.DoMerge();
				return Declaration.CustomsEntryHeaders[0];
			}
		}

#endif
		#endregion
	}
}

**/
