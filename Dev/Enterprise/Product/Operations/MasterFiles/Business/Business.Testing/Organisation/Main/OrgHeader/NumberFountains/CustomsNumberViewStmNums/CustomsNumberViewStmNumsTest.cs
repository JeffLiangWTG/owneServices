using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class CustomsNumberViewStmNumsNonTransactionedTest : TestCase
	{
		public void TestProperties()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums = provider.NewCustomsNumber();
			AssertEquals("stmNums.SN_NameInfo.ReadOnly", true, stmNums.SN_NameInfo.ReadOnly);
			AssertEquals("stmNums.SN_TypeInfo.ReadOnly", false, stmNums.SN_TypeInfo.ReadOnly);
			AssertEquals("stmNums.SN_PrefixInfo.ReadOnly", true, stmNums.SN_PrefixInfo.ReadOnly);
			AssertEquals("stmNums.SN_FountainNameInfo.ReadOnly", false, stmNums.SN_FountainNameInfo.ReadOnly);
			AssertEquals("stmNums.SN_Name", "C#ER-CEN _BOB NUMBER|", stmNums.SN_Name);
			AssertEquals("stmNums.SN_Prefix", "BOB NUMBER|", stmNums.SN_Prefix);
			AssertEquals("stmNums.SequenceInfo.ReadOnly", false, stmNums.SequenceInfo.ReadOnly);
			AssertEquals("stmNums.Sequence", (ZShort)0, stmNums.Sequence);
			AssertEquals("stmNums.SN_OwnerInfo.ReadOnly", false, stmNums.SN_OwnerInfo.ReadOnly);
			AssertEquals("stmNums.SN_Owner", GlbCompany.CurrentCompany.PK, stmNums.SN_Owner);
			AssertEquals("stmNums.SN_MinimumValueInfo.ReadOnly", false, stmNums.SN_MinimumValueInfo.ReadOnly);
			AssertEquals("stmNums.SN_MinimumValue", ViewStmNums.Schema.MinimumValue, (long)stmNums.SN_MinimumValue);
			AssertEquals("stmNums.SN_MaximumValueInfo.ReadOnly", true, stmNums.SN_MaximumValueInfo.ReadOnly);
			AssertEquals("stmNums.SN_MaximumValue", stmNums.DefaultTypeRangeMax, stmNums.SN_MaximumValue);
			AssertEquals("stmNums.SN_ValueInfo.ReadOnly", true, stmNums.SN_ValueInfo.ReadOnly);
			AssertEquals("stmNums.SN_Value", 1L, stmNums.SN_Value);
			AssertEquals("stmNums.SN_AvailableNumbers", stmNums.DefaultTypeRangeMax, stmNums.SN_AvailableNumbers);
			stmNums.Factory.Save();
			var rangeDetails = stmNums.GetNumberRanges();
			AssertEquals("A NumberRangeDetail should be create on saving", 1, rangeDetails.Length);

			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums.Provider = provider;
			stmNums.SN_MinimumValue = 101L;
			stmNums.SN_Count = 1000L;
			AssertEquals("stmNums.SN_Value", 101L, stmNums.SN_Value);
			AssertEquals("stmNums.SN_ValueForDisplay", 1L, stmNums.SN_ValueForDisplay);
			AssertEquals("stmNums.SN_Count", 1000L, stmNums.SN_Count);
			AssertEquals("stmNums.IsNumberFountainValid", false, stmNums.IsNumberFountainValid);
			AssertEquals("stmNums.SN_OwnerInfo.ReadOnly", false, stmNums.SN_OwnerInfo.ReadOnly);
			AssertEquals("stmNums.SN_MinimumValueInfo.ReadOnly", false, stmNums.SN_MinimumValueInfo.ReadOnly);
			AssertEquals("stmNums.SN_AvailableNumbers", 1100L, stmNums.SN_AvailableNumbers);
			Factory.Save();

			AssertEquals("stmNums.HasChanges", false, stmNums.HasChanges);
			AssertEquals("stmNums.SN_Value", 101L, stmNums.SN_Value);
			AssertEquals("stmNums.SN_ValueForDisplay", 101L, stmNums.SN_ValueForDisplay);
			AssertEquals("stmNums.SN_Count", 1000L, stmNums.SN_Count);
			AssertEquals("stmNums.IsNumberFountainValid", true, stmNums.IsNumberFountainValid);
			AssertEquals("stmNums.SN_OwnerInfo.ReadOnly", false, stmNums.SN_OwnerInfo.ReadOnly);
			AssertEquals("stmNums.SN_MinimumValueInfo.ReadOnly", false, stmNums.SN_MinimumValueInfo.ReadOnly);
			AssertEquals("stmNums.SN_AvailableNumbers", 1000L, stmNums.SN_AvailableNumbers);

			Db.Connection.RunInTransaction(() => stmNums.TryGetNumberFountain().GetNextFormatted(Factory));

			AssertEquals("stmNums.HasChanges", false, stmNums.HasChanges);
			AssertEquals("stmNums.SN_ValueForDisplay", 102L, stmNums.SN_ValueForDisplay);
			AssertEquals("stmNums.SN_Count", 1000L, stmNums.SN_Count);
			AssertEquals("stmNums.IsNumberFountainValid", true, stmNums.IsNumberFountainValid);
			AssertEquals("stmNums.SN_AvailableNumbers", 999L, stmNums.SN_AvailableNumbers);

			var stmNums2 = provider.NewCustomsNumber();
			stmNums2.SN_MinimumValue = 4000L;
			stmNums2.SN_Count = 1001L;
			AssertEquals("stmNums.HasChanges", false, stmNums.HasChanges);
			AssertEquals("stmNums.SN_ValueForDisplay", 102L, stmNums.SN_ValueForDisplay);
			AssertEquals("stmNums.SN_Count", 1000L, stmNums.SN_Count);
			AssertEquals("stmNums.IsNumberFountainValid", true, stmNums.IsNumberFountainValid);
			AssertEquals("stmNums.SN_AvailableNumbers", 999L, stmNums.SN_AvailableNumbers);

			AssertEquals("stmNums2.HasChanges", true, stmNums2.HasChanges);
			AssertEquals("stmNums2.SN_ValueForDisplay", 4000L, stmNums2.SN_ValueForDisplay);
			AssertEquals("stmNums2.SN_MaximumValue", 5000L, stmNums2.SN_MaximumValue);
			AssertEquals("stmNums2.IsNumberFountainValid", false, stmNums2.IsNumberFountainValid);
			AssertEquals("stmNums2.SN_AvailableNumbers", 1001L, stmNums2.SN_AvailableNumbers);

			var stmNumsList = stmNums2.GetAllMatchingNameAndOwner();
			AssertEquals("stmNumsList.Length", 2, stmNumsList.Length);
			AssertCollectionContains(stmNums, stmNumsList);
			AssertCollectionContains(stmNums2, stmNumsList);

			stmNumsList = stmNums2.GetAllMatchingNameAndOwner(true);
			AssertEquals("stmNumsList.Length", 1, stmNumsList.Length);
			AssertEquals("stmNums", stmNums, stmNumsList[0]);
		}

		#region Implementation

		BusinessObjectFactory Factory { get; } = new BusinessObjectFactory();

		#endregion // Implementation
	}

	[TestedType(typeof(CustomsNumberViewStmNums))]
	[UseSnapshotProtection]
	class CustomsNumberViewStmNumsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSequenceAllocationOnSaving()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums1 = provider.NewCustomsNumber();
			stmNums1.Factory.Save();
			AssertEquals(1, stmNums1.Sequence);
			AssertEquals("BOB NUMBER|1", stmNums1.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums2 = provider.NewCustomsNumber();
			stmNums2.Sequence = 11;
			AssertEquals(11, stmNums2.Sequence);
			AssertEquals("BOB NUMBER|B", stmNums2.SN_Prefix);

			var stmNums3 = provider.NewCustomsNumber();
			stmNums3.Factory.Save();
			AssertEquals(12, stmNums3.Sequence);
			AssertEquals("BOB NUMBER|C", stmNums3.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums4 = provider.NewCustomsNumber();
			stmNums4.Factory.Save();
			AssertEquals(13, stmNums4.Sequence);
			AssertEquals("BOB NUMBER|D", stmNums4.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums5 = provider.NewCustomsNumber();
			stmNums5.Sequence = 46655;
			AssertEquals(46655, stmNums5.Sequence);
			AssertEquals("BOB NUMBER|ZZZ", stmNums5.SN_Prefix);

			var stmNums6 = provider.NewCustomsNumber();
			stmNums6.Factory.Save();
			AssertEquals(46656, stmNums6.Sequence);
			AssertEquals("BOB NUMBER|0", stmNums6.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums7 = provider.NewCustomsNumber();
			stmNums7.Factory.Save();
			AssertEquals(46657, stmNums7.Sequence);
			AssertEquals("BOB NUMBER|00", stmNums7.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums8 = provider.NewCustomsNumber();
			stmNums8.Factory.Save();
			AssertEquals(46658, stmNums8.Sequence);
			AssertEquals("BOB NUMBER|000", stmNums8.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums9 = provider.NewCustomsNumber();
			stmNums9.Factory.Save();
			AssertEquals(0, stmNums9.Sequence);
			AssertEquals("BOB NUMBER|", stmNums9.SN_Prefix);

			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums = provider.CustomsNumbers.Cast<CustomsNumberViewStmNums>().OrderBy(x => x.Sequence).ToArray();
			AssertEquals(9, stmNums.Length);
			AssertData(stmNums[0], 0, "BOB NUMBER|");
			AssertData(stmNums[1], 1, "BOB NUMBER|1");
			AssertData(stmNums[2], 11, "BOB NUMBER|B");
			AssertData(stmNums[3], 12, "BOB NUMBER|C");
			AssertData(stmNums[4], 13, "BOB NUMBER|D");
			AssertData(stmNums[5], 46655, "BOB NUMBER|ZZZ");
			AssertData(stmNums[6], 46656, "BOB NUMBER|0");
			AssertData(stmNums[7], 46657, "BOB NUMBER|00");
			AssertData(stmNums[8], 46658, "BOB NUMBER|000");
		}

		public void TestSN_CanRolloverOnSaving()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			provider.CanRolloverForTesting = true;
			var stmNums = provider.NewCustomsNumber();
			stmNums.Factory.Save();

			AssertEquals("SN_CanRollover", true, stmNums.SN_CanRollover);
		}

		public void TestGenerateNextCustomsNumber()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums = provider.NewCustomsNumber();
			stmNums.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums.Provider = provider;
			var connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				AssertEquals("00000001", stmNums.GenerateNextCustomsNumber(Factory));
				AssertEquals(2L, stmNums.SN_ValueForDisplay);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestSetNextNumber()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums = provider.NewCustomsNumber();
			stmNums.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums.Provider = provider;
			var connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				stmNums.SetNextNumber(4101L);
				AssertEquals(4101L, stmNums.SN_ValueForDisplay);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestGenerateCustomsNumber()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			provider.CanRolloverForTesting = true;
			var stmNums = provider.NewCustomsNumber();
			stmNums.SN_MinimumValue = 1000;
			stmNums.SN_MaximumValue = 1001;
			stmNums.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums.Provider = provider;
			AssertEquals(ZString.Empty, stmNums.GenerateCustomsNumber(-1));
			AssertEquals(ZString.Empty, stmNums.GenerateCustomsNumber(0));
			AssertEquals("00000001", stmNums.GenerateCustomsNumber(1));
		}

		#region Implementation

		void AssertData(CustomsNumberViewStmNums stmNum, int sequence, ZString prefix)
		{
			AssertEquals(sequence, stmNum.Sequence);
			AssertEquals(prefix, stmNum.SN_Prefix);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory).NewCustomsNumber();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			var stmNums = provider.NewCustomsNumber();
			stmNums.Factory.Save();
			stmNums = factory.LoadTop1<CustomsNumberViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_ID, stmNums.SN_ID));
			stmNums.Provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(factory);
			return stmNums;
		}

		#endregion
	}
}
