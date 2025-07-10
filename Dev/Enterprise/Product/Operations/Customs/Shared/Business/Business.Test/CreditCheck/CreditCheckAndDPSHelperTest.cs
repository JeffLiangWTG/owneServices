using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CreditCheckAndDPSHelperTest : TestCaseWithFactory
	{
		public void TestRunCheck_CreditChecks()
		{
			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));

			using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = false;

				var declaration1 = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
				declaration1.DPSFreightMovementRestricted = false;
				declaration1.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				declaration1.Importer.MiscServ.OM_ARCreditLimit = 10m;

				var declaration2 = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
				declaration2.DPSFreightMovementRestricted = false;
				declaration2.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				declaration2.Importer.MiscServ.OM_ARCreditLimit = -10m;
				Factory.Save();

				CombineAssertions(() =>
				{
					using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						var result = CreditCheckAndDPSHelper.RunCheck(declaration1, true, "");
						AssertEquals("Importer has credit", true, result.IsAllowedToProceed);
						AssertEquals("no problem, no message", ZString.Empty, result.Message);

						using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CW1"))
						{
							result = CreditCheckAndDPSHelper.RunCheck(declaration2, true, "");
							AssertEquals("Importer does not have credit", false, result.IsAllowedToProceed);
							AssertContains("Over limit message", "above their Credit Limit", result.Message);
							AssertEquals("CW1 Credit Check", false, result.IsExternalSystemUsed);
						}

						using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "EXT"))
						{
							result = CreditCheckAndDPSHelper.RunCheck(declaration2, true, "");
							AssertEquals("Importer does not have credit", false, result.IsAllowedToProceed);
							AssertContains("External System message", "External system request is required to evaluate credit status", result.Message);
							AssertEquals("External Credit Check", true, result.IsExternalSystemUsed);
						}
					}

					using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						var result = CreditCheckAndDPSHelper.RunCheck(declaration2, true, "");
						AssertEquals("Importer does not have credit, but registry not enabled", true, result.IsAllowedToProceed);
						AssertContains("Not enabled, no message", ZString.Empty, result.Message);
					}
				});
			}
		}

		public void TestRunCheck_DeniedPartyScreening()
		{
			var declaration = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
			declaration.DPSFreightMovementRestricted = false;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;

			CombineAssertions(() =>
			{
				var result = CreditCheckAndDPSHelper.RunCheck(declaration, false, "");
				AssertEquals("Denied party screening not restricted", true, result.IsAllowedToProceed);
				AssertEquals(ZString.Empty, result.Message);

				declaration.DPSFreightMovementRestricted = true;
				result = CreditCheckAndDPSHelper.RunCheck(declaration, false, "");
				AssertEquals("Party Denied, cannot send", false, result.IsAllowedToProceed);
				AssertContains("Screening Status is not Clear.", result.Message);
			});
		}

		public void TestProcessOverride()
		{
			using (var overrider = new CreditCheckAndDPSOverrideForTest())
			{
				var bob = Factory.New<GlbStaff>();
				bob.GS_Code = "BOB";
				bob.GS_LoginName = "Bobby";

				var declaration = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
				declaration.DPSFreightMovementRestricted = false;
				declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				var result = CreditCheckAndDPSHelper.RunCheck(declaration, false, "");
				declaration.Logs.RemoveAndDeleteAll();

				CombineAssertions(() =>
				{
					AssertEquals("Pre-Req - Allowed to proceed", true, result.IsAllowedToProceed);
					CreditCheckAndDPSHelper.ProcessOverride(result, declaration, "DPS not enabled");

					AssertEquals("DPS Not enabled", 0, declaration.Logs.Find((x) => x.SL_Reference.Contains("DPS not enabled")).Count());

					declaration.DPSFreightMovementRestricted = true;
					result = CreditCheckAndDPSHelper.RunCheck(declaration, false, "");
					CreditCheckAndDPSHelper.ProcessOverride(result, declaration, "DPS match found");
					AssertEquals("Is not allowed to proceed", false, result.IsAllowedToProceed);
					AssertEquals("DPS enabled but not approved so no log created", 0, declaration.Logs.Find((x) => x.SL_Reference.Contains("DPS match found")).Count());

					overrider.UserCompletesOverride = true;
					CreditCheckAndDPSHelper.ProcessOverride(result, declaration, "DPS match found and allowed to proceced");
					AssertEquals("Is allowed to proceed", true, result.IsAllowedToProceed);
					var log = declaration.Logs.Find((x) => x.SL_Reference.Contains("DPS match found")).FirstOrDefault();
					AssertNotNull("Log created", log);
					AssertEquals("Bob user resolved from log", bob.GS_Code, log.SL_GS_NKUser);
				});
			}
		}

		public void TestCreditCheckResult()
		{
			var creditCheckNoArgs = new CreditCheckResult();

			CombineAssertions(() =>
			{
				AssertEquals("IsAllowrdToProceed", true, creditCheckNoArgs.IsAllowedToProceed);
				AssertEquals("Message", ZString.Empty, creditCheckNoArgs.Message);
				AssertEquals("IsExternalSystemUsed", false, creditCheckNoArgs.IsExternalSystemUsed);
			});
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

	internal class CreditCheckAndDPSOverrideForTest : ICreditCheckAndDPSOverride, IDisposable
	{
		public CreditCheckAndDPSOverrideForTest()
		{
			objFactorySubstitute = ObjectFactory.Substitute<ICreditCheckAndDPSOverride>(this);
		}
		readonly IDisposable objFactorySubstitute;

		public bool UserCompletesOverride { get; set; }

		public void Dispose()
		{
			objFactorySubstitute?.Dispose();
		}

		public void ProcessOverride(SecurityLoginEventArgsForDocumentApproval args, ICreditControlledDocumentDelivery bizObj, ZString caption)
		{
			args.IsAllowedToProceed = UserCompletesOverride;

			if (UserCompletesOverride)
			{
				args.MessageToShowWhenNotAllowed = (NoResString)caption;
				args.AuthorisingStaffLogin = "Bobby";
			}
		}
	}
}
