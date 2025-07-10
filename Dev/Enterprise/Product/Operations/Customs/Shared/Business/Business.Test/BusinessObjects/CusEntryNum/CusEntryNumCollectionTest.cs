using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryNumCollection))]
	sealed class CusEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection()
		{
			CusEntryNumCollection collection = new CusEntryNumCollection(Factory);
			BaseJobDeclaration testJobDeclaration = BaseJobDeclaration.New(Factory);

			AddCusEntryNum(BaseJobDeclaration.Schema.TableName, testJobDeclaration.PK);

			collection.Load();
			AssertEquals("Collection Count", 0, collection.Count);

			ZQuery decFilter = new ZQuery(CusEntryNumSchema.CE_ParentTable, BaseJobDeclaration.Schema.TableName);
			decFilter.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, testJobDeclaration.PK);

			collection.Load(decFilter);
			AssertEquals("Collection Count", 1, collection.Count);

			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AddCusEntryNum(CusEntryHeader.Schema.TableName, entryHeader.PK);

			decFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, entryHeader.PK);
			collection.Load(decFilter);
			AssertEquals("Collection Count", 2, collection.Count);
		}

		void AddCusEntryNum(ZString tableName, ZGuid parentID)
		{
			CusEntryNumber newEntryNum = Factory.New<CusEntryNumber>();
			newEntryNum.CE_ParentTable = tableName;
			newEntryNum.CE_ParentID = parentID;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryNumCollection(Factory);
		}
	}
}
