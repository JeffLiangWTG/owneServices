using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberStmNumberRangeCollection))]
	sealed class CustomsNumberStmNumberRangeCollectionTest : ActiveBusinessObjectCollectionTestCase<CustomsNumberStmNumberRangeCollection>
	{
		[UseSnapshotProtection]
		public void TestCollectionContainCorrectData()
		{
			var collection = Provider.CustomsNumbers;
			var rangeCollection = Provider.NumberRanges;
			AssertEquals(typeof(CustomsNumberStmNumberRangeCollection), rangeCollection.GetType());
			var stmNums = collection.AddNew();
			stmNums.SN_Type = "CEN";
			stmNums.SN_FountainName = "BOB NUMBER";
			stmNums.SN_MinimumValue = 100L;
			stmNums.SN_Count = 1001L;
			Factory.Save();
			AssertEquals("rangeCollection.Provider", Provider, rangeCollection.Provider);
			AssertEquals("rangeCollection.Count", 1, rangeCollection.Count);
			var range = rangeCollection[0];
			AssertEquals("range.Provider", Provider, range.Provider);
			AssertEquals("range.SNR_Name", "C#ER-CEN _BOB NUMBER", range.SNR_Name);
			AssertEquals("range.SNR_Owner", provider.Parent.PK, range.SNR_Owner);

			var range2 = Factory.New<CustomsNumberStmNumberRange>();
			range2.SNR_Name = range.SNR_Name + "A";
			range2.SNR_Owner = GlbBranch.CurrentBranch.PK;

			var range3 = Factory.New<CustomsNumberStmNumberRange>();
			range3.SNR_Name = "#@@#3";
			range3.SNR_Owner = GlbBranch.CurrentBranch.PK;

			AssertEquals("rangeCollection.Count", 2, rangeCollection.Count);
			AssertCollectionContains(range, rangeCollection);
			AssertCollectionContains(range2, rangeCollection);
		}

		public void TestRangeCollectionForTwoCompaniesInOneFactory()
		{
			var sg1 = Factory.New<GlbCompany>();
			sg1.GC_Code = "SC1";
			sg1.GC_RN_NKCountryCode = "SG";
			sg1.GC_CustomsRegistrationNo = "SA0001";
			var sg1Branch = sg1.Branches.AddNew();
			sg1Branch.GB_Code = "SB1";

			var sg2 = Factory.New<GlbCompany>();
			sg2.GC_Code = "SC2";
			sg2.GC_RN_NKCountryCode = "SG";
			sg2.GC_CustomsRegistrationNo = "SA0002";
			var sg2Branch = sg2.Branches.AddNew();
			sg2Branch.GB_Code = "SB2";
			Factory.Save();

			var provider1 = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, "SG", sg1.PK);
			var collection1 = provider1.CustomsNumbers;
			var customsNumber1 = collection1.AddNew();
			customsNumber1.SN_Type = "SMN";
			customsNumber1.SN_MinimumValue = 1000L;
			customsNumber1.SN_MaximumValue = 2000L;
			customsNumber1.SN_Count = 1001L;
			customsNumber1.SN_FountainName = "SA0001";
			Factory.Save();
			AssertEquals("C#SG-SMN _SA0001|1", customsNumber1.SN_Name);
			AssertEquals(sg1.PK, customsNumber1.SN_Owner);

			var rangeCollection1 = provider1.NumberRanges;
			AssertEquals("rangeCollection.Count", 1, rangeCollection1.Count);
			var range1 = rangeCollection1[0];
			AssertEquals("range.SNR_Name", "C#SG-SMN _SA0001", range1.SNR_Name);
			AssertEquals("range.SNR_Owner", sg1.PK, range1.SNR_Owner);

			var provider2 = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, "SG", sg2.PK);
			var collection2 = provider2.CustomsNumbers;
			var customsNumber2 = collection2.AddNew();
			customsNumber2.SN_Type = "SMN";
			customsNumber2.SN_MinimumValue = 3000L;
			customsNumber2.SN_MaximumValue = 4000L;
			customsNumber2.SN_Count = 3001L;
			customsNumber2.SN_FountainName = "SA0002";
			Factory.Save();
			AssertEquals("C#SG-SMN _SA0002|1", customsNumber2.SN_Name);
			AssertEquals(sg2.PK, customsNumber2.SN_Owner);

			var rangeCollection2 = provider2.NumberRanges;
			AssertEquals("rangeCollection.Count", 1, rangeCollection2.Count);
			var range2 = rangeCollection2[0];
			AssertEquals("range.SNR_Name", "C#SG-SMN _SA0002", range2.SNR_Name);
			AssertEquals("range.SNR_Owner", sg2.PK, range2.SNR_Owner);
		}

		#region Implementation

		CustomsNumberViewStmNumsCompanyProviderForTest Provider => provider ?? (provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory));
		CustomsNumberViewStmNumsCompanyProviderForTest provider;

		protected override CustomsNumberStmNumberRangeCollection GetCollectionToTest()
		{
			return new CustomsNumberStmNumberRangeCollection(Provider);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var range = Factory.New<CustomsNumberStmNumberRange>();
			range.SNR_Name = CustomsNumberViewStmNums.GenerateNamePrefix(Core.Constants.CountryCodes.Eritrea);
			range.SNR_Owner = Provider.Parent.PK;
			return range;
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

		#endregion
	}
}
