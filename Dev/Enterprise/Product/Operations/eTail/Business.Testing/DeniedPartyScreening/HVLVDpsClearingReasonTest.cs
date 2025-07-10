using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(HVLVDpsClearingReason))]
	public class HVLVDpsClearingReasonTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dpsMatches = new HVLVDpsMatch[] {
				new HVLVDpsMatch(CreateDpsResponseWithScreeningParty(), Factory) { Cleared = true, ClearingReason = "OTH", ClearingReasonText = "Test Reason" },
			};

			return new HVLVDpsClearingReason(dpsMatches, Factory);
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new HVLVDpsClearingReason(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), null));
		}

		public void TestConstructor_ShouldShowEmptyCodeAndText()
		{
			var dpsClearingReason = GetNewBusinessObject() as HVLVDpsClearingReason;

			AssertNullOrEmpty(dpsClearingReason.ClearingReason);
			AssertNullOrEmpty(dpsClearingReason.ClearingReasonText);
		}

		public void TestClearingReason()
		{
			var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);

			var resourceStringDataAttribute = dpsClearingReason.ClearingReasonInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Clearing Reason", resourceStringDataAttribute.Caption);

			var listAttribute = dpsClearingReason.ClearingReasonInfo.GetAttribute<ListAttribute>();
			AssertNotNull(listAttribute);
			AssertEquals("ClearingReasonList", listAttribute.ListDataSourceMember);
		}

		public void TestClearingReasonText()
		{
			var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);

			var resourceStringDataAttribute = dpsClearingReason.ClearingReasonTextInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Clearing Reason Text", resourceStringDataAttribute.Caption);
		}

		public void TestClearingReasonSetter_WhenReasonIsEmpty_ReasonTextShouldBeEmptyAndReadonly()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(true);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReason = "OTH";
				dpsClearingReason.ClearingReasonText = "Test Reason";

				dpsClearingReason.ClearingReason = string.Empty;

				AssertNullOrEmpty(dpsClearingReason.ClearingReasonText);
				Assert(dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonSetter_WhenReasonIsOther_ReasonTextShouldBeEmptyAndEditable()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(true);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReasonText = "Test Reason";
				dpsClearingReason.ClearingReason = "OTH";

				AssertNullOrEmpty(dpsClearingReason.ClearingReasonText);
				Assert(!dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonSetter_WhenReasonIsMandatoryAndReasonTextIsNotEmpty_ReasonTextShouldBeFromRegistryAndEditable()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(false);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReason = "C01";

				AssertEquals("Clearing Reason 1", dpsClearingReason.ClearingReasonText);
				Assert(!dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonSetter_WhenReasonIsMandatoryAndReasonTextIsEmpty_ReasonTextShouldBeEmptyAndEditable()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(false);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReason = "C01";

				AssertEquals("Clearing Reason 1", dpsClearingReason.ClearingReasonText);
				Assert(!dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonSetter_WhenReasonIsNotMandatoryAndReasonTextIsNotEmpty_ReasonTextShouldBeFromRegistryAndReadonly()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(false);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReason = "C04";

				AssertEquals(StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString(), dpsClearingReason.ClearingReasonText);
				Assert(dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonSetter_WhenReasonIsNotMandatoryAndReasonTextIsEmpty_ReasonTextShouldBeDefaultValueAndReadonly()
		{
			var requireReasonWrapper = CreateRequireReasonWrapper(true);
			var securityCore = CreateSecurityCore(false);

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				dpsClearingReason.ClearingReason = "C03";

				AssertEquals("Clearing Reason 3", dpsClearingReason.ClearingReasonText);
				Assert(dpsClearingReason.ClearingReasonText_ReadOnly);
			}
		}

		public void TestClearingReasonList_WhenRegistryHasBeenConfigured_ShouldReturnClearingReasonsFromRegistry()
		{
			using (Env.SetTemporarySecurityInstanceForTest(CreateSecurityCore(false)))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper()))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				ClearCachedValue();

				var clearingReasonList = dpsClearingReason.ClearingReasonList;
				AssertNotNull(clearingReasonList);
				AssertEquals(4, clearingReasonList.Count);
				AssertEquals("C01", clearingReasonList[0].Code);
				AssertEquals("C02", clearingReasonList[1].Code);
				AssertEquals("C03", clearingReasonList[2].Code);
				AssertEquals("C04", clearingReasonList[3].Code);
			}
		}

		public void TestClearingReasonList_WhenRegistryHasNotBeenConfigured_ShouldReturnEmptyList()
		{
			using (Env.SetTemporarySecurityInstanceForTest(CreateSecurityCore(false)))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				ClearCachedValue();

				var clearingReasonList = dpsClearingReason.ClearingReasonList;

				AssertNotNull(clearingReasonList);
				AssertEquals(0, clearingReasonList.Count);
			}
		}

		public void TestClearingReasonList_WhenOtherReasonIsAllowed_ShouldIncludeOtherReason()
		{
			using (Env.SetTemporarySecurityInstanceForTest(CreateSecurityCore(true)))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper()))
			{
				var dpsClearingReason = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
				ClearCachedValue();

				var clearingReasonList = dpsClearingReason.ClearingReasonList;

				AssertNotNull(clearingReasonList);
				AssertEquals(5, clearingReasonList.Count);
				AssertEquals("C01", clearingReasonList[0].Code);
				AssertEquals("C02", clearingReasonList[1].Code);
				AssertEquals("C03", clearingReasonList[2].Code);
				AssertEquals("C04", clearingReasonList[3].Code);
				AssertEquals(RequireReasonForCLRRegistryConstants.Code.Other, clearingReasonList[4].Code);
			}
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "EFG", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = ScreeningNameTypes.Person },
			};

			return new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
		}

		RequireReasonForCLRWrapper CreateRequireReasonWrapper(bool required = false)
		{
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "C01", Title = "Title 1", ClearingReason = "Clearing Reason 1", IsMandatory = true },
				new RequireReasonForCLRItem { Code = "C02", Title = "Title 2", ClearingReason = string.Empty, IsMandatory = true },
				new RequireReasonForCLRItem { Code = "C03", Title = "Title 3", ClearingReason = "Clearing Reason 3", IsMandatory = false },
				new RequireReasonForCLRItem { Code = "C04", Title = "Title 4", ClearingReason = string.Empty, IsMandatory = false }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = required
			};

			return requireReasonWrapper;
		}

		SecurityCore CreateSecurityCore(bool allowOtherReason)
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK)
			{
				OrgDeniedPartyScreeningAllowOtherReason = { IsAllowed = allowOtherReason }
			};
		}

		void ClearCachedValue()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>("Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason|ClearingReasonList");
		}
	}
}
