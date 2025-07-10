using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberStmNumberRange))]
	sealed class CustomsNumberStmNumberRangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNumberRangeDetail()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			provider.ThresholdRunOutWarningForTesting = 1000L;
			var stmNums = provider.NewCustomsNumber();
			stmNums.SN_MinimumValue = 100L;
			stmNums.SN_Count = 1001L;
			stmNums.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			var range = stmNums.GetNumberRanges()[0];
			range.SNR_Owner = GlbCompany.CurrentCompany.PK;
			AssertNotNull(range.Provider);
			AssertNotEquals(range.SNR_Name, range.Detail);
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestProperties()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			provider.ThresholdRunOutWarningForTesting = 1000L;
			var stmNums = provider.NewCustomsNumber();
			stmNums.SN_MinimumValue = 100L;
			stmNums.SN_Count = 1001L;
			stmNums.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			stmNums = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums.Provider = provider;
			var range = stmNums.GetNumberRanges()[0];
			AssertEquals("range.SNR_Name", "C#ER-CEN _BOB NUMBER", range.SNR_Name);
			AssertEquals("range.SNR_NameInfo.ReadOnly", true, range.SNR_NameInfo.ReadOnly);
			AssertEquals("range.SNR_Owner", GlbCompany.CurrentCompany.PK, range.SNR_Owner);
			AssertEquals("range.SNR_OwnerInfo.ReadOnly", true, range.SNR_OwnerInfo.ReadOnly);
			AssertEquals("range.SNR_OwnerForDisplay", "EDI - Eagle Datamation International", range.SNR_OwnerForDisplay);
			AssertEquals("range.Detail", "Owner: Company - EDI - Eagle Datamation International, Range Type: CEN, Name: BOB NUMBER", range.Detail);
			AssertEquals("range.TotalAvailableNumbers", 1001L, range.TotalAvailableNumbers);
			AssertEquals("range.HasReachedLimit", false, range.HasReachedLimit);

			using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				stmNums.TryGetNumberFountain().GetNextFormatted(Factory);
				manager.CommitTransaction();
			}
			var newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			stmNums = newFactory.LoadTop1<CustomsNumberViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_ID, stmNums.SN_ID));
			stmNums.Provider = provider;
			range = stmNums.GetNumberRanges()[0];
			AssertEquals("range.TotalAvailableNumbers", 1000L, range.TotalAvailableNumbers);
			AssertEquals("range.HasReachedLimit", true, range.HasReachedLimit);
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestGetStmNums()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
			provider.ThresholdRunOutWarningForTesting = 1000L;
			var stmNum1 = provider.NewCustomsNumber();
			stmNum1.SN_MinimumValue = 1000L;
			stmNum1.SN_Count = 1001L;
			var stmNum2 = provider.NewCustomsNumber();
			stmNum2.SN_Type = "CE2";
			stmNum2.SN_MinimumValue = 1000L;
			stmNum2.SN_Count = 1001L;
			stmNum2.Factory.Save();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNum1.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNum1.SN_Owner);
			stmNum1 = CustomsNumberViewStmNumsHelper.LoadTop1StmNums(Factory, query);
			var range = stmNum1.GetNumberRanges()[0];
			var stmNums = range.GetStmNums();
			AssertEquals(1, stmNums.Length);
			AssertEquals(stmNum1, stmNums[0]);

			var stmNum3 = provider.NewCustomsNumber();
			stmNum3.SN_MinimumValue = 3000L;
			stmNum3.SN_Count = 1001L;
			stmNum3.Factory.Save();
			Factory.InvalidateCachedProperties();
			query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNum3.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNum3.SN_Owner);
			stmNum3 = CustomsNumberViewStmNumsHelper.LoadTop1StmNums(Factory, query);
			stmNums = range.GetStmNums();
			AssertEquals(2, stmNums.Length);
			AssertCollectionContains(stmNum1, stmNums);
			AssertCollectionContains(stmNum3, stmNums);
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
