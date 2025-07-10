using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCollection))]
	sealed class CustomsNumberViewStmNumsCollectionTest : ActiveBusinessObjectCollectionTestCase<CustomsNumberViewStmNumsCollection>
	{
		public void TestSetDefaultsForNewChildAndSetCollectionRelationships()
		{
			var collection = Provider.CustomsNumbers;
			AssertEquals(typeof(CustomsNumberViewStmNumsCollection), collection.GetType());
			var stmNums = collection.AddNew();
			AssertEquals("stmNums.SN_Name", "C#ER-", stmNums.SN_Name);
			AssertEquals("stmNums.SN_Prefix", ZString.Empty, stmNums.SN_Prefix);
			AssertEquals("stmNums.SN_Type", ZString.Empty, stmNums.SN_Type);
			AssertEquals("stmNums.SN_FountainName", ZString.Empty, stmNums.SN_FountainName);
			AssertEquals("stmNums.SN_Owner", provider.Parent.PK, stmNums.SN_Owner);
			AssertEquals("stmNums.Provider", Provider, stmNums.Provider);

			var stmNums2 = Factory.New<CustomsNumberViewStmNums>();
			stmNums2.Provider = Provider;
			stmNums2.Sequence = 4;
			stmNums2.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums2.SN_Type = "CEN";
			stmNums2.SN_FountainName = "HELLO WORLD";
			AssertEquals("stmNums2.SN_Name", "C#ER-CEN _HELLO WORLD|4", stmNums2.SN_Name);
			AssertEquals("stmNums2.SN_Prefix", "HELLO WORLD|4", stmNums2.SN_Prefix);
			AssertEquals("stmNums2.SN_Type", "CEN", stmNums2.SN_Type);
			AssertEquals("stmNums2.SN_FountainName", "HELLO WORLD", stmNums2.SN_FountainName);
			AssertEquals("stmNums2.SN_Owner", GlbBranch.CurrentBranch.PK, stmNums2.SN_Owner);
			AssertEquals("stmNums2.Provider", Provider, stmNums2.Provider);
			AssertEquals("stmNums2.Sequence", (ZShort)4, stmNums2.Sequence);
		}

		public void TestNumbersCollectionForTwoCompaniesInOneFactory()
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
			var collection1 = new CustomsNumberViewStmNumsCollection(provider1);
			var customsNumber1 = collection1.AddNew();
			customsNumber1.SN_Type = "SMN";
			customsNumber1.SN_MinimumValue = 1000;
			customsNumber1.SN_MaximumValue = 2000;
			customsNumber1.SN_Value = 1000;
			customsNumber1.SN_FountainName = "SA0001";
			Factory.Save();
			AssertEquals("C#SG-SMN _SA0001|1", customsNumber1.SN_Name);
			AssertEquals(sg1.PK, customsNumber1.SN_Owner);

			var provider2 = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, "SG", sg2.PK);
			var collection2 = new CustomsNumberViewStmNumsCollection(provider2);
			var customsNumber2 = collection2.AddNew();
			customsNumber2.SN_Type = "SMN";
			customsNumber2.SN_MinimumValue = 3000;
			customsNumber2.SN_MaximumValue = 4000;
			customsNumber2.SN_Value = 3000;
			customsNumber2.SN_FountainName = "SA0002";
			Factory.Save();
			AssertEquals("C#SG-SMN _SA0002|1", customsNumber2.SN_Name);
			AssertEquals(sg2.PK, customsNumber2.SN_Owner);

			// prove the numbers can reload
			var factory2 = new BusinessObjectFactory();
			var nums = factory2.Load<CustomsNumberViewStmNums>(new ZQuery(ZArchitecture.Schema.ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, "C#SG"));
			AssertEquals(2, nums.Length);
			var num1 = nums.FirstOrDefault(num => num.SN_Owner == sg1.PK);
			AssertEquals("customsNumber1 sg1 SN_Type", "SMN", num1.SN_Type);
			AssertEquals("customsNumber1 sg1 SN_Prefix", "SA0001|1", num1.SN_Prefix);
			AssertEquals("customsNumber1 sg1 SN_Name", "C#SG-SMN _SA0001|1", num1.SN_Name);
			var num2 = nums.FirstOrDefault(num => num.SN_Owner == sg2.PK);
			AssertEquals("customsNumber2 sg2 SN_Type", "SMN", num2.SN_Type);
			AssertEquals("customsNumber2 sg2 SN_Prefix", "SA0002|1", num2.SN_Prefix);
			AssertEquals("customsNumber2 sg2 SN_Name", "C#SG-SMN _SA0002|1", num2.SN_Name);
		}

		#region Implementation

		CustomsNumberViewStmNumsCompanyProviderForTest Provider => provider ?? (provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory));
		CustomsNumberViewStmNumsCompanyProviderForTest provider;

		protected override CustomsNumberViewStmNumsCollection GetCollectionToTest()
		{
			return new CustomsNumberViewStmNumsCollection(Provider);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.SN_Name = CustomsNumberViewStmNums.GenerateNamePrefix(Core.Constants.CountryCodes.Eritrea);
			stmNum.SN_Owner = Provider.Parent.PK;
			return stmNum;
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
