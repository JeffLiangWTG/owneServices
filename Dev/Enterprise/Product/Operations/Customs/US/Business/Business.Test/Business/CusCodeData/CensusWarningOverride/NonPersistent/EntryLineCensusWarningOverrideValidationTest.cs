using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryLineCensusWarningOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryLinePK()
		{
			CensusWarningOverride.EntryLinePK = ZGuid.Empty;
			AssertHasError(CensusWarningOverride.EntryLinePKInfo, EntryCensusWarningOverrideValidation.EntryLineShouldBeSelected);
			CensusWarningOverride.EntryLinePK = Entry.MergedLines[0].PK;
			AssertNoError(CensusWarningOverride.EntryLinePKInfo, EntryCensusWarningOverrideValidation.EntryLineShouldBeSelected);
			var another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			AssertHasMessageError(another.EntryLinePKInfo, EntryCensusWarningOverrideValidation.MoreThan7ConditionCodes);
			another.EntryLinePK = ZGuid.Empty;
			AssertNoMessageError(another.EntryLinePKInfo, EntryCensusWarningOverrideValidation.MoreThan7ConditionCodes);
		}

		public void TestCheckConditionCode()
		{
			CensusWarningOverride.ConditionCode = ZString.Empty;
			AssertHasMessageErrors(CensusWarningOverride.ConditionCodeInfo);
			CensusWarningOverride.ConditionCode = "~~";
			AssertHasMessageErrors(CensusWarningOverride.ConditionCodeInfo);
			CensusWarningOverride.ConditionCode = CensusWarningCodeList.Codes.ChargesDividedByValue;
			AssertNoMessageErrors(CensusWarningOverride.ConditionCodeInfo);
			CensusWarningOverride.ConditionCode = CensusWarningCodeList.Codes.GrossWeightAir;
			CensusWarningOverride.EntryLinePK = Entry.MergedLines[0].PK;
			var another = Collection.AddNew();
			another.EntryLinePK = Entry.MergedLines[0].PK;
			another.ConditionCode = CensusWarningCodeList.Codes.GrossWeightAir;
			AssertHasMessageError(another.ConditionCodeInfo, EntryCensusWarningOverrideValidation.DuplicateCensusWarningCodeNowAllowed);
			another.ConditionCode = CensusWarningCodeList.Codes.GrossWeightVessel;
			AssertNoMessageError(another.ConditionCodeInfo, EntryCensusWarningOverrideValidation.DuplicateCensusWarningCodeNowAllowed);
		}

		public void TestCheckOverrideCode()
		{
			CensusWarningOverride.OverrideCode = ZString.Empty;
			AssertHasMessageErrors(CensusWarningOverride.OverrideCodeInfo);
			CensusWarningOverride.OverrideCode = "~~";
			AssertHasMessageErrors(CensusWarningOverride.OverrideCodeInfo);
			CensusWarningOverride.OverrideCode = CensusOverrideCodeList.Codes._01;
			AssertNoMessageErrors(CensusWarningOverride.OverrideCodeInfo);
		}

		EntryCensusWarningOverride censusWarningOverride;
		EntryCensusWarningOverride CensusWarningOverride => censusWarningOverride ?? (censusWarningOverride = Collection.AddNew());

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableENS = true;
					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				}

				return entry;
			}
		}

		EntryCensusWarningOverrideCollection collection;
		EntryCensusWarningOverrideCollection Collection => collection ?? (collection = new EntryCensusWarningOverrideCollection(Entry));
	}
}
