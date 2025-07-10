using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeProspectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation()
		{
			AssertInfo(TradeProspect.PAP_IncoTradeTermInfo, true, "FOB");
			AssertInfo(TradeProspect.PAP_RH_NKCommodityCodeInfo, true, "GEN");
			AssertInfo(TradeProspect.PAP_RS_NKServiceLevelInfo, true, "STD");
			AssertInfo(TradeProspect.PAP_DensityInfo, true, TradeProspect.Lookups.Densities[0].Code);
		}

		public void TestCheckPAP_IncoTradeTerm_Expired()
		{
			TradeProspect.PAP_IncoTradeTerm = Core.Constants.IncoTerms.DeliveredAtTerminal;
			AssertEquals("Incoterm 'DAT' should have warning", true, TradeProspect.PAP_IncoTradeTermInfo.HasWarning("This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."));
		}

		public void TestCheckPAP_RC_NKContainer()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "YYY";
			var org = Factory.New<OrgHeader>();

			TradeProspect.PAP_RC_NKContainer = "XXX";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_RC_NKContainerInfo, true);

			TradeProspect.PAP_RC_NKContainer = "YYY";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_RC_NKContainerInfo, false);
		}

		public void TestCheckPAP_RecurrenceType()
		{
			TradeProspect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			AssertMandatoryValidationError(TradeProspect.PAP_RecurrenceTypeInfo, false);
			AssertListValidationInvalidCodeError(TradeProspect.PAP_RecurrenceTypeInfo, false);

			TradeProspect.PAP_RecurrenceType = "";
			AssertMandatoryValidationError(TradeProspect.PAP_RecurrenceTypeInfo, true);
			AssertListValidationInvalidCodeError(TradeProspect.PAP_RecurrenceTypeInfo, false);

			TradeProspect.PAP_RecurrenceType = "XX";
			AssertMandatoryValidationError(TradeProspect.PAP_RecurrenceTypeInfo, false);
			AssertListValidationInvalidCodeError(TradeProspect.PAP_RecurrenceTypeInfo, true);
		}

		public void TestCheckPAP_OH_Competitor()
		{
			var org = Factory.New<OrgHeader>();

			TradeProspect.PAP_OH_Competitor = org.PK;
			AssertHasError(TradeProspect.PAP_OH_CompetitorInfo, "Enter a valid " + TradeProspect.PAP_OH_CompetitorInfo.Description + ".");

			TradeProspect.PAP_OH_Competitor = ZGuid.Empty;
			AssertNoErrors(TradeProspect.PAP_OH_CompetitorInfo);

			org.OH_IsCompetitor = true;
			TradeProspect.PAP_OH_Competitor = org.PK;
			AssertNoErrors(TradeProspect.PAP_OH_CompetitorInfo);
		}

		public void TestCheckPAP_OH_ControllingAgent()
		{
			var org = Factory.New<OrgHeader>();

			TradeProspect.PAP_OH_ControllingAgent = org.PK;
			AssertNoErrors(TradeProspect.PAP_OH_ControllingAgentInfo);

			TradeProspect.PAP_OH_ControllingAgent = ZGuid.Invalid;
			AssertHasError(TradeProspect.PAP_OH_ControllingAgentInfo, "Enter a valid " + TradeProspect.PAP_OH_ControllingAgentInfo.Description + ".");

			TradeProspect.PAP_OH_ControllingAgent = ZGuid.Empty;
			AssertNoErrors(TradeProspect.PAP_OH_ControllingAgentInfo);
		}

		public void TestCheckPA_OH_ServiceProvider()
		{
			var org = Factory.New<OrgHeader>();

			TradeProspect.PAP_OH_ServiceProvider = org.PK;
			AssertHasError(TradeProspect.PAP_OH_ServiceProviderInfo, "Enter a valid " + TradeProspect.PAP_OH_ServiceProviderInfo.Description + ".");

			TradeProspect.PAP_OH_ServiceProvider = ZGuid.Empty;
			AssertNoErrors(TradeProspect.PAP_OH_ServiceProviderInfo);

			org.OH_IsShippingProvider = true;
			TradeProspect.PAP_OH_ServiceProvider = org.PK;
			AssertNoErrors(TradeProspect.PAP_OH_ServiceProviderInfo);
		}

		public void TestCheckPAP_PeriodOfActivity_WithCustomPeriodOfActivity()
		{
			var periodOfActivityTypes = new CodeDescriptionWithEnabledAndDefaultCollection();
			periodOfActivityTypes.AddNew("WIN", (NoResString)"Winter", true, true);
			periodOfActivityTypes.AddNew("SUM", (NoResString)"Summer", false, false);
			OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodOfActivityTypes);

			TradeProspect.IsPeriodOfActivityOverridden = true;

			TradeProspect.PAP_PeriodOfActivity = "";
			TradeProspect.Validation.ValidatePAP_PeriodOfActivity();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, true);

			TradeProspect.PAP_PeriodOfActivity = "XXX";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, true);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, false);

			TradeProspect.PAP_PeriodOfActivity = "WIN";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, false);

			TradeProspect.PAP_PeriodOfActivity = "SUM";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, true);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, false);

			Factory.Save();

			TradeProspect.Validation.ValidatePAP_PeriodOfActivity();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, false);
			AssertHasWarning(TradeProspect.PAP_PeriodOfActivityInfo, ListValidation.InactiveCodeMessage);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, false);
		}

		public void TestCheckPAP_PeriodOfActivity_WithoutCustomPeriodOfActivity()
		{
			TradeProspect.IsPeriodOfActivityOverridden = false;

			TradeProspect.PAP_PeriodOfActivity = "";
			TradeProspect.Validation.ValidatePAP_PeriodOfActivity();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_PeriodOfActivityInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_PeriodOfActivityInfo, false);
		}

		public void TestCheckPAP_IndustryVertical_WithCustomIndustryVertical()
		{
			var industryVerticalTypes = new CodeDescriptionBoolCollection();
			industryVerticalTypes.Add("CAR", (NoResString)"Cars", true);
			industryVerticalTypes.Add("TRU", (NoResString)"Trucks", false);
			OrganisationsDataRegistry.Instance.IndustryVerticalTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, industryVerticalTypes);

			TradeProspect.IsIndustryVerticalOverridden = true;

			TradeProspect.PAP_IndustryVertical = "";
			TradeProspect.Validation.ValidatePAP_IndustryVertical();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, true);

			TradeProspect.PAP_IndustryVertical = "XXX";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, true);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, false);

			TradeProspect.PAP_IndustryVertical = "CAR";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, false);

			TradeProspect.PAP_IndustryVertical = "TRU";
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, true);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, false);

			Factory.Save();

			TradeProspect.Validation.ValidatePAP_IndustryVertical();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, false);
			AssertHasWarning(TradeProspect.PAP_IndustryVerticalInfo, ListValidation.InactiveCodeMessage);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, false);
		}

		public void TestCheckPAP_IndustryVertical_WithoutCustomIndustryVertical()
		{
			TradeProspect.IsIndustryVerticalOverridden = false;

			TradeProspect.PAP_IndustryVertical = "";
			TradeProspect.Validation.ValidatePAP_IndustryVertical();
			AssertListValidationInvalidCodeError(TradeProspect.PAP_IndustryVerticalInfo, false);
			AssertMandatoryValidationError(TradeProspect.PAP_IndustryVerticalInfo, false);
		}

		public void TestCheckPAP_ExpectedTradeStartDate()
		{
			TradeProspect.TradeDetail.PA_Status = OpportunityTradeStatus.Codes.Active;
			TradeProspect.Validation.ValidatePAP_ExpectedTradeStartDate();
			AssertNoErrors(TradeProspect.PAP_ExpectedTradeStartDateInfo);

			TradeProspect.TradeDetail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			TradeProspect.Validation.ValidatePAP_ExpectedTradeStartDate();
			AssertMandatoryValidationError(TradeProspect.PAP_ExpectedTradeStartDateInfo, true);

			TradeProspect.PAP_ExpectedTradeStartDate = ZDate.Today;
			AssertMandatoryValidationError(TradeProspect.PAP_ExpectedTradeStartDateInfo, false);
		}

		OrgTradeProspect TradeProspect
		{
			get
			{
				if (tradeProspect == null)
				{
					var sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
					tradeProspect = sales.TradeDetails.AddNew().ProspectDetail;
				}
				return tradeProspect;
			}
		}
		OrgTradeProspect tradeProspect;

		void AssertInfo(ZPropertyInfo info, bool emptyEnabled, ZString validValue)
		{
			info.Value = (ZString)"##";
			Assert(info.HasError("Enter a valid selection.") || info.HasError("Enter a valid code.") || info.HasError(String.Format("Enter a valid {0}.", info.Description)));

			info.Value = ZString.Empty;
			if (emptyEnabled)
			{
				AssertNoErrors(info);
			}
			else
			{
				Assert(info.HasError("Please enter a value.") || info.HasError(String.Format("Please enter a {0}.", info.Description)));
			}

			info.Value = validValue;
			AssertNoErrors(info);
		}
	}
}
