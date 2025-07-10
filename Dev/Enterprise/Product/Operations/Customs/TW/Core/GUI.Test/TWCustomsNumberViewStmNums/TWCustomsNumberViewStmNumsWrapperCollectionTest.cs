using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TWCustomsNumberViewStmNumsWrapperCollection))]
	sealed class TWCustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TWCustomsNumberViewStmNumsWrapperCollection>
	{
		public void TestLoadData()
		{
			var collection = new CustomsNumberViewStmNumsCollection(Provider);
			var stmNums1 = collection.AddNew();
			stmNums1.SN_Type = "IMP";
			var stmNums2 = collection.AddNew();
			stmNums2.SN_Type = "IMP";
			var wrapperCollection = new TWCustomsNumberViewStmNumsWrapperCollection(collection);
			AssertEquals("wrapperCollection.AllowNew", false, wrapperCollection.AllowNew);
			AssertEquals("wrapperCollection.AllowRemove", false, wrapperCollection.AllowRemove);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			var wrapper1 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums1.PK);
			AssertEquals("wrapper1.StmNums", stmNums1, wrapper1.StmNums);
			var wrapper2 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			var stmNums3 = collection.AddNew();
			stmNums3.SN_Type = "IMP";
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			var wrapper3 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			collection.Delete(stmNums1);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			wrapper2 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var stmNums4 = Factory.New<CustomsNumberViewStmNums>();
			stmNums4.Provider = provider;
			stmNums4.SN_Owner = Company.PK;
			stmNums4.SN_Type = stmNums2.SN_Type;
			stmNums4.SN_FountainName = stmNums2.SN_FountainName;
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			wrapper2 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var wrapper4 = (TWCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums4.PK);
			AssertEquals("wrapper4.StmNums", stmNums4, wrapper4.StmNums);
			AssertExceptionThrown<NotImplementedException>(() => wrapperCollection.AddNew());
		}

		TWCustomsNumberViewStmNumsCompanyProvider Provider => provider ?? (provider = new TWCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK));
		TWCustomsNumberViewStmNumsCompanyProvider provider;
		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override TWCustomsNumberViewStmNumsWrapperCollection GetCollectionToTest()
		{
			return Provider.CustomsNumberWrappers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = Provider.CustomsNumbers.AddNew();
			return Provider.GetOrCreateWrapper(stmNum);
		}
	}
}
