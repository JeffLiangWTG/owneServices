using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressNumberCollection))]
	sealed class JobDocAddressNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobDocAddressNumber>();

		public void TestFind()
		{
			var testCollection = new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());
			testCollection.AddNew(OrgCusCode.CodeTypes.SupplierCode, Core.Constants.CountryCodes.China);
			testCollection.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.UnitedStates);
			var expectedFindResult = testCollection.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.China);

			AssertEquals("Should found the correct result.", expectedFindResult, testCollection.Find(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.China));
			AssertNoExceptionThrown("Should not throw exception when no result fould.", () => testCollection.Find(OrgCusCode.CodeTypes.GS1, Core.Constants.CountryCodes.Uruguay));
		}

		public void TestFindOrCreate()
		{
			var testCollection = new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());
			var existingItem = testCollection.AddNew(OrgCusCode.CodeTypes.SupplierCode, Core.Constants.CountryCodes.China);

			var findResult = testCollection.FindOrCreate(OrgCusCode.CodeTypes.SupplierCode, Core.Constants.CountryCodes.China);
			AssertSame("Should have found the correct existing item.", existingItem, findResult);

			var countBeforeCreate = testCollection.Count;
			var createResult = testCollection.FindOrCreate(OrgCusCode.CodeTypes.GS1, Core.Constants.CountryCodes.Uruguay);
			AssertEquals("Should have created an new item.", 1, testCollection.Count - countBeforeCreate);
			AssertEquals("New item E2N_NumberType", OrgCusCode.CodeTypes.GS1, createResult.E2N_NumberType);
			AssertEquals("New item E2N_RN_NKCountryCode", Core.Constants.CountryCodes.Uruguay, createResult.E2N_RN_NKCountryCode);
		}

		public void TestAddNewWithTypeAndCountryCode()
		{
			var testCollection = new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());
			var result = testCollection.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.China);
			AssertEquals("E2N_NumberType", OrgCusCode.CodeTypes.CustomsClientCode, result.E2N_NumberType);
			AssertEquals("E2N_RN_NKCountryCode", Core.Constants.CountryCodes.China, result.E2N_RN_NKCountryCode);
		}

		public void TestFindFirstByNumberType()
		{
			var testCollection = new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());
			var aeoNumber = testCollection.AddNew(OrgCusCode.ChinaCodeTypes.AEO, Core.Constants.CountryCodes.Uganda);
			var ciqCNNumber = testCollection.AddNew(OrgCusCode.ChinaCodeTypes.CIQ, Core.Constants.CountryCodes.China);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AssertSame(ciqCNNumber, testCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.CIQ));
				AssertSame(aeoNumber, testCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.AEO));
			}
		}
	}
}
