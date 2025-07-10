using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultEPaymentReason))]
	sealed class DefaultEPaymentReasonTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestPropertyReadOnlyness()
		{
			var reason = GetBusinessObjectToSerialise() as DefaultEPaymentReason;
			Assert(!reason.ProviderCodeInfo.ReadOnly);
			Assert(!reason.ReasonCodeInfo.ReadOnly);
			Assert(reason.ReasonDescriptionInfo.ReadOnly);
		}

		public void TestReasonCodeForSelectedProvider()
		{
			var australianCompany = Factory.New<GlbCompany>();
			australianCompany.GC_Code = "CM1";
			var sydneyBranch = australianCompany.Branches.AddNew();
			sydneyBranch.GB_Code = "AU1";
			var newZealandCompany = Factory.New<GlbCompany>();
			australianCompany.GC_Code = "CM2";
			var aucklandBranch = newZealandCompany.Branches.AddNew();
			aucklandBranch.GB_Code = "NZ1";
			Factory.Save();

			var systemLevelReasons = new EPaymentReasonCollection();
			var systemLevelReason1 = systemLevelReasons.AddNew();
			systemLevelReason1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			systemLevelReason1.ReasonCode = "SSS";
			systemLevelReason1.ReasonDescription = "My System Level Reason";

			var companyLevelReasons = new EPaymentReasonCollection();
			var companyLevelReason1 = companyLevelReasons.AddNew();
			companyLevelReason1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			companyLevelReason1.ReasonCode = "CCC";
			companyLevelReason1.ReasonDescription = "My Company Level Reason";

			var registryFactory = new BusinessObjectFactory();
			using (AccountingMasterFilesRegistry.Instance.PaymentReasons.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelReasons))
			using (AccountingMasterFilesRegistry.Instance.PaymentReasons.SetTemporaryValue(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyLevelReasons))
			{
				var systemLevelDefaultReason = new DefaultEPaymentReason(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				systemLevelDefaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
				var systemLevelDefaultReasonCodes = systemLevelDefaultReason.ReasonCodeForSelectedProvider;
				AssertEquals(1, systemLevelDefaultReasonCodes.Count);
				Assert(systemLevelDefaultReasonCodes.ContainsCode(systemLevelReason1.ReasonCode));

				var australianCompanyLevelDefaultReason = new DefaultEPaymentReason(new FallbackLevel(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				australianCompanyLevelDefaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
				var australianCompanyLevelDefaultReasonCodes = australianCompanyLevelDefaultReason.ReasonCodeForSelectedProvider;
				AssertEquals(1, australianCompanyLevelDefaultReasonCodes.Count);
				Assert(australianCompanyLevelDefaultReasonCodes.ContainsCode(companyLevelReason1.ReasonCode));

				var sydneyBranchLevelDefaultReason = new DefaultEPaymentReason(new FallbackLevel(australianCompany.PK.ToGuid(), sydneyBranch.PK.ToGuid(), Guid.Empty));
				sydneyBranchLevelDefaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
				var sydneyBranchLevelDefaultReasonCodes = sydneyBranchLevelDefaultReason.ReasonCodeForSelectedProvider;
				AssertEquals(1, sydneyBranchLevelDefaultReasonCodes.Count);
				Assert(sydneyBranchLevelDefaultReasonCodes.ContainsCode(companyLevelReason1.ReasonCode));

				var newZealandCompanyLevelDefaultReason = new DefaultEPaymentReason(new FallbackLevel(newZealandCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				newZealandCompanyLevelDefaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
				var newZealandCompanyLevelDefaultReasonCodes = newZealandCompanyLevelDefaultReason.ReasonCodeForSelectedProvider;
				AssertEquals(1, newZealandCompanyLevelDefaultReasonCodes.Count);
				Assert(newZealandCompanyLevelDefaultReasonCodes.ContainsCode(systemLevelReason1.ReasonCode));

				var aucklandBranchLevelDefaultReason = new DefaultEPaymentReason(new FallbackLevel(newZealandCompany.PK.ToGuid(), aucklandBranch.PK.ToGuid(), Guid.Empty));
				aucklandBranchLevelDefaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
				var aucklandBranchLevelDefaultReasonCodes = aucklandBranchLevelDefaultReason.ReasonCodeForSelectedProvider;
				AssertEquals(1, aucklandBranchLevelDefaultReasonCodes.Count);
				Assert(aucklandBranchLevelDefaultReasonCodes.ContainsCode(systemLevelReason1.ReasonCode));
			}
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var defaultReason = new DefaultEPaymentReason(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			defaultReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			defaultReason.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			return defaultReason;
		}
		#endregion
	}
}
