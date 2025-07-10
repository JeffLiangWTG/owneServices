using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(CusEntryNumReferenceCollection))]
	public class CusEntryNumReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<CusEntryNumReferenceCollection>
	{
		public void TestDefaultValues()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumReferenceCollection(bizObj);
			var number = numbers.AddNew();
			AssertEquals(CusEntryNumber.Categories.CustomsPermitClearanceNumber, number.CE_Category);
			AssertEquals(bizObj.PK, number.CE_ParentID);
			AssertEquals(DummyBusinessObject.Schema.TableName, number.CE_ParentTable);
			AssertEquals(bizObj, number.Parent);
		}

		public void TestLoadAfterSave()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumReferenceCollection(bizObj);
			var number = numbers.AddNew();
			number.CE_EntryNum = "111";
			number.CE_Category = "CUS";
			number.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			var loadedBizObj = NewFactory().Load<DummyBusinessObject>(bizObj.PK);
			var loadedCustomsReferences = new CusEntryNumReferenceCollection(loadedBizObj);
			var customsReference = loadedCustomsReferences.Single();
			AssertEquals($"Category is {TransitWarehouseReferenceCategories.Codes.CustomsReference}",
				TransitWarehouseReferenceCategories.Codes.CustomsReference, customsReference.CE_Category);
			AssertEquals("Numbers contains previously saved number", "111", customsReference.CE_EntryNum);
			AssertEquals("Parent retained", loadedBizObj, customsReference.Parent);
		}

		protected override CusEntryNumReferenceCollection GetCollectionToTest()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			return new CusEntryNumReferenceCollection(bizObj);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusEntryNumber>();
		}
	}
}
