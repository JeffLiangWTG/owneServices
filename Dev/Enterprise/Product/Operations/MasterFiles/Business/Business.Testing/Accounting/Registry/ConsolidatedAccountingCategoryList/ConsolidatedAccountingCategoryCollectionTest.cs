using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsolidatedAccountingCategoryCollection))]
	public sealed class ConsolidatedAccountingCategoryCollectionTest : CodeDescriptionWithGroupCollectionAbstractTest<ConsolidatedAccountingCategoryCollection>
	{
		protected override ConsolidatedAccountingCategoryCollection GetCollectionToTest()
		{
			var result = new ConsolidatedAccountingCategoryCollection();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ConsolidatedAccountingCategoryItem();
		}

		public void TestGetGroupColumnCaption()
		{
			AssertEquals("Class", ConsolidatedAccountingCategoryCollection.GetGroupColumnCaption());
		}

		public void TestConveter()
		{
			var originalValue = new CodeDescriptionWithGroupCollection();
			originalValue.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			originalValue.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");
			AssertEquals("PreCondition", 2, originalValue.Count);

			var convertedValue = ConsolidatedAccountingCategoryCollection.Converter(originalValue);

			AssertEquals(originalValue.Count, convertedValue.Count);
			foreach (ConsolidatedAccountingCategoryItem itemValue in convertedValue)
			{
				AssertEquals(itemValue.Description, originalValue.FindByCode(itemValue.Code).Description);
			}
		}

		public void TestGetDefaultValue()
		{
			var defaultValue = ConsolidatedAccountingCategoryCollection.GetDefaultValue();
			foreach (ConsolidatedAccountingCategoryItem item in defaultValue)
			{
				AssertEquals(item.Code, item.OriginalCode);
			}

			var expectedDefaultValue = new CodeDescriptionWithGroupCollection(new ConsolidatedAccountingCategoryClassList(), ConsolidatedAccountingCategoryClassList.Codes.Intercompany);
			expectedDefaultValue.Add(Constants.AccountsCategory.Unrelated, (NoResString)"Unrelated company", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			expectedDefaultValue.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			expectedDefaultValue.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");
			expectedDefaultValue.Add(Constants.AccountsCategory.MinorityWithNoReporting, (NoResString)"Minority Interest with no reporting");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedMinorityShareholder, (NoResString)"Related minority shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedMajorityShareholder, (NoResString)"Related Majority Shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedWhollyOwningShareholder, (NoResString)"Related Wholly Owning Shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMinority, (NoResString)"Group Company Related Minority (no direct ownership)");
			expectedDefaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMajority, (NoResString)"Group Company Related Majority (no direct ownership)");

			AssertContainsExactElementsInAnyOrder(
				new AccountingMasterFilesRegistry.CodeDescriptionWithGroupEqualityComparer(),
				expectedDefaultValue.Cast<CodeDescriptionWithGroup>(),
				defaultValue.Cast<CodeDescriptionWithGroup>()
			);
		}

		public void TestGroupLookup()
		{
			var groupLookup1 = GetCollectionToTest().AddNew().GroupLookup;

			var groupLookup2 = ConsolidatedAccountingCategoryCollection.Converter(new CodeDescriptionWithGroupCollection()).AddNew().GroupLookup;

			var expectedGroupLookup = new CodeDescriptionPairList();
			expectedGroupLookup.AddPair("TPY", ConsolidatedAccountingCategoryClassList.Descriptions.ThirdParty);
			expectedGroupLookup.AddPair("INT", ConsolidatedAccountingCategoryClassList.Descriptions.Intercompany);

			AssertContainsExactElementsInAnyOrder(expectedGroupLookup.GetAllCodes(), groupLookup1.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(expectedGroupLookup.GetAllCodes(), groupLookup2.GetAllCodes());
		}

		public override void TestAddNew()
		{
			base.TestAddNew();

			var listValue = GetCollectionToTest();
			AssertType<ConsolidatedAccountingCategoryItem>(listValue.AddNew());
		}

		public override void TestDefaultGroupForNewChild()
		{
			var collection = GetCollectionToTest();

			AssertEquals("Collection.AddNew().Group", ConsolidatedAccountingCategoryClassList.Codes.Intercompany, collection.AddNew().Group);

			var clone = collection.Clone(null, Factory) as ConsolidatedAccountingCategoryCollection;

			AssertEquals("Collection.AddNew().Group", ConsolidatedAccountingCategoryClassList.Codes.Intercompany, clone.AddNew().Group);
		}

		public void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(ConsolidatedAccountingCategoryCollection));

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();
			var element3 = Collection.AddNew();
			var element4 = Collection.AddNew();

			element1.Code = "a";
			element2.Code = "b";
			element3.Code = "c";
			element4.Code = "d";

			element1.Description = (NoResString)"a Description";
			element2.Description = (NoResString)"b Description";
			element3.Description = (NoResString)"c Description";
			element4.Description = (NoResString)"d Description";

			element1.Group = "ABC";
			element2.Group = "CDE";
			element3.Group = "EFG";
			element4.Group = "ABC";

			element1.SystemDefined = false;
			element2.SystemDefined = false;
			element3.SystemDefined = true;
			element4.SystemDefined = true;

			var bytes = dataType.Serialise(Collection);
			var deserialisedCollection = (ConsolidatedAccountingCategoryCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 4, deserialisedCollection.Count);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].Group", "ABC", deserialisedCollection[0].Group);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].Group", "CDE", deserialisedCollection[1].Group);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);

			AssertEquals("[2].Code", "c", deserialisedCollection[2].Code);
			AssertEquals("[2].Description", "c Description", deserialisedCollection[2].Description);
			AssertEquals("[2].Group", "EFG", deserialisedCollection[2].Group);
			AssertEquals("[2].SystemDefined", true, deserialisedCollection[2].SystemDefined);

			AssertEquals("[3].Code", "d", deserialisedCollection[3].Code);
			AssertEquals("[3].Description", "d Description", deserialisedCollection[3].Description);
			AssertEquals("[3].Group", "ABC", deserialisedCollection[3].Group);
			AssertEquals("[3].SystemDefined", true, deserialisedCollection[3].SystemDefined);
		}

		public void TestOnUpdateAction()
		{
			var collection = GetCollectionToTest();
			collection.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			collection.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");
			AssertEquals("PreCondition", 2, collection.Count);
			foreach (ConsolidatedAccountingCategoryItem itemValue in collection)
			{
				AssertNullOrEmpty("PreCondition", itemValue.OriginalCode);
			}

			collection.OnUpdateAction();
			foreach (ConsolidatedAccountingCategoryItem itemValue in collection)
			{
				AssertEquals(itemValue.Code, itemValue.OriginalCode);
			}
		}
	}
}
