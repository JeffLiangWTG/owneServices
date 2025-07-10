using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGCustomsNumberViewStmNumsWrapperValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestValidateForRowNotifications()
		{
			Company.GC_CustomsRegistrationNo = "SA0001";
			var customsNumber = Company.CustomsNumberProvider.CustomsNumbers.AddNew();
			customsNumber.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber.SN_MinimumValue = 1000;
			customsNumber.SN_MaximumValue = 2000;
			customsNumber.SN_Value = 1000;
			customsNumber.SN_FountainName = "SA0001";
			customsNumber.Factory.Save();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "AAA";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			company2.GC_CustomsRegistrationNo = "SA0001";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "CCC";
			branch.GB_GC = company2.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var staff = GlbStaff.New(Factory);
			staff.GS_Code = "EEE";
			staff.GS_LoginName = "Squanchy";
			Factory.Save();
			var provider = Company.CustomsNumberProvider;
			AssertNotNull(provider);
			var stmNumsWrapper = Company.CustomsNumberProvider.GetOrCreateWrapper(customsNumber);
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var customsNumber2 = company2.CustomsNumberProvider.CustomsNumbers.AddNew();
				customsNumber2.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
				customsNumber2.SN_MinimumValue = 500;
				customsNumber2.SN_MaximumValue = 1500;
				customsNumber2.SN_Value = 500;
				customsNumber2.SN_FountainName = "SA0001";
				var stmNumsWrapper2 = company2.CustomsNumberProvider.GetOrCreateWrapper(customsNumber2);
				AssertNotNull(stmNumsWrapper2);
				var rowError = $"The SMN Number Range is overlapping with Company ({Company.GC_Code} - {Company.GC_Name}) which shares the same Customs Registration Number.";
				stmNumsWrapper2.Validation.ValidateAll();
				AssertHasRowError(stmNumsWrapper2, rowError);
				customsNumber2.SN_MinimumValue = 500;
				customsNumber2.SN_MaximumValue = 900;
				stmNumsWrapper2.Validation.ValidateAll();
				AssertNoRowError(stmNumsWrapper2, rowError);
				var rowWarning = "This range would be ignored as the Customs Registration Number doesn't match the company's Customs Registration Number.";
				Company.GC_CustomsRegistrationNo = "XCV00669";
				stmNumsWrapper.Validation.ValidateAll();
				AssertHasRowWarning(stmNumsWrapper, rowWarning);
				Company.GC_CustomsRegistrationNo = "SA0001";
				stmNumsWrapper.Validation.ValidateAll();
				AssertNoRowWarningContaining(stmNumsWrapper, rowWarning);
			}
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
