using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingGlbStaffMaintenanceTest : PatternMatchingSourceTest<GlbStaff, PatternMatchingGlbStaffMaintenance>
	{
		protected override GlbStaff BizO => staff ?? (staff = Factory.NewWithValidTestData<GlbStaff>());
		GlbStaff staff;

		protected override PatternMatchingGlbStaffMaintenance MaintenanceObject => new PatternMatchingGlbStaffMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueEmailToHash = "edward@wisetechglobal.com".ToUpperInvariant();
			var valueNameToHash = "Edward Mills".ToUpperInvariant();
			var valueAddressToHash = (BizO.GS_UserAddress1 + BizO.GS_UserAddress2 + BizO.GS_City + BizO.GS_Postcode + BizO.GS_State).ToUpperInvariant();

			var patternEmail = patternMatchingEmails[0];
			var patternName = patternMatchingNames[0];
			var patternAddress = patternMatchingAddresses[0];
			var patternRegcode = patternMatchingRegCodes[0];

			AssertEquals(1, patternMatchingAddresses.Length);
			AssertEquals(0, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(4, patternMatchingPhones.Length);
			AssertEquals(1, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(staff.PK, patternEmail.PME_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(staff.PK, patternName.PMN_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueAddressToHash), patternAddress.PMA_HashedValue);
			AssertEquals(staff.PK, patternAddress.PMA_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast("1"), patternRegcode.PMR_HashedValue);
			AssertEquals(staff.PK, patternRegcode.PMR_ParentId);

			AssertContainsExactElementsInAnyOrder(
				new ZInt[]
				{
					TextStandardizerHelper.ComputeStringHashFast("61449743938"),
					TextStandardizerHelper.ComputeStringHashFast("2889232"),
					TextStandardizerHelper.ComputeStringHashFast("612690138438"),
					TextStandardizerHelper.ComputeStringHashFast("613038838992")
				},
				Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK))
					.Select(p => p.PMP_HashedValue).ToArray());
			AssertEquals(staff.PK, patternMatchingPhones[0].PMP_ParentId);
		}

		public void TestAccessingCountryCode_WithoutAccessRights_ShouldNotShowException()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			var targetUser = Factory.NewWithValidTestData<GlbStaff>();

			regularUser.GS_RN_NKCountryCode = "AU";
			targetUser.GS_RN_NKCountryCode = "NZ";
			targetUser.GS_FullName = "Peter Luis";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(regularUser.GS_LoginName))
			{
				var patternMatchMaintenance = new PatternMatchingGlbStaffMaintenance(targetUser);
				ExceptionReporterTestListener.Instance.Clear();
				CombineAssertions(() =>
				{
					AssertEquals("Precondition: No errors reported", 0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("Precondition: Staff shouldn't be able to view by default", "** View Denied due to Security Access **", targetUser.GS_RN_NKCountryCode);
					AssertEquals("Precondition: We can bypass security", "NZ", typeof(GlbStaff).GetProperty("GS_RN_NKCountryCodeInternal",
						(BindingFlags.NonPublic | BindingFlags.Instance)).GetValue(targetUser));
					AssertEquals("Precondition: Regular user should not be admin of target staff", false, targetUser.IsCurrentUserLocalAdminForThisStaff);
					AssertEquals("Precondition: Security checkpoint should not be allowed", false, Env.Security.StaffViewHomeAddressDetails.IsAllowed);
				});

				try
				{
					patternMatchMaintenance.CreateOrUpdatePatternMatchingTables();
				}
				finally
				{
					AssertEquals("Error should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
				}
			}
		}

		public void TestAccessingAddressInformation_WithoutAccessRights_ProcessCorrectly()
		{
			var userWithoutSecurity = Factory.NewWithValidTestData<GlbStaff>();
			var targetUser = Factory.NewWithValidTestData<GlbStaff>();
			targetUser.GS_UserAddress1 = "User Address 123";
			targetUser.GS_UserAddress2 = "User Address 234";
			targetUser.GS_City = "SYD";
			targetUser.GS_State = "DUMMY";
			targetUser.GS_RN_NKCountryCode = "AU";
			targetUser.GS_Postcode = "ABC";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(userWithoutSecurity.GS_LoginName))
			{
				var patternMatchMaintenance = new PatternMatchingGlbStaffMaintenance(targetUser);
				AssertEquals("Precondition: Security checkpoint should not be allowed", false, Env.Security.StaffViewHomeAddressDetails.IsAllowed);
				AssertEquals(true, patternMatchMaintenance.CreateOrUpdatePatternMatchingTables());

				var patternMatchingAddress = Factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, targetUser.PK));
				CombineAssertions(() =>
				{
					AssertNotNull(patternMatchingAddress);
					AssertEquals(TextStandardizerHelper.ComputeStringHashFast("USER ADDRESS 123USER ADDRESS 234SYDABCDUMMY"), patternMatchingAddress.PMA_HashedValue);
					AssertEquals("AU", patternMatchingAddress.PMA_RN_NKCountryCode);
				});
			}
		}

		public void TestShouldNotThrowException_QueueStaffWithoutPersonForDuplication()
		{
			var regularUser = Factory.NewWithValidTestData<GlbStaff>();
			var targetUser = Factory.NewWithValidTestData<GlbStaff>();

			regularUser.GS_RN_NKCountryCode = "AU";
			targetUser.GS_RN_NKCountryCode = "NZ";
			targetUser.GS_FullName = "Peter Luis";

			Factory.Save();
			targetUser.GS_PER = Guid.Empty;

			using (CurrentUserChanger.SwitchToNewUserTemporarily(regularUser.GS_LoginName))
			{
				var patternMatchMaintenance = new PatternMatchingGlbStaffMaintenance(targetUser);

				AssertNoExceptionThrown(() => patternMatchMaintenance.CreateOrUpdatePatternMatchingTables());
			}
		}

		protected override void SetupBizO()
		{
			BizO.GS_EmailAddress = "edward@wisetechglobal.com";
			BizO.GS_MobilePhone = "+61449743938";
			BizO.GS_FaxNum = "2889232";
			BizO.GS_WorkPhone = "+612690138438";
			BizO.GS_HomePhone = "+613038838992";
			BizO.GS_FullName = "Edward Mills";

			BizO.GS_UserAddress1 = "72 O'Riordan Street";
			BizO.GS_UserAddress2 = "";
			BizO.GS_City = "SYDNEY";
			BizO.GS_Postcode = "2015";
			BizO.GS_State = "NSW";

			BizO.GS_Birthdate = new ZDate(1753, 1, 2);

			Factory.Save();
		}
	}
}
