using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementRecipientRate))]
	sealed class OrgCommissionAgreementRecipientRateTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		#region CAT_CommissionPercentage

		public void TestCAT_CommissionPercentage_ReadOnly()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var rate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = "";
			AssertEquals(true, rate.CAT_CommissionPercentageInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(false, rate.CAT_CommissionPercentageInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(true, rate.CAT_CommissionPercentageInfo.ReadOnly);

			recipient.CAR_CommissionType = "XXX";
			AssertEquals(true, rate.CAT_CommissionPercentageInfo.ReadOnly);
		}

		#endregion

		#region CAT_RX_NKCommissionCurrency

		public void TestCAT_RX_NKCommissionCurrency_ReadOnly()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var rate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = "";
			AssertEquals(true, rate.CAT_RX_NKCommissionCurrencyInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(false, rate.CAT_RX_NKCommissionCurrencyInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(true, rate.CAT_RX_NKCommissionCurrencyInfo.ReadOnly);

			recipient.CAR_CommissionType = "XXX";
			AssertEquals(true, rate.CAT_RX_NKCommissionCurrencyInfo.ReadOnly);
		}

		#endregion

		#region CAT_CommissionAmount

		public void TestCAT_CommissionAmount_ReadOnly()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var rate = recipient.Rates.AddNew();

			recipient.CAR_CommissionType = "";
			AssertEquals(true, rate.CAT_CommissionAmountInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(false, rate.CAT_CommissionAmountInfo.ReadOnly);

			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(true, rate.CAT_CommissionAmountInfo.ReadOnly);

			recipient.CAR_CommissionType = "XXX";
			AssertEquals(true, rate.CAT_CommissionAmountInfo.ReadOnly);
		}

		public void TestCommissionAmountDecimalPlaces()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();
			rate.CAT_RX_NKCommissionCurrency = "";
			AssertEquals(2, rate.CommissionAmountDecimalPlaces);

			rate.CAT_RX_NKCommissionCurrency = "IDR";
			AssertEquals(0, rate.CommissionAmountDecimalPlaces);

			rate.CAT_RX_NKCommissionCurrency = "USD";
			AssertEquals(2, rate.CommissionAmountDecimalPlaces);
		}

		#endregion

		#region CAT_CommissionStartDate

		public void TestCAT_CommissionStartDate()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var customer = Factory.New<OrgHeader>();
			customer.MiscServ.OM_CMClientCommenced = new ZDateTime(2000, 2, 2);
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_OH_Customer = customer.PK;
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(new ZDateTime(2000, 2, 2), rate.CAT_CommissionStartDate);

			rate.CAT_CommissionPeriod = "";
			AssertEquals(new ZDateTime(2000, 2, 2), rate.CAT_CommissionStartDate);
			AssertEquals(new ZDateTime(2000, 2, 2), rate.CAT_CommissionStartDateOverride);

			rate.CAT_CommissionStartDateOverride = new ZDate(2003, 3, 3);
			AssertEquals(new ZDateTime(2003, 3, 3), rate.CAT_CommissionStartDate);

			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(ZDateTime.Empty, rate.CAT_CommissionStartDateOverride);
			AssertEquals(new ZDateTime(2000, 2, 2), rate.CAT_CommissionStartDate);
		}

		public void TestCAT_CommissionStartDate_WithInvalidOpportunityEffectiveDate()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = ZDate.Invalid;

			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(ZDate.Invalid, rate.CAT_CommissionStartDate);
		}

		#endregion

		#region CAT_CommissionEndDate

		public void TestCAT_CommissionEndDate()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var customer = Factory.New<OrgHeader>();
			customer.MiscServ.OM_CMClientCommenced = new ZDateTime(2000, 2, 2);
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_OH_Customer = customer.PK;
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(new ZDateTime(2001, 2, 1), rate.CAT_CommissionEndDate);

			rate.CAT_CommissionPeriod = "";
			AssertEquals(new ZDateTime(2001, 2, 1), rate.CAT_CommissionEndDate);
			AssertEquals(new ZDateTime(2001, 2, 1), rate.CAT_CommissionEndDateOverride);

			rate.CAT_CommissionEndDateOverride = new ZDate(2003, 3, 3);
			AssertEquals(new ZDateTime(2003, 3, 3), rate.CAT_CommissionEndDate);

			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(ZDateTime.Empty, rate.CAT_CommissionEndDateOverride);
			AssertEquals(new ZDateTime(2001, 2, 1), rate.CAT_CommissionEndDate);
		}

		public void TestCAT_CommissionEndDate_WithInvalidOpportunityEffectiveDate()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = ZDate.Invalid;

			var recipient = agreement.Recipients.AddNew();
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionPeriod = "0-12";
			AssertEquals(ZDate.Invalid, rate.CAT_CommissionEndDate);
		}

		#endregion

		#region CommissionDateCovered

		public void TestCommissionDateCovered()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();

			rate.CAT_CommissionStartDate = ZDate.Empty;
			rate.CAT_CommissionEndDate = ZDate.Empty;
			AssertEquals(true, rate.CommissionDateCovered(new ZDate(2002, 2, 2)));

			rate.CAT_CommissionStartDate = ZDate.Empty;
			rate.CAT_CommissionEndDate = new ZDate(2002, 2, 2);
			AssertEquals(true, rate.CommissionDateCovered(new ZDate(2001, 1, 1)));
			AssertEquals(false, rate.CommissionDateCovered(new ZDate(2003, 3, 3)));
			AssertEquals(true, rate.CommissionDateCovered(new ZDate(2002, 2, 2)));

			rate.CAT_CommissionStartDate = new ZDate(2002, 2, 2);
			rate.CAT_CommissionEndDate = ZDate.Empty;
			AssertEquals(false, rate.CommissionDateCovered(new ZDate(2001, 1, 1)));
			AssertEquals(true, rate.CommissionDateCovered(new ZDate(2003, 3, 3)));
			AssertEquals(true, rate.CommissionDateCovered(new ZDate(2002, 2, 2)));
		}

		#endregion

		#endregion

		#region Decimals

		public void TestZDecimalsHaveCorrectDecimalPlacesOrgCommissionAgreementRecipientRate()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();

			var comList = new List<string>
			{
				nameof(rate.CAT_CommissionAmount)
			};

			var percentList = new List<string>
			{
				nameof(rate.CAT_CommissionPercentage)
			};

			var tester = new DecimalPlacesAttributeTester(rate);
			tester.CheckNonLocalCurrency(comList, nameof(rate.CommissionAmountDecimalPlaces), nameof(rate.CAT_RX_NKCommissionCurrency), rate);
			tester.CheckConstant(percentList, nameof(rate.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		#endregion

		#region Draft

		public void TestCreateAndMergeDraft()
		{
			var rate = Factory.New<OrgCommissionAgreementRecipientRate>();
			rate.CAT_CommissionPercentage = 5;

			var rateDraft = rate.CreateDraft();
			rateDraft.CAT_CommissionPercentage = 10;

			rateDraft.MergeDraft();

			AssertEquals((ZDecimal)10, rate.CAT_CommissionPercentage);
		}

		#endregion

		#region Delete

		public void TestCanDelete()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			var rate = recipient.Rates.AddNew();

			Factory.Save();

			AssertEquals(true, rate.CanDelete);

			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = rate.PK;
			((BusinessObject)commissionLine).FillWithValidTestData();
			Factory.Save();

			AssertEquals(false, rate.CanDelete);

			var reloadedRate = new BusinessObjectFactory().Load<OrgCommissionAgreementRecipientRate>(rate.PK);
			AssertEquals(false, reloadedRate.CanDelete);
			AssertEquals("This rate has already been used for a commission pay out.", reloadedRate.ReasonForNotAbleToDelete);

			var draftAgreement = agreement.CreateDraft();
			var draftRecipient = draftAgreement.Recipients.First();
			var draftRate = draftRecipient.Rates.First();
			AssertEquals(false, draftRate.CanDelete);
			AssertEquals("This rate has already been used for a commission pay out.", draftRate.ReasonForNotAbleToDelete);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var recipient = factory.NewWithValidTestData<OrgCommissionAgreementRecipient>();
			var rate = recipient.Rates.AddNew();
			rate.FillWithValidTestData();

			return rate;
		}

		#endregion
	}
}
