using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsWrapperCollection))]
	[UseSnapshotProtection]
	sealed class CustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomsNumberViewStmNumsWrapperCollection>
	{
		public void TestLoadData()
		{
			var collection = new CustomsNumberViewStmNumsCollection(Provider);
			var stmNums1 = collection.AddNew();
			var stmNums2 = collection.AddNew();
			var wrapperCollection = new CustomsNumberViewStmNumsWrapperCollection(collection);
			AssertEquals("wrapperCollection.AllowNew", false, wrapperCollection.AllowNew);
			AssertEquals("wrapperCollection.AllowRemove", false, wrapperCollection.AllowRemove);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);

			var wrapper1 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums1.PK);
			AssertEquals("wrapper1.StmNums", stmNums1, wrapper1.StmNums);
			var wrapper2 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);

			var stmNums3 = collection.AddNew();
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			var wrapper3 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);

			collection.Delete(stmNums1);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			wrapper2 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);

			var stmNums4 = Provider.NewCustomsNumber();
			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			wrapper2 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var wrapper4 = (CustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums4.PK);
			AssertEquals("wrapper4.StmNums", stmNums4, wrapper4.StmNums);

			AssertExceptionThrown<NotImplementedException>(() => wrapperCollection.AddNew());
		}

		CustomsNumberViewStmNumsCompanyProviderForTest Provider => provider ?? (provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory));
		CustomsNumberViewStmNumsCompanyProviderForTest provider;

		protected override CustomsNumberViewStmNumsWrapperCollection GetCollectionToTest()
		{
			return Provider.CustomsNumberWrappers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = Provider.CustomsNumbers.AddNew();
			return Provider.GetOrCreateWrapper(stmNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			providerSetup = new CustomsNumberViewStmNumsCompanyProviderForTestSetUp();
		}
		IDisposable providerSetup;
		IDisposable countrySetter;

		protected override void TearDown()
		{
			if (providerSetup != null)
			{
				providerSetup.Dispose();
				providerSetup = null;
			}
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}
	}
}
