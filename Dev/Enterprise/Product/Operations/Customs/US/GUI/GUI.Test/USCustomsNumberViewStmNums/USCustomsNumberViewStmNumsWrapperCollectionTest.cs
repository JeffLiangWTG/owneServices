using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCustomsNumberViewStmNumsWrapperCollection))]
	sealed class USCustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<USCustomsNumberViewStmNumsWrapperCollection>
	{
		public void TestLoadData()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			var collection = new CustomsNumberViewStmNumsCollection(Provider);
			var stmNums1 = collection.AddNew();
			stmNums1.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			var stmNums2 = collection.AddNew();
			stmNums2.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			var wrapperCollection = new USCustomsNumberViewStmNumsWrapperCollection(collection);
			AssertEquals("wrapperCollection.AllowNew", false, wrapperCollection.AllowNew);
			AssertEquals("wrapperCollection.AllowRemove", false, wrapperCollection.AllowRemove);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			var wrapper1 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums1.PK);
			AssertEquals("wrapper1.StmNums", stmNums1, wrapper1.StmNums);
			var wrapper2 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			var stmNums3 = collection.AddNew();
			stmNums3.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			var wrapper3 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			collection.Delete(stmNums1);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			wrapper2 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var stmNums4 = Factory.New<CustomsNumberViewStmNums>();
			stmNums4.Provider = provider;
			stmNums4.SN_Owner = Company.PK;
			stmNums4.SN_Type = stmNums2.SN_Type;
			stmNums4.SN_FountainName = stmNums2.SN_FountainName;
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			wrapper2 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var wrapper4 = (USCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums4.PK);
			AssertEquals("wrapper4.StmNums", stmNums4, wrapper4.StmNums);
			AssertExceptionThrown<NotImplementedException>(() => wrapperCollection.AddNew());
		}

		protected override USCustomsNumberViewStmNumsWrapperCollection GetCollectionToTest() => Provider.CustomsNumberWrappers;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = Provider.CustomsNumbers.AddNew();
			return Provider.GetOrCreateWrapper(stmNum);
		}

		USCustomsNumberViewStmNumsCompanyProvider provider;
		USCustomsNumberViewStmNumsCompanyProvider Provider => provider ?? (provider = new USCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK));

		GlbCompany company;
		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
	}
}
