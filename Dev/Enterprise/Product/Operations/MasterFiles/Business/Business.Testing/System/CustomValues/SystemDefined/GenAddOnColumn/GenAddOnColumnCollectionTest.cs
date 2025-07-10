using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenAddOnColumnCollection))]
	sealed class GenAddOnColumnCollectionTest : ActiveBusinessObjectCollectionTestCase<GenAddOnColumnCollection>
	{
		public void TestFind()
		{
			GenAddOnColumnCollection coll = GetCollectionToTest();
			GenAddOnColumn column1 = coll.AddNew();
			column1.XA_Name = "XXX";

			GenAddOnColumn column2 = coll.AddNew();
			column2.XA_Name = "XXY";

			AssertEquals(column2, coll.Find("XXY"));
			AssertEquals(column1, coll.Find("XXX"));
		}

		public void TestSetDeafultValuesForNewElement()
		{
			GenAddOnColumnCollection coll = GetCollectionToTest();
			GenAddOnColumn column = coll.AddNew();
			AssertEquals("ParentTableCode is set", Declaration.TablePrefix, column.XA_ParentTableCode);
			AssertEquals("ParentID is set", Declaration.PK, column.XA_ParentID);
		}

		public void TestLoadCorrectElements()
		{
			GenAddOnColumn column1 = Factory.New<GenAddOnColumn>();
			column1.XA_ParentID = Declaration.PK;
			column1.XA_ParentTableCode = Declaration.TablePrefix;

			BusinessObject declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			GenAddOnColumn column2 = Factory.New<GenAddOnColumn>();
			column2.XA_ParentID = declaration2.PK;
			column2.XA_ParentTableCode = declaration2.TablePrefix;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GenAddOnColumnCollection collForDec1 = new GenAddOnColumnCollection((BusinessObject)factory2.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(Declaration.PK));
			AssertNotNull("should contain column1", collForDec1.Find(new ZQuery(GenCustomAddOnValueSchema.PK, column1.PK)));

			GenAddOnColumnCollection collForDec2 = new GenAddOnColumnCollection((BusinessObject)factory2.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration2.PK));
			AssertNotNull("should contain column2", collForDec2.Find(new ZQuery(GenCustomAddOnValueSchema.PK, column2.PK)));
		}

		protected override GenAddOnColumnCollection GetCollectionToTest()
		{
			return new GenAddOnColumnCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GenAddOnColumn result = Factory.New<GenAddOnColumn>();
			result.XA_ParentID = Declaration.PK;
			result.XA_ParentTableCode = Declaration.TablePrefix;
			return result;
		}

		BusinessObject Declaration
		{
			get { return declaration ?? (declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>()); }
		}
		BusinessObject declaration;
	}
}
