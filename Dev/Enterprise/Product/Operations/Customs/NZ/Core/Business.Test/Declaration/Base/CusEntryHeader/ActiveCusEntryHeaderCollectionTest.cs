using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.Types;
	using NUnit.Framework;

	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	public class ActiveCusEntryHeaderCollectionTest1 : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new ActiveCusEntryHeaderCollection(declaration);
		}

		public override void TestAreAllEntriesCleared()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();

			var cusCodeHelper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);

			var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
			var cusCodeList = cusCodeHelper.CreateNewOrGetExistingCusCodeList(declaration.CountryCode, "CSTA", "TST", "TST DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var cusCodeListAttribute = cusCodeHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			newFactory.Save();

			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			entry1 = Factory.Load<CusEntryHeader>(entry1.PK);
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = "XXX";
			Factory.Save();
			AssertEquals(false, declaration.ActiveEntryHeaders.AreAllEntriesCleared);

			entry1.CH_EntryStatus = "TST";
			Factory.Save();
			AssertEquals(true, declaration.ActiveEntryHeaders.AreAllEntriesCleared);
		}
	}
}
