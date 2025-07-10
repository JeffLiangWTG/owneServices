using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class CusEntryHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestFormalEntry()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry, typeof(FormalEntry.CusEntryHeader));
		}

		public void TestECIWriteOff()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOff, typeof(ECIWriteOff.CusEntryHeader));
		}

		public void TestECIWriteOffManifesting()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest, typeof(ECIWriteOff.Manifesting.CusEntryHeader));
		}

		public void TestCompletion()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.Completion, typeof(FormalEntry.CompletionCusEntryHeader));
		}

		public void TestPrimaryIndustries()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries, typeof(FormalEntry.PrimaryIndustriesCusEntryHeader));
		}

		public void TestOriginal()
		{
			SetupAndCheck(CusEntryHeader.EntryHeaderTypes.NZ.Original, typeof(FormalEntry.OriginalCusEntryHeader));
		}

		public void TestNZBase()
		{
			SetupAndCheck("", typeof(FormalEntry.CusEntryHeader));
		}

		void SetupAndCheck(string headerType, Type expectedType)
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_MessageType = headerType;

			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			CusEntryHeader loadedEntryHeader = secondFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals(expectedType, loadedEntryHeader.GetType());
		}
	}
}
