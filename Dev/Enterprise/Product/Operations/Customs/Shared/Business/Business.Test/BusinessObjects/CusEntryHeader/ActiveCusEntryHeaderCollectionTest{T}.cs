using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class ActiveCusEntryHeaderCollectionTest<T> : BusinessObjectCollectionViewTestCase<T> where T : ActiveCusEntryHeaderCollection
	{
		#region Implementation

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusEntryHeader>();
		}

		#endregion

		public virtual void TestAreAllEntriesCleared()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();

			var cusCodeHelper = new UniversalReferenceTestDataHelper(newFactory);

			var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
			var cusCodeList = cusCodeHelper.CreateNewOrGetExistingCusCodeList(declaration.CountryCode, "CSTA", "TST", "TST DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var cusCodeListAttribute = cusCodeHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "Ref1";
			newFactory.Save();

			declaration = Factory.Load<BaseJobDeclaration>(declaration.PK);
			entry1 = Factory.Load<CusEntryHeader>(entry1.PK);
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = "XXX";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = "TST";
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "Ref2";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry2.CH_EntryStatus = "XXX";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry2.CH_EntryStatus = "TST";
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
		}
	}
}
