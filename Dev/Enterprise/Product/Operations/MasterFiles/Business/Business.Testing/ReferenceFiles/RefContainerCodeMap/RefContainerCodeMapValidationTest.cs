using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefContainerCodeMapValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRCM_Code()
		{
			SetupJPRCM_Code();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var testItem = Factory.NewWithValidTestData<RefContainerCodeMap>();
				testItem.RCM_RN_NKCountry = "US";
				testItem.RCM_Code = "";
				AssertHasErrorContaining(testItem.RCM_CodeInfo, MandatoryValidation.MustBeEntered);

				testItem.RCM_Code = "XXX";
				AssertHasMessageErrorContaining(testItem.RCM_CodeInfo, ListValidation.InvalidCodeMessageError);

				testItem.RCM_Code = "AF";
				AssertNoNotifications(testItem.RCM_CodeInfo);

				testItem.RCM_RN_NKCountry = "JP";

				testItem.RCM_Code = "12GP";
				AssertNoErrorContaining(testItem.RCM_CodeInfo, ListValidation.InvalidCodeError);

				testItem.RCM_Code = "21GP";
				testItem.Validation.ValidateRCM_Code();
				AssertHasErrorContaining(testItem.RCM_CodeInfo, ListValidation.InvalidCodeError);
			}
		}

		public void TestCheckRCM_Usage()
		{
			var testContainer = Factory.NewWithValidTestData<RefContainer>();
			var testItem = testContainer.CodeMapCollection.AddNew();

			testItem.RCM_RN_NKCountry = "US";
			testItem.RCM_Usage = "CUS";
			AssertHasMessageErrorContaining(testItem.RCM_UsageInfo, ListValidation.InvalidCodeMessageError);

			testItem.RCM_Usage = "AMS";
			AssertNoNotifications(testItem.RCM_UsageInfo);

			testItem.RCM_Usage = "";
			AssertNoNotifications(testItem.RCM_UsageInfo);

			testItem.RCM_RN_NKCountry = "CZ";
			testItem.RCM_Usage = "CUS";
			AssertHasMessageErrorContaining(testItem.RCM_UsageInfo, MandatoryValidation.DoNotEntered);

			testItem.RCM_Usage = "";
			AssertNoNotifications(testItem.RCM_UsageInfo);

			var testItem2 = testContainer.CodeMapCollection.AddNew();
			testItem2.RCM_RN_NKCountry = "CZ";
			testItem2.Validation.ValidateRCM_Usage();

			AssertHasErrorContaining(testItem2.RCM_UsageInfo, "There is already a code map with same Country/Region and Usage.");
		}

		public void TestCheckRCM_RN_NKCountry()
		{
			var testItem = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testItem.RCM_RN_NKCountry = "";

			AssertHasErrorContaining(testItem.RCM_RN_NKCountryInfo, MandatoryValidation.MustBeEntered);

			testItem.RCM_RN_NKCountry = "XX";
			AssertHasErrorContaining(testItem.RCM_RN_NKCountryInfo, "Enter a valid selection.");
		}

		void SetupJPRCM_Code()
		{
			Factory.Load(ZZRefCusCodeListSchema.Constants.Prefix, ZGuid.NewZGuid());
			var table = ((INeedDataSet)Factory).Data.Tables[ZZRefCusCodeListSchema.Constants.TableName];
			var heightRow = table.NewRow();
			var heightPk = ZGuid.NewZGuid();
			heightRow[ZZRefCusCodeListSchema.Constants.PK] = heightPk.ToGuid();
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_CodeType] = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight;
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_Code] = "2";
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "1112";
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_CountryOrGrouping] = Core.Constants.CountryCodes.Japan;
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_StartDate] = ZDateTime.Today.AddDays(-1).ToDateTime();
			heightRow[ZZRefCusCodeListSchema.Constants.ZZD_EndDate] = ZDateTime.Today.AddDays(1).ToDateTime();
			table.Rows.Add(heightRow);
			var lengthRow = table.NewRow();
			var lengthPk = ZGuid.NewZGuid();
			lengthRow[ZZRefCusCodeListSchema.Constants.PK] = lengthPk.ToGuid();
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_CodeType] = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength;
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_Code] = "1";
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "1112";
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_CountryOrGrouping] = Core.Constants.CountryCodes.Japan;
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_StartDate] = ZDateTime.Today.AddDays(-1).ToDateTime();
			lengthRow[ZZRefCusCodeListSchema.Constants.ZZD_EndDate] = ZDateTime.Today.AddDays(1).ToDateTime();
			table.Rows.Add(lengthRow);
			var typeRow = table.NewRow();
			var typePk = ZGuid.NewZGuid();
			typeRow[ZZRefCusCodeListSchema.Constants.PK] = typePk.ToGuid();
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_CodeType] = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType;
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_Code] = "GP";
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "1112";
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_CountryOrGrouping] = Core.Constants.CountryCodes.Japan;
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_StartDate] = ZDateTime.Today.AddDays(-1).ToDateTime();
			typeRow[ZZRefCusCodeListSchema.Constants.ZZD_EndDate] = ZDateTime.Today.AddDays(1).ToDateTime();
			table.Rows.Add(typeRow);
			Factory.Save();
		}
	}
}
