using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CheckCreditHelper))]
	sealed class CheckCreditHelperTest : TestCaseWithFactory
	{
		public void TestCheckCredit() => CombineAssertions(() =>
		{
			using var temporaryCreditControllerOverrideThresholdValue = CreditCheckHelperSetup.DisposableCreditControllerOverride();
			using var temporaryCreditCheckOnMessageSendValue = CreditCheckHelperSetup.DisposableCreditCheckOnSendOverride(true);

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			declaration1.Importer.MiscServ.OM_ARCreditLimit = -1;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			declaration2.Importer.MiscServ.OM_ARCreditLimit = 9999;
			Factory.Save();

			AssertEquals("Check Credit not Passed", expected: false, declaration1.CheckCredit(out var reasonForNotAllowed1));
			AssertNotEquals("Check Credit not Passed, Reason", string.Empty, reasonForNotAllowed1);

			AssertEquals("Check Credit Passed OK", expected: true, declaration2.CheckCredit(out var reasonForNotAllowed2));
		});

		static class CreditCheckHelperSetup
		{
			public static IDisposable DisposableCreditControllerOverride() => AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateAuthorizationRequirements());

			public static IDisposable DisposableCreditCheckOnSendOverride(bool value) => CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);

			static AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection CreateAuthorizationRequirements() =>
			[
				new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
				{
					Amount = 1,
					Percentage = 0,
					Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo,
					AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly
				},
				new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
				{
					Amount = 1,
					Percentage = 0,
					Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above,
					AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly
				}
			];
		}
	}
}
