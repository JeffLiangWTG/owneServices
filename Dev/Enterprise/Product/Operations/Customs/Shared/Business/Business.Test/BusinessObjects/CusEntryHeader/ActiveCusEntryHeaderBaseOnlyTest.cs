using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ActiveCusEntryHeaderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestDeactivateAll()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry1 = declaration.ActiveEntryHeaders.AddNew();
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			declaration.ActiveEntryHeaders.DeactivateAll();
			AssertEquals(0, declaration.ActiveEntryHeaders.Count);
		}

		public void TestHasAnEntryWithEntryNumber()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.ActiveEntryHeaders.HasAnEntryWithEntryNumber);
			entry1.EntryNumber = "1";
			AssertEquals(true, declaration.ActiveEntryHeaders.HasAnEntryWithEntryNumber);
		}

		public void TestHasEntriesNonAmendableChanges()
		{
			var declaration = Factory.NewMoq<BaseJobDeclaration>();
			var entry = Factory.NewMoq<CusEntryHeader>();
			declaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);

			declaration.Object.CustomsEntryHeaders.Add(entry.Object);
			AssertEquals(false, declaration.Object.ActiveEntryHeaders.HasEntriesWithNonAmendableChanges);

			entry.Object.EntryNumber = "1";
			entry.Protected().Setup<bool>("HasNonAmendableChangesCore").Returns(true);
			entry.Setup(m => m.HasBeenWithdrawn).Returns(false);
			AssertEquals(true, declaration.Object.ActiveEntryHeaders.HasEntriesWithNonAmendableChanges);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Thailand))
			{
				var declarationTH = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var entryTH = declarationTH.CustomsEntryHeaders.AddNew();
				entryTH.CH_JE = declarationTH.PK;
				Assert("Integrated declaration IsCustomsHeaderAmendmentATotalReplacement should return false without exception", !declarationTH.ActiveEntryHeaders.HasEntriesWithNonAmendableChanges);
			}
		}

		public void TestIsThisPartOfCollection()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			var mockEntry1 = Factory.NewMoq<CusEntryHeader>();
			var mockEntry2 = Factory.NewMoq<CusEntryHeader>();

			CusEntryHeader entry1 = mockEntry1.Object;
			declaration.CustomsEntryHeaders.Add(entry1);
			CusEntryHeader entry2 = mockEntry2.Object;
			declaration.CustomsEntryHeaders.Add(entry2);

			mockEntry1.Setup(m => m.IsActive).Returns(true);
			mockEntry2.Setup(m => m.IsActive).Returns(false);

			ActiveCusEntryHeaderCollection coll = new ActiveCusEntryHeaderCollection(declaration);
			AssertEquals("coll should have entry1", true, coll.Contains(entry1));
			AssertEquals("coll should not have entry2", false, coll.Contains(entry2));
		}

		public void TestBGMReferencesAsCommaDelimitedString()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("", declaration.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString);

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "BGM456";

			AssertEquals("BGM456", declaration.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString);

			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BGM123";

			declaration.ActiveEntryHeaders.Sort(CusEntryHeader.Schema.CH_BGMReference);
			AssertEquals("BGM123, BGM456", declaration.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString);
		}
	}
}
