using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsNumberStmNumberRangeValidationTest : BusinessObjectValidationTestCase
	{
		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestCheckSNR_ThresholdRunOutWarning()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNums1 = provider.NewCustomsNumber();
			stmNums1.SN_MinimumValue = 10L;
			stmNums1.SN_Count = 149L;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums1.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums1.SN_Name);
			stmNums1 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums1.Provider = provider;

			var range = stmNums1.GetNumberRanges()[0];
			range.SNR_ThresholdRunOutWarning = 150L;
			range.Validation.ValidateSNR_ThresholdRunOutWarning();
			AssertHasWarning(range.SNR_ThresholdRunOutWarningInfo, CustomsNumberStmNumberRangeValidation.ThresholdRunOutWarningIsLessThanTotalAvailableNumbers);
			var stmNums2 = provider.NewCustomsNumber();
			stmNums2.Provider = provider;
			stmNums2.SN_MinimumValue = 200L;
			stmNums2.SN_Count = 1L;
			newFactory.Save();
			range = newFactory.Load<CustomsNumberStmNumberRange>(range.PK);
			range.Validation.ValidateSNR_ThresholdRunOutWarning();
			AssertNoWarning(range.SNR_ThresholdRunOutWarningInfo, CustomsNumberStmNumberRangeValidation.ThresholdRunOutWarningIsLessThanTotalAvailableNumbers);
		}

		public void TestCheckSNR_Name()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNums1 = provider.NewCustomsNumber();
			stmNums1.SN_MinimumValue = 10L;
			stmNums1.SN_Count = 149L;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums1.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums1.SN_Name);
			stmNums1 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums1.Provider = provider;

			var range = stmNums1.GetNumberRanges()[0];
			range.SNR_Name = "TestWithNon-EnglishCharacter⑧";
			range.Validation.ValidateSNR_Name();
			AssertNoErrors(range.SNR_NameInfo);
		}
	}
}
