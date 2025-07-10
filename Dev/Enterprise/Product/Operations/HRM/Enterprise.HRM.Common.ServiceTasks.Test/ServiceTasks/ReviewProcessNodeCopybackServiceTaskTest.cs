using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.HRM.Common;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	[UseSnapshotProtection(skipTransaction: true)]
	[TestedType(typeof(ReviewProcessNodeCopybackServiceTask))]
	class ReviewProcessNodeCopybackServiceTaskTest : ServiceTaskTestCase<ReviewProcessNodeCopybackServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
			=> new[] { new TaskNudgeInformationForTest("ReviewProcessNode", "Ready to Submit Review Node Proposals", new[] { ReviewProcessNodeSchema.Constants.RRN_Status + "=APP" }) };

		#region Remuneration

		public void TestSingleProposal_NoCurrentRemunerationFound()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(0, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			AssertExceptionThrown<InvalidOperationException>("Not Found StaffRemuneration For Staff: " + staff.GS_Code, () => task.RunTask());
		}

		public void TestSingleProposal_CreateNewRem_CurrentRemunerationFoundNoPackageItems()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });

			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package);
		}

		public void TestSingleProposal_OmmittingZeroPackageItems()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });

			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			var ent1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent1.GSI_GSR_Remuneration = rem.PK;
			ent1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent1.GSI_Value = 100000;

			var ent2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent2.GSI_GSR_Remuneration = rem.PK;
			ent2.GSI_EntitlementCode = "PBC"; //Performance Bonus Cash
			ent2.GSI_Value = 0;

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package);
			AssertEquals(null, package.FirstOrDefault(x => x.GSI_EntitlementCode == "PBC"));
		}

		public void TestSingleProposal_CopyOverReviewedTypeHadZeroValue()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });

			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			var ent = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent.GSI_GSR_Remuneration = rem.PK;
			ent.GSI_EntitlementCode = "RME"; //Remuneration Equity
			ent.GSI_Value = 0;

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package);
		}

		public void TestSingleProposal_CurrentPackageItemNotInReviewShouldCarriedOver()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			SetupRemEntitlements(new[] { staff });

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem3.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem3.GSI_Value = 50000;

			AssertEquals(3, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2, expectedPackageItem3 }, package);
		}

		public void TestSingleProposal_ReviewedItemNotInCurrentPackageShouldBeIgnored()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			var ent1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent1.GSI_GSR_Remuneration = rem.PK;
			ent1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent1.GSI_Value = 100000;

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package);
		}

		public void TestSingleProposal_ReviewedItemNotInCurrentPackageShouldBeIgnored2()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			var ent1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent1.GSI_GSR_Remuneration = rem.PK;
			ent1.GSI_EntitlementCode = "BAS"; //Remuneration Equity
			ent1.GSI_Value = 100000;

			var ent2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent2.GSI_GSR_Remuneration = rem.PK;
			ent2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			ent2.GSI_Value = 0;

			Factory.Save();

			var remunerations = LoadRemunerations(staff);
			AssertEquals(1, remunerations.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations = LoadRemunerations(staff);
			AssertEquals(2, remunerations.Length);

			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package);
		}

		public void TestSingleProposal_NewRemPackageHasCorrectEffectiveDate_IsNotInPrevPackage()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);
			rem.GSR_FullTimeEquivalent = 0.9;
			rem.GSR_LeaveLiabilityHourlyRate = 5;
			rem.GSR_RN_NKCountry = "NZ";
			rem.GSR_RX_NKCurrency = "NZD";

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration = LoadRemunerations(staff).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedRemuneration = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			expectedRemuneration.GSR_GS_Staff = staff.PK;

			expectedRemuneration.GSR_EffectiveDate = EffectiveDateInStaffTimeZone.AddYears(-2);
			expectedRemuneration.GSR_FullTimeEquivalent = 0.9;
			expectedRemuneration.GSR_LeaveLiabilityHourlyRate = 5;
			expectedRemuneration.GSR_RN_NKCountry = "NZ";
			expectedRemuneration.GSR_RX_NKCurrency = "NZD";

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;
			//Shoule be review process effective date
			expectedPackageItem1.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;
			//Shoule be review process effective date
			expectedPackageItem2.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			AssertEquals(2, package.Length);
			AssertRemuneration(expectedRemuneration, latestRemuneration);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package, isGrantDate: true);
		}

		public void TestSingleProposal_NewRemPackageHasCorrectEffectiveDate_IsInPrevPackageAndChanged()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			SetupRemEntitlements(new[] { staff }, withEntitlementNotInReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration = LoadRemunerations(staff).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedRemuneration = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			expectedRemuneration.GSR_GS_Staff = staff.PK;

			expectedRemuneration.GSR_EffectiveDate = EffectiveDate.AddYears(-2);
			expectedRemuneration.GSR_FullTimeEquivalent = 1;
			expectedRemuneration.GSR_LeaveLiabilityHourlyRate = 5;
			expectedRemuneration.GSR_RN_NKCountry = "NZ";
			expectedRemuneration.GSR_RX_NKCurrency = "NZD";

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;
			//Shoule be review process effective date
			expectedPackageItem1.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;
			//Shoule be review process effective date
			expectedPackageItem2.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package, isGrantDate: true);
		}

		public void TestSingleProposal_NewRemPackageHasCorrectEffectiveDate_IsInPrevPackageAndUnChanged()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "WRK";
			process.RPR_ConfigType = "D";
			process.RPR_EffectiveDate = EffectiveDateOnly.AddYears(-2);

			var node = Factory.NewWithValidTestData<ReviewProcessNode>();
			node.RRN_RPR_ReviewProcess = process.PK;
			node.RRN_GS_Reviewer = manager.PK;
			node.RRN_Status = "APP"; //Approved

			var proposal = Factory.NewWithValidTestData<ReviewProposal>();
			proposal.RRP_RRN_ReviewNode = node.PK;
			proposal.RRP_GS_Staff = staff.PK;

			var entitlement1 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
			entitlement1.RRE_RRP_Proposal = proposal.PK;
			entitlement1.RRE_EntitlementCode = "BAS"; //Total Base Salary
			entitlement1.RRE_Value = 300000;

			var entitlement2 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
			entitlement2.RRE_RRP_Proposal = proposal.PK;
			entitlement2.RRE_EntitlementCode = "RME"; //Remuneration Equity
			entitlement2.RRE_Value = 30000;

			SetupRemEntitlements(new[] { staff }, withEntitlementNotInReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration = LoadRemunerations(staff).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 300000;
			//Shoule be previous Entitlement Grant Date
			expectedPackageItem1.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(4);

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 30000;
			//Shoule be review process effective date
			expectedPackageItem2.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(5);

			AssertEquals(2, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package, isGrantDate: true);
		}

		public void TestSingleProposal_NewRemPackageHasCorrectEffectiveDate_IsInPrevPackageNotInReview()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });
			SetupRemEntitlements(new[] { staff });

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration = LoadRemunerations(staff).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;
			expectedPackageItem1.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;
			expectedPackageItem2.GSI_GrantDate = EffectiveDateOnly.AddYears(-2);

			//13M was in the previous package but was not reviewed.
			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem3.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem3.GSI_Value = 50000;
			//Copy over the existing entitlement grant date.
			expectedPackageItem3.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(6);

			AssertEquals(3, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2, expectedPackageItem3 }, package, isGrantDate: true);
		}

		public void TestMultipleTopNodeProposals()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff1 }, new[] { manager });
			SetupHierarchyProposals(new[] { staff2 }, new[] { manager });
			SetupHierarchyProposals(new[] { staff3 }, new[] { manager });

			var rem1 = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem1.GSR_GS_Staff = staff1.PK;
			rem1.GSR_EffectiveDate = EffectiveDate.AddYears(-4);

			var ent1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent1.GSI_GSR_Remuneration = rem1.PK;
			ent1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent1.GSI_Value = 100000;

			var rem2 = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem2.GSR_GS_Staff = staff2.PK;
			rem2.GSR_EffectiveDate = EffectiveDate.AddYears(-5);

			var ent2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent2.GSI_GSR_Remuneration = rem2.PK;
			ent2.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent2.GSI_Value = 300000;

			var rem3 = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem3.GSR_GS_Staff = staff3.PK;
			rem3.GSR_EffectiveDate = EffectiveDate.AddYears(-6);

			var ent3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent3.GSI_GSR_Remuneration = rem3.PK;
			ent3.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent3.GSI_Value = 400000;

			Factory.Save();

			var remunerations1 = LoadRemunerations(staff1);
			var remunerations2 = LoadRemunerations(staff2);
			var remunerations3 = LoadRemunerations(staff3);
			AssertEquals(1, remunerations1.Length);
			AssertEquals(1, remunerations2.Length);
			AssertEquals(1, remunerations3.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			remunerations1 = LoadRemunerations(staff1);
			remunerations2 = LoadRemunerations(staff2);
			remunerations3 = LoadRemunerations(staff3);
			AssertEquals(2, remunerations1.Length);
			AssertEquals(2, remunerations2.Length);
			AssertEquals(2, remunerations3.Length);

			var latestRemuneration1 = remunerations1.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration2 = remunerations2.OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration3 = remunerations3.OrderByDescending(x => x.GSR_EffectiveDate).First();

			var package1 = LoadEntitlements(latestRemuneration1);
			var package2 = LoadEntitlements(latestRemuneration2);
			var package3 = LoadEntitlements(latestRemuneration3);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem3.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem3.GSI_Value = 200000;

			var expectedPackageItem4 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem4.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem4.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem4.GSI_Value = 20000;

			var expectedPackageItem5 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem5.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem5.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem5.GSI_Value = 200000;

			var expectedPackageItem6 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem6.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem6.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem6.GSI_Value = 20000;

			AssertEquals(2, package1.Length);
			AssertEquals(2, package2.Length);
			AssertEquals(2, package3.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2 }, package1);
			AssertPackage(new[] { expectedPackageItem3, expectedPackageItem4 }, package2);
			AssertPackage(new[] { expectedPackageItem5, expectedPackageItem6 }, package3);
		}

		public void TestProposalsWithSingleHierarchy()
		{
			var managers = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var staffs = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();

			SetupHierarchyProposals(staffs, managers);
			SetupRemEntitlements(staffs);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration1 = LoadRemunerations(staffs[0]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration2 = LoadRemunerations(staffs[1]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration3 = LoadRemunerations(staffs[2]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration4 = LoadRemunerations(staffs[3]).OrderByDescending(x => x.GSR_EffectiveDate).First();

			var package1 = LoadEntitlements(latestRemuneration1);
			var package2 = LoadEntitlements(latestRemuneration2);
			var package3 = LoadEntitlements(latestRemuneration3);
			var package4 = LoadEntitlements(latestRemuneration4);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem3.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem3.GSI_Value = 50000;

			var expectedPackageItem4 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem4.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem4.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem4.GSI_Value = 200001;

			var expectedPackageItem5 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem5.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem5.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem5.GSI_Value = 20001;

			var expectedPackageItem6 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem6.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem6.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem6.GSI_Value = 50000;

			var expectedPackageItem7 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem7.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem7.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem7.GSI_Value = 200002;

			var expectedPackageItem8 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem8.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem8.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem8.GSI_Value = 20002;

			var expectedPackageItem9 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem9.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem9.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem9.GSI_Value = 50000;

			var expectedPackageItem10 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem10.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem10.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem10.GSI_Value = 200003;

			var expectedPackageItem11 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem11.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem11.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem11.GSI_Value = 20003;

			var expectedPackageItem12 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem12.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem12.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem12.GSI_Value = 50000;

			AssertEquals(3, package1.Length);
			AssertEquals(3, package2.Length);
			AssertEquals(3, package3.Length);
			AssertEquals(3, package4.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2, expectedPackageItem3 }, package1);
			AssertPackage(new[] { expectedPackageItem4, expectedPackageItem5, expectedPackageItem6 }, package2);
			AssertPackage(new[] { expectedPackageItem7, expectedPackageItem8, expectedPackageItem9 }, package3);
			AssertPackage(new[] { expectedPackageItem10, expectedPackageItem11, expectedPackageItem12 }, package4);
		}

		public void TestProposalsWithMultipleHierarchys()
		{
			var managers1 = Enumerable.Range(0, 2).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var managers2 = Enumerable.Range(0, 2).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var staffs1 = Enumerable.Range(0, 2).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var staffs2 = Enumerable.Range(0, 2).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();

			SetupHierarchyProposals(staffs1, managers1);
			SetupHierarchyProposals(staffs2, managers2);
			SetupRemEntitlements(staffs1);
			SetupRemEntitlements(staffs2);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var latestRemuneration1 = LoadRemunerations(staffs1[0]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration2 = LoadRemunerations(staffs1[1]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration3 = LoadRemunerations(staffs2[0]).OrderByDescending(x => x.GSR_EffectiveDate).First();
			var latestRemuneration4 = LoadRemunerations(staffs2[1]).OrderByDescending(x => x.GSR_EffectiveDate).First();

			var package1 = LoadEntitlements(latestRemuneration1);
			var package2 = LoadEntitlements(latestRemuneration2);
			var package3 = LoadEntitlements(latestRemuneration3);
			var package4 = LoadEntitlements(latestRemuneration4);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration1.PK;
			expectedPackageItem3.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem3.GSI_Value = 50000;

			var expectedPackageItem4 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem4.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem4.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem4.GSI_Value = 200001;

			var expectedPackageItem5 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem5.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem5.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem5.GSI_Value = 20001;

			var expectedPackageItem6 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem6.GSI_GSR_Remuneration = latestRemuneration2.PK;
			expectedPackageItem6.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem6.GSI_Value = 50000;

			var expectedPackageItem7 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem7.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem7.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem7.GSI_Value = 200000;

			var expectedPackageItem8 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem8.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem8.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem8.GSI_Value = 20000;

			var expectedPackageItem9 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem9.GSI_GSR_Remuneration = latestRemuneration3.PK;
			expectedPackageItem9.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem9.GSI_Value = 50000;

			var expectedPackageItem10 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem10.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem10.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem10.GSI_Value = 200001;

			var expectedPackageItem11 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem11.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem11.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem11.GSI_Value = 20001;

			var expectedPackageItem12 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem12.GSI_GSR_Remuneration = latestRemuneration4.PK;
			expectedPackageItem12.GSI_EntitlementCode = "13M"; //13th Month Bonus
			expectedPackageItem12.GSI_Value = 50000;

			AssertEquals(3, package1.Length);
			AssertEquals(3, package2.Length);
			AssertEquals(3, package3.Length);
			AssertEquals(3, package4.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2, expectedPackageItem3 }, package1);
			AssertPackage(new[] { expectedPackageItem4, expectedPackageItem5, expectedPackageItem6 }, package2);
			AssertPackage(new[] { expectedPackageItem7, expectedPackageItem8, expectedPackageItem9 }, package3);
			AssertPackage(new[] { expectedPackageItem10, expectedPackageItem11, expectedPackageItem12 }, package4);
		}

		public void TestExistingEntitlementGroupCarriedOver()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });

			var entitlementGroup1 = Factory.NewWithValidTestData<GlbStaffEntitlementGroup>();
			entitlementGroup1.GEG_Name = "TestEntGroup1";
			entitlementGroup1.GEG_RN_NKCountry = "AU";
			entitlementGroup1.GEG_EntitlementCode = "BAS";

			var entitlementGroup2 = Factory.NewWithValidTestData<GlbStaffEntitlementGroup>();
			entitlementGroup2.GEG_Name = "TestEntGroup2";
			entitlementGroup2.GEG_RN_NKCountry = "AU";
			entitlementGroup2.GEG_EntitlementCode = "WOW";

			var entitlementType1 = Factory.NewWithValidTestData<GlbStaffEntitlementType>();
			entitlementType1.GEW_GEG_EntitlementGroup = entitlementGroup1.PK;
			entitlementType1.GEW_EntitlementCode = "WOW"; //Ways of Working Allowance
			entitlementType1.GEW_PropertyType = "C";
			entitlementType1.GEW_WeightPercentage = 10;

			var entitlementType2 = Factory.NewWithValidTestData<GlbStaffEntitlementType>();
			entitlementType2.GEW_GEG_EntitlementGroup = entitlementGroup1.PK;
			entitlementType2.GEW_EntitlementCode = "BAS"; //Total Base Salary
			entitlementType2.GEW_PropertyType = "C";
			entitlementType2.GEW_WeightPercentage = 90;

			var entitlementType3 = Factory.NewWithValidTestData<GlbStaffEntitlementType>();
			entitlementType3.GEW_GEG_EntitlementGroup = entitlementGroup2.PK;
			entitlementType3.GEW_EntitlementCode = "WOW"; //Ways of Working Allowance
			entitlementType3.GEW_PropertyType = "C";
			entitlementType3.GEW_WeightPercentage = 100;

			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

			var ent1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent1.GSI_GSR_Remuneration = rem.PK;
			ent1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			ent1.GSI_Value = 100000;
			ent1.GSI_GEG_Breakdown = entitlementGroup1.PK;

			var ent2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			ent2.GSI_GSR_Remuneration = rem.PK;
			ent2.GSI_EntitlementCode = "WOW"; //Ways of Working Allowance
			ent2.GSI_Value = 300000;
			ent2.GSI_GEG_Breakdown = entitlementGroup2.PK;

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var remunerations = LoadRemunerations(staff);
			var latestRemuneration = remunerations.OrderByDescending(x => x.GSR_EffectiveDate).First();

			var package = LoadEntitlements(latestRemuneration);

			var expectedPackageItem1 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem1.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem1.GSI_EntitlementCode = "BAS"; //Total Base Salary
			expectedPackageItem1.GSI_Value = 200000;
			expectedPackageItem1.GSI_GEG_Breakdown = entitlementGroup1.PK;

			var expectedPackageItem2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem2.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem2.GSI_EntitlementCode = "RME"; //Remuneration Equity
			expectedPackageItem2.GSI_Value = 20000;

			var expectedPackageItem3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
			expectedPackageItem3.GSI_GSR_Remuneration = latestRemuneration.PK;
			expectedPackageItem3.GSI_EntitlementCode = "WOW"; //Ways of Working Allowance
			expectedPackageItem3.GSI_Value = 300000;
			expectedPackageItem3.GSI_GEG_Breakdown = entitlementGroup2.PK;

			AssertEquals(3, package.Length);
			AssertPackage(new[] { expectedPackageItem1, expectedPackageItem2, expectedPackageItem3 }, package);
		}

		public void TestStaffWithExistingRemuneration_SameEffectiveDate()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			SetupHierarchyProposals(new[] { staff }, new[] { manager });

			var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
			rem.GSR_GS_Staff = staff.PK;
			rem.GSR_EffectiveDate = EffectiveDateInStaffTimeZone.AddYears(-2);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };

			_ = AssertExceptionThrown<InvalidOperationException>("Should throw exception when the last remuneration has the same effective date",
				"Remuneration already exists for staff ZLV with effective date 12-Feb-22 00:00:00",
				() =>
				{
					task.RunTask();
				});
		}

		static void AssertRemuneration(GlbStaffRemuneration expectedRemuneration, GlbStaffRemuneration actualRemuneration)
		{
			AssertEquals(expectedRemuneration.GSR_EffectiveDate, actualRemuneration.GSR_EffectiveDate);
			AssertEquals(expectedRemuneration.GSR_FullTimeEquivalent, actualRemuneration.GSR_FullTimeEquivalent);
			AssertEquals(expectedRemuneration.GSR_LeaveLiabilityHourlyRate, actualRemuneration.GSR_LeaveLiabilityHourlyRate);
			AssertEquals(expectedRemuneration.GSR_RN_NKCountry, actualRemuneration.GSR_RN_NKCountry);
			AssertEquals(expectedRemuneration.GSR_RX_NKCurrency, actualRemuneration.GSR_RX_NKCurrency);
		}

		static void AssertPackage(GlbStaffEntitlement[] expectedPackage, GlbStaffEntitlement[] actualPackage, bool isGrantDate = false)
		{
			foreach (var expectedPackageItem in expectedPackage)
			{
				var actualPackageItem = actualPackage.First(p => p.GSI_EntitlementCode == expectedPackageItem.GSI_EntitlementCode);

				AssertEquals(expectedPackageItem.GSI_GSR_Remuneration, actualPackageItem.GSI_GSR_Remuneration);
				AssertEquals(expectedPackageItem.GSI_EntitlementCode, actualPackageItem.GSI_EntitlementCode);
				AssertEquals(expectedPackageItem.GSI_Value, actualPackageItem.GSI_Value);
				AssertEquals(expectedPackageItem.GSI_GEG_Breakdown, actualPackageItem.GSI_GEG_Breakdown);

				if (isGrantDate)
				{
					AssertEquals(expectedPackageItem.GSI_GrantDate, actualPackageItem.GSI_GrantDate);
				}
			}
		}

		#endregion

		public void TestShouldNotCloseReviewNodes()
		{
			var managers1 = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var staffs1 = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();

			//Not all node statuses in a tree with APP
			var reviewProcessNodes = SetupHierarchyProposals(staffs1, managers1).ToArray();
			reviewProcessNodes[1].RRN_Status = "ASN";
			SetupRemEntitlements(staffs1);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			reviewProcessNodes.ForEach(n => n.Reload());
			reviewProcessNodes.ForEach(n => n.ReviewProcess.Reload());
			var finishedNodes = reviewProcessNodes.Where(x => x.RRN_Status == "FIN");

			AssertEquals(3, finishedNodes.Count());
			AssertEquals("WRK", finishedNodes.First().ReviewProcess.RPR_Status);
		}

		public void TestShouldCloseReviewNodes()
		{
			var managers1 = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var staffs1 = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();

			//All node statuses in a tree with APP
			var reviewProcessNodes = SetupHierarchyProposals(staffs1, managers1).ToArray();
			SetupRemEntitlements(staffs1);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			reviewProcessNodes.ForEach(n => n.Reload());
			reviewProcessNodes.ForEach(n => n.ReviewProcess.Reload());

			var finishedNodes = reviewProcessNodes.Where(x => x.RRN_Status == "FIN");
			AssertEquals(4, finishedNodes.Count());
			AssertEquals("CLS", finishedNodes.First().ReviewProcess.RPR_Status);
		}

		public void TestShouldCloseReviewNodesWithExceptionToExcludedProposals()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var includedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var excludedStaff = Factory.NewWithValidTestData<GlbStaff>();

			//All node statuses in a tree with APP
			var reviewProcessNode = SetupHierarchyProposals(new[] { includedStaff }, new[] { manager });

			var proposal = Factory.NewWithValidTestData<ReviewProposal>(); // The review proposal which should be ignored.
			proposal.RRP_RRN_ReviewNode = reviewProcessNode[0].PK;
			proposal.RRP_GS_Staff = excludedStaff.PK;
			proposal.RRP_IsExcluded = true;

			var entitlement1 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
			entitlement1.RRE_RRP_Proposal = proposal.PK;
			entitlement1.RRE_EntitlementCode = "BAS"; //Total Base Salary
			entitlement1.RRE_Value = 200000;

			var entitlement2 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
			entitlement2.RRE_RRP_Proposal = proposal.PK;
			entitlement2.RRE_EntitlementCode = "RME"; //Remuneration Equity
			entitlement2.RRE_Value = 20000;

			SetupRemEntitlements(new[] { includedStaff, excludedStaff });

			Factory.Save();

			var includedRem = LoadRemunerations(includedStaff);
			var excludedRem = LoadRemunerations(excludedStaff);

			AssertEquals(1, includedRem.Length);
			AssertEquals(1, excludedRem.Length);

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			includedRem = LoadRemunerations(includedStaff);
			excludedRem = LoadRemunerations(excludedStaff);

			AssertEquals(2, includedRem.Length);
			AssertEquals(1, excludedRem.Length);
		} 

		#region Performance

		public void TestStaffWithNoExistingPerformanceScore()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var performance = LoadPerformanceReviews(staff).Single();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);
		}

		public void TestStaffWithExistingPerformanceScore()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var previousManager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffReview>();
			previous.GSV_GS_Staff = staff.PK;
			previous.GSV_GS_NKReviewer = previousManager.GS_Code;
			previous.GSV_Score = 2;
			previous.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var performances = LoadPerformanceReviews(staff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(2, performances.Count());

			var performance = performances.First();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);
		}

		public void TestStaffWithExistingPerformanceScore_SameEffectiveDate()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var previousManager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffReview>();
			previous.GSV_GS_Staff = staff.PK;
			previous.GSV_GS_NKReviewer = previousManager.GS_Code;
			previous.GSV_Score = 2;
			previous.GSV_EffectiveDate = EffectiveDateInStaffTimeZone;

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };

			_ = AssertExceptionThrown<InvalidOperationException>("Should throw exception when an existing classification has the same effective date",
				"Performance score already exists for staff HA6 with effective date 12-Feb-24 00:00:00",
				() =>
				{
					task.RunTask();
				});
		}

		public void TestMultipleStaffInPerformanceReview()
		{
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var previousManager1 = Factory.NewWithValidTestData<GlbStaff>();
			var previousManager2 = Factory.NewWithValidTestData<GlbStaff>();

			var staff1 = CreateStaffWithTimeZone();
			var staff2 = CreateStaffWithTimeZone();
			var staff3 = CreateStaffWithTimeZone();
			var staff4 = CreateStaffWithTimeZone();

			var previous1 = Factory.NewWithValidTestData<GlbStaffReview>();
			previous1.GSV_GS_Staff = staff1.PK;
			previous1.GSV_GS_NKReviewer = previousManager1.GS_Code;
			previous1.GSV_Score = 1;
			previous1.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous2 = Factory.NewWithValidTestData<GlbStaffReview>();
			previous2.GSV_GS_Staff = staff2.PK;
			previous2.GSV_GS_NKReviewer = previousManager2.GS_Code;
			previous2.GSV_Score = 2;
			previous2.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous3 = Factory.NewWithValidTestData<GlbStaffReview>();
			previous3.GSV_GS_Staff = staff3.PK;
			previous3.GSV_GS_NKReviewer = previousManager2.GS_Code;
			previous3.GSV_Score = 4;
			previous3.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous4 = Factory.NewWithValidTestData<GlbStaffReview>();
			previous4.GSV_GS_Staff = staff4.PK;
			previous4.GSV_GS_NKReviewer = previousManager1.GS_Code;
			previous4.GSV_Score = 5;
			previous4.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			var staffs = new[] { staff1, staff2, staff3, staff4 };
			var managers = new[] { manager1, manager1, manager2, manager2 };
			_ = SetupPerformanceAndClassificationProposals(staffs, managers, isPerformanceReview: true, isClassificationReview: false);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			foreach (var (s, m) in staffs.Zip(managers, (s, m) => Tuple.Create(s, m)))
			{
				var performances = LoadPerformanceReviews(s).OrderByDescending(p => p.GSV_EffectiveDate);
				AssertEquals(2, performances.Count());

				var performance = performances.First();
				AssertEquals(s.PK, performance.GSV_GS_Staff);
				AssertEquals(m.GS_Code, performance.GSV_GS_NKReviewer);
				AssertEquals(PerformanceScore, performance.GSV_Score);
				AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
				AssertEquals(Comment, performance.GSV_Comments);
			}
		}

		public void TestStaffExcludedFromPerformanceReview()
		{
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var includedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var excludedStaff = Factory.NewWithValidTestData<GlbStaff>();

			_ = SetupPerformanceAndClassificationProposals(new[] { includedStaff, excludedStaff }, new[] { manager1, excludedStaff }, isPerformanceReview: true, isClassificationReview: false);
			var excludedProposal = Factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_GS_Staff, excludedStaff.PK));
			AssertEquals(1, excludedProposal.Length);
			excludedProposal[0].RRP_IsExcluded = true;

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var includedStaffPerformance = LoadPerformanceReviews(includedStaff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(1, includedStaffPerformance.Count());
			var excludedStaffPerformance = LoadPerformanceReviews(excludedStaff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(0, excludedStaffPerformance.Count());
		}
		#endregion

		#region Classification

		public void TestStaffWithNoExistingClassification()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: false, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var classification = LoadClassifications(staff).Single();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);
		}

		public void TestStaffWithExistingClassification()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous.GSL_GS_Staff = staff.PK;
			previous.GSL_Classification = "BT2";
			previous.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: false, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var classifications = LoadClassifications(staff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(2, classifications.Count());

			var classification = classifications.First();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);
		}

		public void TestStaffWithExistingClassification_SameEffectiveDate()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous.GSL_GS_Staff = staff.PK;
			previous.GSL_Classification = "BT2";
			previous.GSL_EffectiveDate = EffectiveDateInStaffTimeZone;

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: false, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };

			_ = AssertExceptionThrown<InvalidOperationException>("Should throw exception when an existing classification has the same effective date",
				"Classification already exists for staff ZLV with effective date 12-Feb-24 00:00:00",
				() =>
				{
					task.RunTask();
				});
		}

		public void TestMultipleStaffInClassificationReview()
		{
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();

			var staff1 = CreateStaffWithTimeZone();
			var staff2 = CreateStaffWithTimeZone();
			var staff3 = CreateStaffWithTimeZone();
			var staff4 = CreateStaffWithTimeZone();

			var previous1 = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous1.GSL_GS_Staff = staff1.PK;
			previous1.GSL_Classification = "BT2";
			previous1.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous2 = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous2.GSL_GS_Staff = staff2.PK;
			previous2.GSL_Classification = "BT3";
			previous2.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous3 = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous3.GSL_GS_Staff = staff3.PK;
			previous3.GSL_Classification = "BT4";
			previous3.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			var previous4 = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous4.GSL_GS_Staff = staff4.PK;
			previous4.GSL_Classification = "BT5";
			previous4.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			var staffs = new[] { staff1, staff2, staff3, staff4 };
			var managers = new[] { manager1, manager1, manager2, manager2 };
			_ = SetupPerformanceAndClassificationProposals(staffs, managers, isPerformanceReview: false, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			foreach (var staff in staffs)
			{
				var classifications = LoadClassifications(staff).OrderByDescending(p => p.GSL_EffectiveDate);
				AssertEquals(2, classifications.Count());

				var classification = classifications.First();
				AssertEquals(staff.PK, classification.GSL_GS_Staff);
				AssertEquals(Classification, classification.GSL_Classification);
				AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);
			}
		}

		public void TestStaffExcludedFromClassificationReview()
		{
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var includedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var excludedStaff = Factory.NewWithValidTestData<GlbStaff>();

			_ = SetupPerformanceAndClassificationProposals(new[] { includedStaff, excludedStaff }, new[] { manager1, manager2 }, isPerformanceReview: false, isClassificationReview: true);
			var excludedProposal = Factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_GS_Staff, excludedStaff.PK));
			AssertEquals(1, excludedProposal.Length);
			excludedProposal[0].RRP_IsExcluded = true;

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var includedClassification = LoadClassifications(includedStaff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(1, includedClassification.Count());
			var excludedClassification = LoadClassifications(excludedStaff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(0, excludedClassification.Count());
		}

		#endregion

		#region Combined

		public void TestStaffWithPerformanceNoClassification_Combined()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var previousManager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffReview>();
			previous.GSV_GS_Staff = staff.PK;
			previous.GSV_GS_NKReviewer = previousManager.GS_Code;
			previous.GSV_Score = 2;
			previous.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var performances = LoadPerformanceReviews(staff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(2, performances.Count());

			var performance = performances.First();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);

			var classification = LoadClassifications(staff).Single();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);
		}

		public void TestStaffWithClassificationNoPerformance_Combined()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previous = Factory.NewWithValidTestData<GlbStaffClassification>();
			previous.GSL_GS_Staff = staff.PK;
			previous.GSL_Classification = "BT2";
			previous.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var classifications = LoadClassifications(staff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(2, classifications.Count());

			var classification = classifications.First();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);

			var performance = LoadPerformanceReviews(staff).Single();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);
		}

		public void TestStaffWithNoClassificationOrPerformance()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var performance = LoadPerformanceReviews(staff).Single();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);

			var classification = LoadClassifications(staff).Single();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);
		}

		public void TestStaffWithExistingClassificationAndPerformance()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = CreateStaffWithTimeZone();

			var previousClassification = Factory.NewWithValidTestData<GlbStaffClassification>();
			previousClassification.GSL_GS_Staff = staff.PK;
			previousClassification.GSL_Classification = "BT2";
			previousClassification.GSL_EffectiveDate = EffectiveDate.AddYears(-1);

			var previousPerformance = Factory.NewWithValidTestData<GlbStaffReview>();
			previousPerformance.GSV_GS_Staff = staff.PK;
			previousPerformance.GSV_GS_NKReviewer = manager.GS_Code;
			previousPerformance.GSV_Score = 2;
			previousPerformance.GSV_EffectiveDate = EffectiveDate.AddYears(-1);

			_ = SetupPerformanceAndClassificationProposals(new[] { staff }, new[] { manager }, isPerformanceReview: true, isClassificationReview: true);

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var classifications = LoadClassifications(staff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(2, classifications.Count());

			var classification = classifications.First();
			AssertEquals(staff.PK, classification.GSL_GS_Staff);
			AssertEquals(Classification, classification.GSL_Classification);
			AssertEquals(EffectiveDateInStaffTimeZone, classification.GSL_EffectiveDate);

			var performances = LoadPerformanceReviews(staff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(2, performances.Count());

			var performance = performances.First();
			AssertEquals(staff.PK, performance.GSV_GS_Staff);
			AssertEquals(manager.GS_Code, performance.GSV_GS_NKReviewer);
			AssertEquals(PerformanceScore, performance.GSV_Score);
			AssertEquals(EffectiveDateInStaffTimeZone, performance.GSV_EffectiveDate);
			AssertEquals(Comment, performance.GSV_Comments);
		}

		public void TestStaffWithExcludedPerformanceAndClassificationsProposal()
		{
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var includedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var excludedStaff = Factory.NewWithValidTestData<GlbStaff>();

			_ = SetupPerformanceAndClassificationProposals(new[] { includedStaff, excludedStaff }, new[] { manager1, manager2 }, isPerformanceReview: true, isClassificationReview: true);
			var excludedProposal = Factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_GS_Staff, excludedStaff.PK));
			AssertEquals(1, excludedProposal.Length);
			excludedProposal[0].RRP_IsExcluded = true;

			Factory.Save();

			var task = new ReviewProcessNodeCopybackServiceTask() { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var includedStaffPerformance = LoadPerformanceReviews(includedStaff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(1, includedStaffPerformance.Count());
			var excludedStaffPerformance = LoadPerformanceReviews(excludedStaff).OrderByDescending(p => p.GSV_EffectiveDate);
			AssertEquals(0, excludedStaffPerformance.Count());

			var includedClassification = LoadClassifications(includedStaff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(1, includedClassification.Count());
			var excludedClassification = LoadClassifications(excludedStaff).OrderByDescending(p => p.GSL_EffectiveDate);
			AssertEquals(0, excludedClassification.Count());
		}

		#endregion

		List<ReviewProcessNode> SetupHierarchyProposals(GlbStaff[] staffs, GlbStaff[] managers)
		{
			if (staffs.Length != managers.Length)
			{
				throw new ArgumentException("Cannot setup Hierarchy");
			}

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "WRK";
			process.RPR_ConfigType = "D";
			process.RPR_EffectiveDate = EffectiveDateOnly.AddYears(-2);

			var nodes = new List<ReviewProcessNode>();
			for (var i = 0; i < staffs.Length; i++)
			{
				var node = Factory.NewWithValidTestData<ReviewProcessNode>();
				node.RRN_RPR_ReviewProcess = process.PK;
				node.RRN_GS_Reviewer = managers[i].PK;
				node.RRN_Status = "APP"; //Approved

				var proposal = Factory.NewWithValidTestData<ReviewProposal>();
				proposal.RRP_RRN_ReviewNode = node.PK;
				proposal.RRP_GS_Staff = staffs[i].PK;
				proposal.RRP_IsExcluded = false;

				var entitlement1 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
				entitlement1.RRE_RRP_Proposal = proposal.PK;
				entitlement1.RRE_EntitlementCode = "BAS"; //Total Base Salary
				entitlement1.RRE_Value = 200000 + i;

				var entitlement2 = Factory.NewWithValidTestData<ReviewProposalEntitlement>();
				entitlement2.RRE_RRP_Proposal = proposal.PK;
				entitlement2.RRE_EntitlementCode = "RME"; //Remuneration Equity
				entitlement2.RRE_Value = 20000 + i;

				nodes.Add(node);
			}

			return nodes;
		}

		List<ReviewProcessNode> SetupPerformanceAndClassificationProposals(GlbStaff[] staffs, GlbStaff[] managers, bool isPerformanceReview, bool isClassificationReview)
		{
			if (staffs.Length != managers.Length)
			{
				throw new ArgumentException("Cannot setup Hierarchy");
			}

			var f = Factory.New<StmModuleFilter>();
			Factory.Save();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "WRK";
			process.RPR_Type = isClassificationReview ? isPerformanceReview ? "C&P" : "CLA" : "PER";
			process.RPR_ConfigType = string.Empty;
			process.RPR_GC_Company = Guid.Empty;
			process.RPR_EffectiveDate = EffectiveDateOnly;
			process.RPR_S9_EmployeesInReview = f.PK;
			process.RPR_RX_NKCurrency = string.Empty;

			var nodes = new List<ReviewProcessNode>();
			for (var i = 0; i < staffs.Length; i++)
			{
				var node = Factory.Load<ReviewProcessNode>(new ZQuery(ReviewProcessNodeSchema.RRN_GS_Reviewer, managers[i].PK)).FirstOrDefault();
				if (node == null)
				{
					node = Factory.NewWithValidTestData<ReviewProcessNode>();
					node.RRN_RPR_ReviewProcess = process.PK;
					node.RRN_GS_Reviewer = managers[i].PK;
					node.RRN_Status = "APP";
				}

				var proposal = Factory.NewWithValidTestData<ReviewProposal>();
				proposal.RRP_RRN_ReviewNode = node.PK;
				proposal.RRP_GS_Staff = staffs[i].PK;
				if (isPerformanceReview)
				{
					proposal.RRP_PerformanceScore = new ZShort(PerformanceScore);
					proposal.RRP_Comments = Comment;
				}

				if (isClassificationReview)
				{
					proposal.RRP_Classification = Classification;
				}

				nodes.Add(node);
			}

			return nodes;
		}

		void SetupRemEntitlements(GlbStaff[] staffs, bool withEntitlementNotInReview = true)
		{
			for (int i = 0; i < staffs.Length; i++)
			{
				var rem = Factory.NewWithValidTestData<GlbStaffRemuneration>();
				rem.GSR_GS_Staff = staffs[i].PK;
				rem.GSR_EffectiveDate = EffectiveDate.AddYears(-3);

				var ent = Factory.NewWithValidTestData<GlbStaffEntitlement>();
				ent.GSI_GSR_Remuneration = rem.PK;
				ent.GSI_EntitlementCode = "BAS"; //Total Base Salary
				ent.GSI_Value = 300000;
				ent.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(4);

				var ent2 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
				ent2.GSI_GSR_Remuneration = rem.PK;
				ent2.GSI_EntitlementCode = "RME"; //Remuneration Equity
				ent2.GSI_Value = 30000;
				ent2.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(5);

				if (withEntitlementNotInReview)
				{
					var ent3 = Factory.NewWithValidTestData<GlbStaffEntitlement>();
					ent3.GSI_GSR_Remuneration = rem.PK;
					ent3.GSI_EntitlementCode = "13M"; //13th Month Bonus
					ent3.GSI_Value = 50000;
					ent3.GSI_GrantDate = EffectiveDateOnly.AddYears(-3).AddDays(6);
				}
			}
		}

		GlbStaff CreateStaffWithTimeZone()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			GetTimeZoneSet("Literal/Three", GetTimeZone("LIT3", 3 * 60), null);

			AddTimezoneHistory(staff, new[]
			{
				(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), "Literal/Three"),
			});

			return staff;
		}

		void AddTimezoneHistory(GlbStaff staff, IEnumerable<(DateTimeOffset Start, string TimezoneName)> histories)
		{
			for (var i = 0; i < histories.Count(); i++)
			{
				var row = Factory.NewWithValidTestData<GlbStaffTimezone>();

				row.GSZ_GS_Staff = staff.PK;
				row.GSZ_EffectiveDate = histories.ElementAt(i).Start;
				row.GSZ_R3_NKTimeZoneSetName = histories.ElementAt(i).TimezoneName;

				if (i < histories.Count() - 1)
				{
					row.GSZ_AutoEffectiveEndDate = histories.ElementAt(i + 1).Start;
				}
			}
		}

		RefTimeZoneSet GetTimeZoneSet(string timezoneName, RefTimeZone stdTz, RefTimeZone dstTz, bool updateAnyway = false)
		{
			var timezoneSet = Factory.LoadTop1<RefTimeZoneSet>(new ZQuery(RefTimeZoneSetSchema.R3_TimeZoneSetName, timezoneName));
			var tzSetNotFound = timezoneSet is null;

			if (tzSetNotFound)
			{
				timezoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			}

			if (updateAnyway || tzSetNotFound)
			{
				timezoneSet.R3_TimeZoneSetName = timezoneName;
				timezoneSet.R3_R2_StandardZone = stdTz.PK;
				timezoneSet.R3_R2_DaylightSavingZone = dstTz == null ? ZGuid.Empty : dstTz.PK;
			}

			return timezoneSet;
		}

		RefTimeZone GetTimeZone(string timezoneCode, short offsetMinutesFromUTC, bool updateAnyway = false)
		{
			var timezone = Factory.LoadTop1<RefTimeZone>(new ZQuery(RefTimeZoneSchema.R2_CivilianTimeZoneCode, timezoneCode));
			var tzNotFount = timezone is null;

			if (tzNotFount)
			{
				timezone = Factory.NewWithValidTestData<RefTimeZone>();
			}

			if (updateAnyway || tzNotFount)
			{
				timezone.R2_CivilianTimeZoneCode = timezoneCode;
				timezone.R2_OffsetMinutesFromUTC = offsetMinutesFromUTC;
			}

			return timezone;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "RPR");
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			connection?.Dispose();
		}

		DbConnection connection;
		protected override DbConnection TestConnection
			=> connection ?? (connection = Db.NewAdminConnection());

		protected override BusinessObjectFactory NewFactory()
			=> new BusinessObjectFactory(TestConnection);

		GlbStaffRemuneration[] LoadRemunerations(GlbStaff staff)
			=> Factory.Load<GlbStaffRemuneration>(new ZQuery(GlbStaffRemunerationSchema.GSR_GS_Staff, staff.PK));
		GlbStaffEntitlement[] LoadEntitlements(GlbStaffRemuneration rem)
			=> Factory.Load<GlbStaffEntitlement>(new ZQuery(GlbStaffEntitlementSchema.GSI_GSR_Remuneration, rem.PK));

		GlbStaffReview[] LoadPerformanceReviews(GlbStaff staff)
			=> Factory.Load<GlbStaffReview>(new ZQuery(GlbStaffReviewSchema.GSV_GS_Staff, staff.PK));

		GlbStaffClassification[] LoadClassifications(GlbStaff staff)
			=> Factory.Load<GlbStaffClassification>(new ZQuery(GlbStaffClassificationSchema.GSL_GS_Staff, staff.PK));

		readonly byte PerformanceScore = 3;

		readonly string Classification = "BT1";

		readonly ZDateTimeOffset EffectiveDate = new (2024, 02, 12);

		readonly ZDateTimeOffset EffectiveDateInStaffTimeZone = new(new DateTime(2024, 02, 12), new TimeSpan(3, 0, 0));

		readonly ZDate EffectiveDateOnly = new (2024, 02, 12);

		readonly ZString Comment = new ("Doing great!");
	}
}
