using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierPartFetchStrategyTest : TestCaseWithFactory
	{
		#region TestFetchForLoad

		public void TestFetchForLoad_Count()
		{
			AssertEquals(0, Factory.ActiveFetchHintsForTable(OrgPartRelationSchema.Constants.TableName));
			var product = OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PartNum";
			OrgSupplierPartFetchStrategy strategy = new OrgSupplierPartFetchStrategy(product);
			strategy.FetchForLoad();
			AssertResults();
		}

		void AssertResults()
		{
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgPartRelationSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(CusClassPartPivotSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgPartUnitSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgPartBOMSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgSupplierPartBarcodeSchema.Constants.TableName));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(WhsPickFaceSchema.Constants.TableName));
		}

		#endregion

		#region TestFetchForFactorySave

		public void TestFetchForFactorySave()
		{
			AssertEquals(0, Factory.ActiveFetchHintsForTable(OrgPartRelationSchema.Constants.TableName));
			var product = OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PartNum";
			OrgSupplierPartFetchStrategy strategy = new OrgSupplierPartFetchStrategy(product);
			strategy.FetchForFactorySave();
			AssertResults();
		}

		#endregion

		#region TestCusClassPartPivotFetchHintForMultipleCountries

		[ExpectNoExceptions]
		public void TestCusClassPartPivotFetchHintForMultipleCountries()
		{
			var part = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>());
			part["OP_PartNum"] = part.PK.ToString().Replace("-", "");
			var nzPivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ICusClassPartPivot>());
			nzPivot["CI_OP"] = part.PK;
			var auPivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusClassPartPivot>());
			auPivot["CI_OP"] = part.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var partCopy = newFactory.Load(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>(), part.PK);
			var auPartCopy = newFactory.Load(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusClassPartPivot>(), auPivot.PK);
		}

		#endregion
	}
}
