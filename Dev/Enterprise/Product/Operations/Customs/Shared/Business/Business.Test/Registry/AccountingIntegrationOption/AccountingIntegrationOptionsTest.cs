using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AccountingIntegrationOptions))]
	sealed class AccountingIntegrationOptionsTest : RegistryBusinessObjectTemplateTestCase<AccountingIntegrationOptions>
	{
		public void TestActions()
		{
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();

			option.EnableAccountingIntegration = true;
			AssertEquals(ChargePosterBehaviours.AutoRateDSB, option.Actions);

			option.ARPostDSB = true;
			AssertEquals(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, option.Actions);

			option.APPostDSB = true;
			AssertEquals(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB, option.Actions);

			option.PreApprovalBillingJob = true;
			AssertEquals(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, option.Actions);
		}

		public void TestWhenEnableAccountingIntegrationChangesToFalse()
		{
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.PreApprovalBillingJob = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			option.CDSCustomsStatusCodes = "01,02,03";
			option.ChiefCustomsStatusCodes = "01,02,03";
			option.EUCustomsStatusCodes = "CLR,CLP,CDA";

			option.EnableAccountingIntegration = false;

			AssertEquals(false, option.PreApprovalBillingJob);
			AssertEquals(false, option.APPostDSB);
			AssertEquals(false, option.ARPostDSB);
			AssertEquals(ZString.Empty, option.CDSCustomsStatusCodes);
			AssertEquals(ZString.Empty, option.ChiefCustomsStatusCodes);
			AssertEquals(ZString.Empty, option.EUCustomsStatusCodes);
		}

		public void TestIsPreApprovalBillJobSupported()
		{
			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			Assert(!option.IsPreApprovalBillingJobSupported());

			AccountingIntegrationOptions optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(usCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			Assert(optionCloned.IsPreApprovalBillingJobSupported());
		}

		public void TestValidateAPPostDSBForUS()
		{
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();

			AccountingIntegrationOptions optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			optionCloned.EnableAccountingIntegration = true;
			optionCloned.APPostDSB = true;
			AssertEquals(false, optionCloned.APPostDSBInfo.HasWarning(AccountingIntegrationOptions.APPostingOnEntryClearanceErrorForUS));

			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(usCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			optionCloned.EnableAccountingIntegration = true;
			optionCloned.APPostDSB = true;
			AssertEquals(true, optionCloned.APPostDSBInfo.HasWarning(AccountingIntegrationOptions.APPostingOnEntryClearanceErrorForUS));
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override AccountingIntegrationOptions GetBusinessObjectToClone()
		{
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.PreApprovalBillingJob = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			return option;
		}

		protected override AccountingIntegrationOptions GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
