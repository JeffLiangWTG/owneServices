using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportPGARequirementIndicatorTest : TestCaseWithFactory
	{
		public void TestExportPGARequirementOnInvoiceLine()
		{
			invoiceLine.JI_Tariff = "0000000001";
			var requirementIndicator = invoiceLine.ExportPGARequirementIndicator;
			AssertEquals(false, requirementIndicator.RequireAMS);
			AssertEquals(false, requirementIndicator.RequireATF);
			AssertEquals(false, requirementIndicator.RequireEPA);
			AssertEquals(false, requirementIndicator.RequireNMFS);
			AssertEquals(false, requirementIndicator.RequireTTB);
			AssertEquals(false, requirementIndicator.RequireFWS);
			AssertEquals(true, requirementIndicator.HasEPARequirement);
			AssertEquals(false, requirementIndicator.MayRequireDEA);

			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = "0000000002";
			AssertEquals(true, requirementIndicator.RequireAMS);
			AssertEquals(true, requirementIndicator.RequireATF);
			AssertEquals(true, requirementIndicator.RequireEPA);
			AssertEquals(true, requirementIndicator.RequireNMFS);
			AssertEquals(true, requirementIndicator.RequireTTB);
			AssertEquals(true, requirementIndicator.RequireFWS);
			AssertEquals(true, requirementIndicator.HasEPARequirement);
			AssertEquals(true, requirementIndicator.MayRequireDEA);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.PGA);
			var shbTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff1.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var epaConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, GovernmentAgencyProgramCodeList.Codes.EPA);
			helper.CreateOrGetExistingRefCusConditionValue(epaConditionValueType.PK, condition1.PK, UniversalReferenceConstants.TariffConditionValue.Values.Optional);

			var shbTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff2.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var pgas = new string[] {
				GovernmentAgencyProgramCodeList.Codes.AMS,
				GovernmentAgencyProgramCodeList.Codes.EPA,
				GovernmentAgencyProgramCodeList.NMFS,
				GovernmentAgencyProgramCodeList.Codes.ATF,
				GovernmentAgencyProgramCodeList.Codes.FWS,
				GovernmentAgencyProgramCodeList.Codes.TTB
			};
			foreach (var conditionValueType in pgas)
			{
				var refCusConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, conditionValueType);
				helper.CreateOrGetExistingRefCusConditionValue(refCusConditionValueType.PK, condition2.PK, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);
			}
			var deaConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, GovernmentAgencyProgramCodeList.Codes.DEA);
			helper.CreateOrGetExistingRefCusConditionValue(deaConditionValueType.PK, condition2.PK, UniversalReferenceConstants.TariffConditionValue.Values.Optional);

			Factory.Save();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
