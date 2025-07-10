using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PSCReasonCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckParentTypeIndicator()
		{
			var pscCode = new PSCReasonCode(Entry, new PSCReasonCodeCollection(Entry));
			pscCode.ParentTypeIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(pscCode.ParentTypeIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			pscCode.ParentTypeIndicator = "~";
			AssertNoMessageErrorContaining(pscCode.ParentTypeIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(pscCode.ParentTypeIndicatorInfo, ListValidation.InvalidCodeMessageError);
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			AssertNoMessageErrorContaining(pscCode.ParentTypeIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckLineNumber()
		{
			var pscCode = new PSCReasonCode(Entry, new PSCReasonCodeCollection(Entry));
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode.LineNumber = ZString.Empty;
			AssertHasMessageErrorContaining(pscCode.LineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			pscCode.LineNumber = "002";
			AssertNoMessageErrorContaining(pscCode.LineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(pscCode.LineNumberInfo, PSCReasonCodeValidation.NoEntryLineExistsWithLineNumber);
			pscCode.LineNumber = "001";
			AssertNoMessageError(pscCode.LineNumberInfo, PSCReasonCodeValidation.NoEntryLineExistsWithLineNumber);
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode.LineNumber = ZString.Empty;
			AssertNoMessageErrorContaining(pscCode.LineNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckReason()
		{
			var pscCode = new PSCReasonCode(Entry, new PSCReasonCodeCollection(Entry));
			pscCode.Reason1 = ZString.Empty;
			AssertHasMessageErrorContaining(pscCode.Reason1Info, PSCReasonCodeValidation.NoReasonCodeEntered);
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode.Reason1 = PSCHeaderReasonList.Codes.H04;
			pscCode.Reason2 = PSCHeaderReasonList.Codes.H04;
			pscCode.Reason3 = PSCHeaderReasonList.Codes.H04;
			pscCode.Reason4 = PSCHeaderReasonList.Codes.H04;
			pscCode.Reason5 = PSCHeaderReasonList.Codes.H04;
			AssertNoMessageErrorContaining(pscCode.Reason1Info, PSCReasonCodeValidation.NoReasonCodeEntered);
			AssertHasMessageErrorContaining(pscCode.Reason1Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pscCode.Reason2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pscCode.Reason3Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pscCode.Reason4Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pscCode.Reason5Info, ListValidation.InvalidCodeMessageError);
			pscCode.Reason1 = PSCLineReasonList.Codes.L01;
			pscCode.Reason2 = PSCLineReasonList.Codes.L01;
			pscCode.Reason3 = PSCLineReasonList.Codes.L01;
			pscCode.Reason4 = PSCLineReasonList.Codes.L01;
			pscCode.Reason5 = PSCLineReasonList.Codes.L01;
			AssertNoMessageErrorContaining(pscCode.Reason1Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pscCode.Reason2Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pscCode.Reason3Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pscCode.Reason4Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pscCode.Reason5Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckParentTypeIsDuplicated()
		{
			var collection = new PSCReasonCodeCollection(Entry);
			var pscCode1 = collection.AddNew();
			var pscCode2 = collection.AddNew();
			pscCode2.LineNumber = "1";
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			AssertNoMessageError(pscCode1.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
			AssertNoMessageError(pscCode2.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			AssertHasMessageError(pscCode2.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode2.LineNumber = "1";
			pscCode1.LineNumber = "1";
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			AssertHasMessageError(pscCode1.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
		}

		public void TestCheckParentTypeAndLineNumberIsDuplicated()
		{
			var collection = new PSCReasonCodeCollection(Entry);
			var pscCode1 = collection.AddNew();
			var pscCode2 = collection.AddNew();
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode1.LineNumber = "1";
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode2.LineNumber = "2";
			AssertNoMessageError(pscCode2.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
			pscCode2.LineNumber = "1";
			AssertHasMessageError(pscCode2.LineNumberInfo, PSCReasonCodeValidation.DuplicatedParentType);
		}

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
