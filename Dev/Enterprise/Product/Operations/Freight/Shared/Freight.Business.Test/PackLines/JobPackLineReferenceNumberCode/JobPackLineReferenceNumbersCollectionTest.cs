using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobPackLineReferenceNumbersCollection))]
	sealed class JobPackLineReferenceNumbersCollectionTest : ActiveBusinessObjectCollectionTestCase<JobPackLineReferenceNumbersCollection>
	{
		public void TestDefaultValues()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new JobPackLineReferenceNumbersCollection(bizObj);
			var number = numbers.AddNew();
			AssertEquals(bizObj.PK, number.CE_ParentID);
			AssertEquals(AutoDummyBizo.Schema.TableName, number.CE_ParentTable);
			AssertEquals(bizObj, number.Parent);
			AssertEquals(number.CE_EntryIsSystemGenerated, false);
		}

		public void TestLoadAfterSave()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new JobPackLineReferenceNumbersCollection(bizObj);
			var number = numbers.AddNew();
			number.CE_EntryNum = "111";
			number.CE_ParentTable = "JobDeclaration";
			Factory.Save();

			var loadedBizObj = NewFactory().Load<DummyBusinessObject>(bizObj.PK);
			var loadedPortReferences = new JobPackLineReferenceNumbersCollection(loadedBizObj);
			var portReference = loadedPortReferences.Single();
			AssertEquals("Numbers contains previously saved number", "111", portReference.CE_EntryNum);
			AssertEquals("Parent retained", loadedBizObj, portReference.Parent);
		}

		protected override JobPackLineReferenceNumbersCollection GetCollectionToTest()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			return new JobPackLineReferenceNumbersCollection(bizObj);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var number = Factory.New<CusEntryNumber>();
			return number;
		}
	}
}
