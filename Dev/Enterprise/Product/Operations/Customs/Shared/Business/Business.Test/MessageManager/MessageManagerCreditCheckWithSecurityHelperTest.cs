using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageManagerCreditCheckWithSecurityHelperTest : TestCaseWithFactory
	{
		public void TestIsCreditCheckOKToSend()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var declaration = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
				declaration.DPSFreightMovementRestricted = false;
				declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				declaration.Importer.MiscServ.OM_ARCreditLimit = -1M;
				Factory.Save();

				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = false;
				AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
				collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
				collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));

				using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				using (var overrider = new CreditCheckAndDPSOverrideForTest())
				{
					using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);

						AssertEquals("User has no rights but registry is disabled", true, helper.IsCreditCheckOKToSend);
						AssertEquals(null, helper.ReasonForNotAllowed);
					}

					using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
						AssertEquals("User has no rights and registry enabled, cannot send", false, helper.IsCreditCheckOKToSend);
						AssertContains("Is at or above their Credit Limit", helper.ReasonForNotAllowed);

						overrider.UserCompletesOverride = true;
						AssertEquals("User has no rights and registry enabled, but supervisor overrides", true, helper.IsCreditCheckOKToSend);
					}
				}
			}
		}

		public void TestIsDeniedPartyOKToSend()
		{
			var declaration = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
			declaration.DPSFreightMovementRestricted = false;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Env.Security.ReceivablesOnCreditHoldController.IsAllowed = true;

			using (var overrider = new CreditCheckAndDPSOverrideForTest())
			{
				var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				AssertEquals("DPS is disabled", true, helper.IsDeniedPartyOKToSend);
				AssertEquals(null, helper.ReasonForNotAllowed);

				declaration.DPSFreightMovementRestricted = true;
				AssertEquals("DPS is enabled", false, helper.IsDeniedPartyOKToSend);
				AssertContains("Screening Status is not Clear.", helper.ReasonForNotAllowed);

				using (Globals.SetIsUserInteractiveForTest(true))
				{
					overrider.UserCompletesOverride = true;
					Assert("User has no rights and registry enabled, but supervisor overrides", helper.IsDeniedPartyOKToSend);
				}
			}
		}

		public void TestOverridingPreCreditCheckConditions()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithAccIntegrationSupport>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;

			CombineAssertions(() =>
			{
				var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				AssertEquals("With DefaultAdditional Check", false, helper.IsCreditCheckOKToSend);
				AssertStartsWith("With DefaultAdditional Check", @"Auto-rating of Customs Disbursement failed:

There was a system error while attempting billing. Please check the record. See below for more information.
Object reference not set to an instance of an object.", helper.ReasonForNotAllowed);
				CargoWise.Common.ErrorReporter.Clear();

				helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				helper.SetAdditionalConditionPreCreditCheck(null);
				AssertEquals("Without DefaultAdditional Check", true, helper.IsCreditCheckOKToSend);
				AssertEquals("Without DefaultAdditional Check", null, helper.ReasonForNotAllowed);

				helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				helper.SetAdditionalConditionPreCreditCheck((BaseJobDeclaration dec) => { return "JustFail"; });
				AssertEquals("Without DefaultAdditional Check", false, helper.IsCreditCheckOKToSend);
				AssertEquals("Without DefaultAdditional Check", "JustFail", helper.ReasonForNotAllowed);

				var hit1 = false;
				var hit2 = false;
				var hit3 = false;
				helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
				helper.SetAdditionalConditionPreCreditCheck(
					(BaseJobDeclaration dec) => { hit1 = true; return ""; },
					(BaseJobDeclaration dec) => { hit2 = true; return "JustFailX"; },
					(BaseJobDeclaration dec) => { hit3 = true; return "JustFailY"; }
					);
				AssertEquals("Without DefaultAdditional Check", false, helper.IsCreditCheckOKToSend);
				AssertEquals("Without DefaultAdditional Check", "JustFailX", helper.ReasonForNotAllowed);
				AssertEquals(true, hit1);
				AssertEquals(true, hit2);
				AssertEquals(false, hit3);
			});
		}

		public void TestFactoryInAccIntegrationDataProviderForPreCreditCheck()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithAccIntegrationSupport>();
			var dataProvider = (IAccIntegrationDataProvider)new JobDeclarationIAccIntegrationDataProviderForPreCreditCheck(declaration);
			AssertNotEquals(declaration.Factory, dataProvider.Factory);

			using (declaration.MarkDeclarationIsValidatingCustomsMessaging())
			{
				dataProvider = new JobDeclarationIAccIntegrationDataProviderForPreCreditCheck(declaration);
				AssertEquals(declaration.Factory, dataProvider.Factory);
			}
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}
	}
}
