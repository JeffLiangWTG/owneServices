using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIILine))]
	public class AIILineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<AIILine>
	{
		public void TestDefault()
		{
			AIILine line = Factory.New<AIILine>();
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, line.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USAIILine, line.B7_Type);
			AssertEquals("Default: US_BasisUnit", 1, line.US_UnitBasis);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var lineGroup = invoiceLine.LineGroupingRanges.AddNew();
			lineGroup.US_StartSequenceNo = 1;
			lineGroup.US_EndSequenceNo = 1;
			var aiiLine = invoiceLine.AIILines.AddNew(lineGroup);
			ICusCodeDataTypeSupporter supporter = aiiLine;
			supporter.AssertType(typeof(RegoNumber), CusCodeDataTypeList.Codes.RegoNumber);
			supporter.AssertType(null, "ZZ!");

			var number = aiiLine.RegoNumbers.AddNew();
			number.CY_Data = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(number.PK);
			AssertEquals(typeof(RegoNumber), codeData.GetType());
		}

		public void TestUS_ArticleNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_IsInvoiceByRequest = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var range = invoiceLine.LineGroupingRanges.AddNew(1, 2);
			var aiiLine = invoiceLine.AIILines.AddNew(range);

			var aValue1 = "PRODUCT NO 1";
			var aValue2 = "PRODUCT NO 11";
			var bValue1 = "PRODUCT NO 2";
			var bValue2 = "PRODUCT NO 22";

			invoiceLine.US_ArticleNoA = aValue1;
			invoiceLine.US_ArticleNoB = bValue1;

			AssertEquals("Default value of US_ArticleNoA in AII line must be equal to US_ArticleNoA in the parent Invoice Line", invoiceLine.US_ArticleNoA, aiiLine.US_ArticleNoA);
			AssertEquals("Default value of US_ArticleNoB in AII line must be equal to US_ArticleNoB in the parent Invoice Line", invoiceLine.US_ArticleNoB, aiiLine.US_ArticleNoB);

			aiiLine.US_ArticleNoA = aValue2;
			aiiLine.US_ArticleNoB = bValue2;

			AssertEquals("AII line does not store US_ArticleNoA value", aValue2, aiiLine.US_ArticleNoA);
			AssertEquals("AII line does not store US_ArticleNoB value", bValue2, aiiLine.US_ArticleNoB);
		}

		public void TestExtendedDescriptionNoteIsDeletedOnSavingIfNotValid()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_IsInvoiceByRequest = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			InvoiceLineGroupingRange range = invoiceLine.LineGroupingRanges.AddNew(1, 2);
			AIILine aiiLine = invoiceLine.AIILines.AddNew(range);
			aiiLine.US_Desc = "HELLO WORLD";
			AssertEquals("No Extended Note", 0, aiiLine.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description).Length);
			aiiLine.US_ExtendedDesc = "GOODBYE WORLD";
			StmNote[] notes = aiiLine.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description);
			AssertEquals("Extended Notes", 1, notes.Length);
			StmNote extendedDescriptionNote = notes[0];
			AssertEquals(false, extendedDescriptionNote.IsDeleted);
			AssertEquals("GOODBYE WORLD", extendedDescriptionNote.ST_NoteText);
			AssertEquals("Is Extended Description enabled", true, aiiLine.US_IsExtCommDescEnabled);

			aiiLine.US_ExtendedDesc = "";
			AssertEquals(true, extendedDescriptionNote.IsDeleted);
			AssertEquals("Is Extended Description enabled", false, aiiLine.US_IsExtCommDescEnabled);

			aiiLine.US_ExtendedDesc = "GOODBYE WORLD";
			notes = aiiLine.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description);
			AssertEquals("Extended Notes", 1, notes.Length);
			extendedDescriptionNote = notes[0];
			AssertEquals(false, extendedDescriptionNote.IsDeleted);
			AssertEquals("GOODBYE WORLD", extendedDescriptionNote.ST_NoteText);
			AssertEquals("Is Extended Description enabled", true, aiiLine.US_IsExtCommDescEnabled);

			aiiLine.US_Desc = "";
			AssertEquals(true, extendedDescriptionNote.IsDeleted);
			AssertEquals("Is Extended Description enabled", false, aiiLine.US_IsExtCommDescEnabled);
		}

		public void TestDescriptionOn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_IsInvoiceByRequest = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = false;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AIILine aiiLine = invoiceLine.FirstAIILine;
			AssertEquals(ZString.Empty, aiiLine.US_Desc);
			AssertEquals(ZString.Empty, aiiLine.US_ExtendedDesc);
			invoiceLine.JI_Description = "HELLO WORLD";
			invoiceLine.JI_ExtraInfoForClassification = "GOODBYE WORLD";
			AssertEquals("HELLO WORLD", aiiLine.US_Desc);
			AssertEquals("GOODBYE WORLD", aiiLine.US_ExtendedDesc);
			aiiLine.US_Desc = ZString.Empty;
			AssertEquals(ZString.Empty, aiiLine.US_ExtendedDesc);
			AssertEquals(ZString.Empty, invoiceLine.JI_Description);
			AssertEquals(ZString.Empty, invoiceLine.JI_ExtraInfoForClassification);
			aiiLine.US_Desc = "HELLO BOB";
			aiiLine.US_ExtendedDesc = "BYE BOB";
			AssertEquals("HELLO BOB", invoiceLine.JI_Description);
			AssertEquals("BYE BOB", invoiceLine.JI_ExtraInfoForClassification);
			invoice.US_IsLineGrouping = true;
			invoiceLine.JI_Description = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals(ZString.Empty, aiiLine.US_Desc);
			AssertEquals(ZString.Empty, aiiLine.US_ExtendedDesc);
			aiiLine.US_Desc = "HI WENDY";
			aiiLine.US_ExtendedDesc = "BYE WENDY";
			AssertEquals("HI WENDY", invoiceLine.JI_Description);
			AssertEquals("BYE WENDY", invoiceLine.JI_ExtraInfoForClassification);
			invoiceLine.FirstGroupingRange.US_StartSequenceNo = 1;
			invoiceLine.FirstGroupingRange.US_EndSequenceNo = 2;
			invoiceLine.AIILines.AddNew();
			AssertEquals(2, invoiceLine.AIILines.Count);
			aiiLine = invoiceLine.AIILines[0];
			AIILine aiiLine1 = invoiceLine.AIILines[1];
			AssertEquals("HI WENDY", invoiceLine.JI_Description);
			AssertEquals("BYE WENDY", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals("HI WENDY", aiiLine.US_Desc);
			AssertEquals("BYE WENDY", aiiLine.US_ExtendedDesc);
			AssertEquals("HI WENDY", aiiLine1.US_Desc);
			AssertEquals("BYE WENDY", aiiLine1.US_ExtendedDesc);

			aiiLine.US_Desc = "HI JACK";
			AssertEquals("HI WENDY", invoiceLine.JI_Description);
			AssertEquals("BYE WENDY", invoiceLine.JI_ExtraInfoForClassification);
			AssertEquals("HI JACK", aiiLine.US_Desc);
			AssertEquals("", aiiLine.US_ExtendedDesc);
			AssertEquals("HI WENDY", aiiLine1.US_Desc);
			AssertEquals("BYE WENDY", aiiLine1.US_ExtendedDesc);
		}

		[TestDate(2008, 1, 1)]
		public void TestCustomsUnitFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_GenAIIForSup = true;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99010052"; // 5.99 cents/litre extra
			invoiceLine.JI_Tariff = "2909191800";   // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals("L", invoiceLine.US_SupUQ1);
			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);

			AIILine supLine = invoiceLine.AIILines.AddNew();
			supLine.US_SupLine = true;
			AssertEquals("99010052", supLine.US_Tariff);
			AssertEquals("L", supLine.US_CustomsQtyUQ);

			AIILine line = invoiceLine.AIILines.AddNew();
			line.US_SupLine = false;
			AssertEquals("2909.19.1800", line.US_Tariff);
			AssertEquals("KG", line.US_CustomsQtyUQ);
		}

		public void TestChangingParentTypeIsNotSupported()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AIILine aiiLine = Factory.New<AIILine>();
			AssertNoExceptionThrown(delegate
			{ aiiLine.B7_ParentTableCode = invoiceLine.TablePrefix; });
			AssertExceptionThrown(typeof(NotSupportedException), "Setting AIILine.B7_ParentTableCode is not supported.", delegate
			{ aiiLine.B7_ParentTableCode = invoice.TablePrefix; });

			AssertNoExceptionThrown(delegate
			{ aiiLine.B7_ParentID = invoiceLine.PK; });
			AssertExceptionThrown(typeof(NotSupportedException), "Setting AIILine.B7_ParentID is not supported.", delegate
			{ aiiLine.B7_ParentID = invoice.PK; });
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aiiLine = invoiceLine.AIILines.AddNew(invoiceLine.FirstGroupingRange);
			return aiiLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AIILine aiiLine = invoiceLine.AIILines.AddNew(invoiceLine.FirstGroupingRange); // making sure that it's not the first AII Line
			return invoiceLine.AIILines.AddNew();
		}
	}
}
