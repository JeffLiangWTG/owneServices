using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(StampDutyLedgerNumberCustomizationRegistrySetting))]
	class StampDutyLedgerNumberCustomizationRegistrySettingTest : RegistryBusinessObjectTemplateTestCase<StampDutyLedgerNumberCustomizationRegistrySetting>
	{
		public void TestReadElements()
		{
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting { StartDate = new ZDateTime(2022, 07, 01), EndDate = new ZDateTime(2023, 06, 30), StartNumber = 3456, ExpiredYear = 2022 };

			var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(StampDutyLedgerNumberCustomizationRegistrySetting));
			var serializedValue = dummyDataType.Serialise(setting);
			var deserializedBusinessObject = (StampDutyLedgerNumberCustomizationRegistrySetting)dummyDataType.Deserialise(serializedValue);

			CombineAssertions(() =>
			{
				AssertEquals(new ZDateTime(2022, 07, 01), deserializedBusinessObject.StartDate);
				AssertEquals(new ZDateTime(2023, 06, 30), deserializedBusinessObject.EndDate);
				AssertEquals(3456, deserializedBusinessObject.StartNumber);
				AssertEquals(2022, deserializedBusinessObject.ExpiredYear);
			});
		}

		public void TestSetCustomDefaultValues()
		{
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			AssertEquals(1, setting.StartNumber);

			setting.StartDate = new ZDateTime(2022, 07, 01);
			AssertEquals(new ZDateTime(2023, 06, 30), setting.EndDate);
		}

		public void TestValidateStartDate()
		{
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			setting.EndDate = new ZDateTime(2022, 06, 30);

			var message = "Start Date should not be greater than End Date.";
			setting.StartDate = new ZDateTime(2022, 07, 01);
			AssertHasErrorContaining(setting.StartDateInfo, message);

			setting.StartDate = new ZDateTime(2021, 07, 01);
			AssertNoErrorContaining(setting.StartDateInfo, message);

			message = "Start Date should not be more than one year earlier than than End Date.";
			setting.StartDate = new ZDateTime(2021, 06, 30);
			AssertHasErrorContaining(setting.StartDateInfo, message);

			setting.StartDate = new ZDateTime(2021, 07, 01);
			AssertNoErrorContaining(setting.StartDateInfo, message);
		}

		public void TestValidateEndDate()
		{
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			setting.StartDate = new ZDateTime(2022, 07, 01);

			var message = "End Date should not be less than Start Date.";
			setting.EndDate = new ZDateTime(2022, 06, 30);
			AssertHasErrorContaining(setting.EndDateInfo, message);

			setting.EndDate = new ZDateTime(2023, 06, 30);
			AssertNoErrorContaining(setting.EndDateInfo, message);

			message = "End Date should not be more than one year later than Start Date.";
			setting.EndDate = new ZDateTime(2023, 07, 01);
			AssertHasErrorContaining(setting.EndDateInfo, message);

			setting.EndDate = new ZDateTime(2023, 06, 30);
			AssertNoErrorContaining(setting.EndDateInfo, message);
		}

		public void TestValidateStartNumber()
		{
			var message = "Start Number should be between 1 and 9999999.";
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			setting.StartNumber = -1;
			AssertHasErrorContaining(setting.StartNumberInfo, message);
			setting.StartNumber = 3456;
			AssertNoErrorContaining(setting.StartNumberInfo, message);
			setting.StartNumber = 19999999;
			AssertHasErrorContaining(setting.StartNumberInfo, message);
		}

		[TestDate(2022, 06, 02)]
		public void TestValidateExpiredYear()
		{
			var message = "Expired End of Fiscal Year should be equal to or greater than current year.";
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			setting.ExpiredYear = 2023;
			AssertNoErrorContaining(setting.ExpiredYearInfo, message);
			setting.ExpiredYear = 2020;
			AssertHasErrorContaining(setting.ExpiredYearInfo, message);
			setting.ExpiredYear = 2022;
			AssertNoErrorContaining(setting.ExpiredYearInfo, message);
		}

		[TestDate(2022, 06, 02)]
		public void TestRunPreSaveValidation()
		{
			var setting = new StampDutyLedgerNumberCustomizationRegistrySetting();
			setting.StartDate = new ZDateTime(2022, 07, 01);
			setting.EndDate = new ZDateTime(2022, 06, 30);
			setting.StartNumber = 0;
			setting.ExpiredYear = 2020;
			setting.ClearAllNotifications();
			setting.RunPreSaveValidation();
			AssertHasErrorContaining(setting.StartDateInfo, "Start Date should not be greater than End Date.");
			AssertHasErrorContaining(setting.EndDateInfo, "End Date should not be less than Start Date.");
			AssertHasErrorContaining(setting.StartNumberInfo, "Start Number should be between 1 and 9999999.");
			AssertHasErrorContaining(setting.ExpiredYearInfo, "Expired End of Fiscal Year should be equal to or greater than current year.");
		}

		protected override StampDutyLedgerNumberCustomizationRegistrySetting GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override StampDutyLedgerNumberCustomizationRegistrySetting GetBusinessObjectToSerialise() => BizObj;

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;
	}
}
