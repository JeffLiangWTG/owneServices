using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(OriginalCusEntryHeader))]
	public class OriginalCusEntryHeaderTest : CusEntryHeaderTest<OriginalCusEntryHeader>
	{
		public override void TestSetDefaultValues()
		{
			AssertEquals("EntryHeader.CH_MessageType", CusEntryHeader.EntryHeaderTypes.NZ.Original, EntryHeader.CH_MessageType);
			AssertEquals("EntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms, EntryHeader.CH_EntryStatus);
		}

		public void TestEntryNumberShowsAsUserEnteredAndIsOfTheRightType()
		{
			EntryHeader.EntryNumber = "48274930";
			CusEntryNumber entryNumber = EntryHeader.CusEntryNumber;
			AssertEquals("EntryNumber.CE_EntryIsSystemGenerated", false, entryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("EntryNumber.CE_EntryType", CusEntryNumberTypeList.Codes.OriginalEntry, entryNumber.CE_EntryType);
			AssertEquals("EntryNumber.CE_EntryNum", "48274930", entryNumber.CE_EntryNum);
		}

		#region Implementation
		protected override string ExpectedCusEntryNumType
		{
			get { return CusEntryNumberTypeList.Codes.OriginalEntry; }
		}

		protected override string ExpectedCusEntryHeaderType
		{
			get { return CusEntryHeader.EntryHeaderTypes.NZ.Original; }
		}

		protected new OriginalCusEntryHeader EntryHeader
		{
			get { return base.EntryHeader; }
		}

		protected override Declaration.CusEntryHeader GetNewCusEntryHeader()
		{
			Declaration.CusEntryHeader result = Declaration.CustomsEntryHeaders.AddNew(typeof(OriginalCusEntryHeader));
			result.EntryNumber = "89898989";
			return result;
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			return declaration;
		}
		#endregion
	}
}
