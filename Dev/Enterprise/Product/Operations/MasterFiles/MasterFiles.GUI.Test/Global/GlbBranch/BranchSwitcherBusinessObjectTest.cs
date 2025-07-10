using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BranchSwitcherBusinessObject))]
	sealed class BranchSwitcherBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFindsCorrectTasks()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var rightBranch = Factory.NewWithValidTestData<GlbBranch>();
				var wrongBranch = Factory.NewWithValidTestData<GlbBranch>();

				var rightServiceTask = CreateServiceTask(rightBranch);
				var wrongServiceTask = CreateServiceTask(wrongBranch);
				var rightReportTask = CreateReportTask(rightBranch);
				var wrongReportTask = CreateReportTask(wrongBranch);

				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(rightBranch);

				AssertCollectionContains(rightServiceTask.PK, switcher.ServiceTasks.Select(o => o.PK));
				AssertCollectionNotContains(wrongServiceTask.PK, switcher.ServiceTasks.Select(o => o.PK));
				AssertCollectionContains(rightReportTask.PK, switcher.ScheduledReports.Select(o => o.PK));
				AssertCollectionNotContains(wrongReportTask.PK, switcher.ScheduledReports.Select(o => o.PK));
			}
		}

		public void TestFindsCorrectStaff()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var staff1 = CreateGlbStaff(branch1.PK);
			var staff2 = CreateGlbStaff(branch1.PK);
			staff2.GS_IsActive = false;
			var staff3 = CreateGlbStaff(branch2.PK);

			Factory.Save();

			var branchSwitcher = new BranchSwitcherBusinessObject(branch1);

			CombineAssertions(() =>
			{
				AssertCollectionContains("The Staff's HomeBranch is equal to the Branch passed to the BranchSwitcher. The Staff record should be in the collection.", staff1.PK, branchSwitcher.Staff.Select(s => s.PK));
				AssertCollectionContains("The Staff's HomeBranch is equal to the Branch passed to the BranchSwitcher. The Staff record should be in the collection.", staff2.PK, branchSwitcher.Staff.Select(s => s.PK));

				AssertCollectionNotContains("The Staff's HomeBranch is different to the Branch passed to the BranchSwitcher. The Staff record should have been filtered out.", staff3.PK, branchSwitcher.Staff.Select(s => s.PK));
			});
		}

		public void TestUpdatesAllApplicableTasksWhenChanging()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
				var newBranch = Factory.NewWithValidTestData<GlbBranch>();
				var explicitBranch = Factory.NewWithValidTestData<GlbBranch>();

				var explicitTask = CreateServiceTask(oldBranch);
				var implicitTask = CreateServiceTask(oldBranch);
				var explicitReport = CreateReportTask(oldBranch);
				var implicitReport = CreateReportTask(oldBranch);

				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(oldBranch);

				var explicitTaskOnOtherFactory = (ServiceTaskSchedule)switcher.ServiceTasks.First(t => t.PK == explicitTask.PK);
				var implicitTaskOnOtherFactory = (ServiceTaskSchedule)switcher.ServiceTasks.First(t => t.PK == implicitTask.PK);
				var explicitReportOnOtherFactory = (StmScheduleTask)switcher.ScheduledReports.First(t => t.PK == explicitReport.PK);
				var implicitReportOnOtherFactory = (StmScheduleTask)switcher.ScheduledReports.First(t => t.PK == implicitReport.PK);

				explicitTaskOnOtherFactory.S5_GB = explicitBranch.PK;
				explicitReportOnOtherFactory.S5_GB = explicitBranch.PK;

				switcher.ReplacementBranch = newBranch.PK;

				AssertEquals("Because we explicitly set this branch, it should not be changed when we set the replacement", explicitBranch.PK, explicitTaskOnOtherFactory.S5_GB);
				AssertEquals("Because this is set to the default, we should update it when a replacement is selected", newBranch.PK, implicitTaskOnOtherFactory.S5_GB);
				AssertEquals("Because we explicitly set this branch, it should not be changed when we set the replacement", explicitBranch.PK, explicitReportOnOtherFactory.S5_GB);
				AssertEquals("Because this is set to the default, we should update it when a replacement is selected", newBranch.PK, implicitReportOnOtherFactory.S5_GB);
			}
		}

		public void TestUpdatesAllApplicableStaff_WhenReplacingImplicitly()
		{
			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = CreateGlbStaff(oldBranch.PK);

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(oldBranch);
			var updatedStaff = switcher.Staff.First(s => s.PK == staff.PK);

			switcher.ReplacementBranch = newBranch.PK;

			AssertEquals("Because HomeBranch is set to the default, we should update it when a replacement is selected.", newBranch.PK, updatedStaff.GS_GB_HomeBranch);
		}

		public void TestUpdatesAllApplicableStaff_WhenReplacingExplicitly()
		{
			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var explicitBranch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = CreateGlbStaff(oldBranch.PK);

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(oldBranch);
			var updatedStaff = switcher.Staff.First(s => s.PK == staff.PK);

			updatedStaff.GS_GB_HomeBranch = explicitBranch.PK;
			switcher.ReplacementBranch = newBranch.PK;

			AssertEquals("Because we explicitly set the HomeBranch, it should not be changed when we set the replacement.", explicitBranch.PK, updatedStaff.GS_GB_HomeBranch);
		}

		public void TestCompanyWithoutAnyBranchShouldNotHaveActiveSchedules()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(company);
				AssertEquals("switcher.HasActiveSchedules", false, switcher.HasActiveSchedules);
			}
		}

		public void TestCompanyWithoutAnyBranchShouldNotHaveActiveStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(company);

			Assert("There should be no active Staff.", !switcher.HasActiveStaff);
		}

		public void TestRefreshValidationOnBranches()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var taskSchedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				taskSchedule.S5_GB = GlbBranch.CurrentBranch.PK;
				taskSchedule.S5_IsActive = false;

				Factory.Save();

				var attributes = new HostedServiceAttribute { IsMandatory = true };

				taskSchedule.SetStaticServiceAttributesDebugOnly(attributes);

				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
				switcher.RefreshValidationOnBranches();

				AssertNoErrors(switcher);
			}
		}

		public void TestRefreshValidationOnBranches_ForStaff()
		{
			var staff = CreateGlbStaff(GlbBranch.CurrentBranch.PK);
			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_IsActive = false;

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
			switcher.RefreshValidationOnBranches();

			AssertNoErrors(switcher);

			var branchSwitcherStaff = switcher.Staff.First(s => s.PK == staff.PK);
			branchSwitcherStaff.GS_GB_HomeBranch = inactiveBranch.PK;

			switcher.RefreshValidationOnBranches();
			AssertHasError(branchSwitcherStaff.GS_GB_HomeBranchInfo, "This branch is inactive.");
		}

		public void TestHasActiveSchedules()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CreateServiceTask(GlbBranch.CurrentBranch);
				CreateServiceTask(GlbBranch.CurrentBranch);
				CreateReportTask(GlbBranch.CurrentBranch);
				CreateReportTask(GlbBranch.CurrentBranch);
				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
				Assert(switcher.HasActiveSchedules);

				switcher.ServiceTasks[0].S5_IsActive = false;
				Assert(switcher.HasActiveSchedules);

				switcher.ServiceTasks[1].S5_IsActive = false;
				Assert(switcher.HasActiveSchedules);

				switcher.ScheduledReports[0].S5_IsActive = false;
				Assert(switcher.HasActiveSchedules);

				switcher.ScheduledReports[1].S5_IsActive = false;
				Assert("Nothing active", !switcher.HasActiveSchedules);

				switcher.ServiceTasks[1].S5_IsActive = true;
				Assert(switcher.HasActiveSchedules);
			}
		}

		public void TestHasActiveStaff()
		{
			_ = CreateGlbStaff(GlbBranch.CurrentBranch.PK);
			_ = CreateGlbStaff(GlbBranch.CurrentBranch.PK);

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
			Assert(switcher.HasActiveStaff);

			switcher.Staff[0].GS_IsActive = false;
			Assert(switcher.HasActiveStaff);

			switcher.Staff[1].GS_IsActive = false;
			Assert("Nothing active", !switcher.HasActiveStaff);

			switcher.Staff[1].GS_IsActive = true;
			Assert(switcher.HasActiveStaff);
		}

		public void TestCancelBranchChanges()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				_ = CreateServiceTask(GlbBranch.CurrentBranch);
				_ = CreateServiceTask(GlbBranch.CurrentBranch);
				_ = CreateReportTask(GlbBranch.CurrentBranch);
				_ = CreateReportTask(GlbBranch.CurrentBranch);

				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
				switcher.ServiceTasks[0].S5_IsActive = false;
				switcher.ServiceTasks[1].S5_GB = ZGuid.NewZGuid();
				switcher.ScheduledReports[0].S5_IsActive = false;
				switcher.ScheduledReports[1].S5_GB = ZGuid.NewZGuid();

				CombineAssertions("Precondition", () =>
				{
					Assert(switcher.ServiceTasks[0].HasChanges);
					Assert(switcher.ServiceTasks[1].HasChanges);

					Assert(switcher.ScheduledReports[0].HasChanges);
					Assert(switcher.ScheduledReports[1].HasChanges);
				});

				switcher.CancelBranchChanges();

				CombineAssertions(() =>
				{
					Assert(switcher.ServiceTasks[0].S5_IsActive);
					AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ServiceTasks[0].S5_GB);
					Assert(switcher.ServiceTasks[1].S5_IsActive);
					AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ServiceTasks[1].S5_GB);

					Assert(switcher.ScheduledReports[0].S5_IsActive);
					AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ScheduledReports[0].S5_GB);
					Assert(switcher.ScheduledReports[1].S5_IsActive);
					AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ScheduledReports[1].S5_GB);
				});
			}
		}

		public void TestCancelBranchChanges_ForStaff()
		{
			_ = CreateGlbStaff(GlbBranch.CurrentBranch.PK);
			_ = CreateGlbStaff(GlbBranch.CurrentBranch.PK);

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);

			switcher.Staff[0].GS_IsActive = false;
			switcher.Staff[0].GS_GB_HomeBranch = ZGuid.NewZGuid();
			switcher.Staff[1].GS_IsActive = false;
			switcher.Staff[1].GS_GB_HomeBranch = ZGuid.NewZGuid();

			CombineAssertions("Precondition", () =>
			{
				Assert(switcher.Staff[0].HasChanges);
				Assert(switcher.Staff[1].HasChanges);
			});

			switcher.CancelBranchChanges();

			CombineAssertions(() =>
			{
				Assert(switcher.Staff[0].GS_IsActive);
				AssertEquals(GlbBranch.CurrentBranch.PK, switcher.Staff[0].GS_GB_HomeBranch);
				Assert(switcher.Staff[1].GS_IsActive);
				AssertEquals(GlbBranch.CurrentBranch.PK, switcher.Staff[1].GS_GB_HomeBranch);
			});
		}

		public void TestCompanyLevel()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				var branch3 = Factory.NewWithValidTestData<GlbBranch>();
				company.Branches.Add(branch1);
				company.Branches.Add(branch2);

				var task1 = CreateServiceTask(branch1);
				var task2 = CreateServiceTask(branch2);
				CreateServiceTask(branch3);

				var report1 = CreateReportTask(branch1);
				var report2 = CreateReportTask(branch2);
				CreateReportTask(branch3);

				Factory.Save();

				var switcher = new BranchSwitcherBusinessObject(company);

				AssertEquals(2, switcher.ServiceTasks.Count);
				AssertContainsExactElementsInAnyOrder(new[] { task1.PK, task2.PK }, switcher.ServiceTasks.Select(o => o.PK));

				AssertEquals(2, switcher.ScheduledReports.Count);
				AssertContainsExactElementsInAnyOrder(new[] { report1.PK, report2.PK }, switcher.ScheduledReports.Select(o => o.PK));
			}
		}

		public void TestCompanyLevel_ForStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch1);
			company.Branches.Add(branch2);

			var staff1 = CreateGlbStaff(branch1.PK);
			var staff2 = CreateGlbStaff(branch2.PK);
			_ = CreateGlbStaff(branch3.PK);

			Factory.Save();

			var switcher = new BranchSwitcherBusinessObject(company);

			CombineAssertions(() =>
			{
				AssertEquals(2, switcher.Staff.Count);
				AssertContainsExactElementsInAnyOrder(new[] { staff1.PK, staff2.PK }, switcher.Staff.Select(s => s.PK));
			});
		}

		class StmServiceTaskTest : TestCaseWithFactory
		{
			public void TestFindsCorrectTasks()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var rightBranch = Factory.NewWithValidTestData<GlbBranch>();
					var wrongBranch = Factory.NewWithValidTestData<GlbBranch>();

					var rightServiceTask = CreateStmServiceTask(rightBranch, "111");
					var wrongServiceTask = CreateStmServiceTask(wrongBranch, "222");
					var rightReportTask = CreateReportTask(rightBranch, Factory);
					var wrongReportTask = CreateReportTask(wrongBranch, Factory);

					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(rightBranch);

					AssertCollectionContains(rightServiceTask.PK, switcher.StmServiceTasks.Select(o => o.PK));
					AssertCollectionNotContains(wrongServiceTask.PK, switcher.StmServiceTasks.Select(o => o.PK));
					AssertCollectionContains(rightReportTask.PK, switcher.ScheduledReports.Select(o => o.PK));
					AssertCollectionNotContains(wrongReportTask.PK, switcher.ScheduledReports.Select(o => o.PK));
				}
			}

			public void TestUpdatesAllApplicableTasksWhenChanging()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
					var newBranch = Factory.NewWithValidTestData<GlbBranch>();
					var explicitBranch = Factory.NewWithValidTestData<GlbBranch>();

					var explicitTask = CreateStmServiceTask(oldBranch, "111");
					var implicitTask = CreateStmServiceTask(oldBranch, "222");
					var explicitReport = CreateReportTask(oldBranch, Factory);
					var implicitReport = CreateReportTask(oldBranch, Factory);

					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(oldBranch);

					var explicitTaskOnOtherFactory = (StmServiceTask)switcher.StmServiceTasks.First(t => t.PK == explicitTask.PK);
					var implicitTaskOnOtherFactory = (StmServiceTask)switcher.StmServiceTasks.First(t => t.PK == implicitTask.PK);
					var explicitReportOnOtherFactory = (StmScheduleTask)switcher.ScheduledReports.First(t => t.PK == explicitReport.PK);
					var implicitReportOnOtherFactory = (StmScheduleTask)switcher.ScheduledReports.First(t => t.PK == implicitReport.PK);

					explicitTaskOnOtherFactory.SST_GB_Branch = explicitBranch.PK;
					explicitReportOnOtherFactory.S5_GB = explicitBranch.PK;

					switcher.ReplacementBranch = newBranch.PK;

					AssertEquals("Because we explicitly set this branch, it should not be changed when we set the replacement", explicitBranch.PK, explicitTaskOnOtherFactory.SST_GB_Branch);
					AssertEquals("Because this is set to the default, we should update it when a replacement is selected", newBranch.PK, implicitTaskOnOtherFactory.SST_GB_Branch);
					AssertEquals("Because we explicitly set this branch, it should not be changed when we set the replacement", explicitBranch.PK, explicitReportOnOtherFactory.S5_GB);
					AssertEquals("Because this is set to the default, we should update it when a replacement is selected", newBranch.PK, implicitReportOnOtherFactory.S5_GB);
				}
			}

			public void TestCompanyWithoutAnyBranchShouldNotHaveActiveSchedules()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(company);
					AssertEquals("switcher.HasActiveSchedules", false, switcher.HasActiveSchedules);
				}
			}

			public void TestRefreshValidationOnBranches()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: true, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var taskSchedule = Factory.NewWithValidTestData<StmServiceTask>();
					taskSchedule.SST_GB_Branch = GlbBranch.CurrentBranch.PK;
					taskSchedule.SST_Active = false;

					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
					switcher.RefreshValidationOnBranches();

					AssertNoErrors(switcher);
				}
			}

			public void TestHasActiveSchedules()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var serviceTask1 = CreateStmServiceTask(GlbBranch.CurrentBranch, "111");
					serviceTask1.NextRunTimeCalculator = new NextRunTimeCalculatorDays { Period = 1 };
					var serviceTask2 = CreateStmServiceTask(GlbBranch.CurrentBranch, "222");
					serviceTask2.NextRunTimeCalculator = new NextRunTimeCalculatorDays { Period = 1 };
					CreateReportTask(GlbBranch.CurrentBranch, Factory);
					CreateReportTask(GlbBranch.CurrentBranch, Factory);
					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
					Assert(switcher.HasActiveSchedules);

					switcher.StmServiceTasks[0].SST_Active = false;
					Assert(switcher.HasActiveSchedules);

					switcher.StmServiceTasks[1].SST_Active = false;
					Assert(switcher.HasActiveSchedules);

					switcher.ScheduledReports[0].S5_IsActive = false;
					Assert(switcher.HasActiveSchedules);

					switcher.ScheduledReports[1].S5_IsActive = false;
					Assert("Nothing active", !switcher.HasActiveSchedules);

					switcher.StmServiceTasks[1].SST_Active = true;
					Assert(switcher.HasActiveSchedules);
				}
			}

			public void TestCancelBranchChanges()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					_ = CreateStmServiceTask(GlbBranch.CurrentBranch, "111");
					_ = CreateStmServiceTask(GlbBranch.CurrentBranch, "222");
					_ = CreateReportTask(GlbBranch.CurrentBranch, Factory);
					_ = CreateReportTask(GlbBranch.CurrentBranch, Factory);

					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(GlbBranch.CurrentBranch);
					switcher.StmServiceTasks[0].SST_Active = false;
					switcher.StmServiceTasks[1].SST_GB_Branch = ZGuid.NewZGuid();
					switcher.ScheduledReports[0].S5_IsActive = false;
					switcher.ScheduledReports[1].S5_GB = ZGuid.NewZGuid();

					CombineAssertions("Precondition", () =>
					{
						Assert(switcher.StmServiceTasks[0].HasChanges);
						Assert(switcher.StmServiceTasks[1].HasChanges);

						Assert(switcher.ScheduledReports[0].HasChanges);
						Assert(switcher.ScheduledReports[1].HasChanges);
					});

					switcher.CancelBranchChanges();

					CombineAssertions(() =>
					{
						Assert(switcher.StmServiceTasks[0].SST_Active);
						AssertEquals(GlbBranch.CurrentBranch.PK, switcher.StmServiceTasks[0].SST_GB_Branch);
						Assert(switcher.StmServiceTasks[1].SST_Active);
						AssertEquals(GlbBranch.CurrentBranch.PK, switcher.StmServiceTasks[1].SST_GB_Branch);

						Assert(switcher.ScheduledReports[0].S5_IsActive);
						AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ScheduledReports[0].S5_GB);
						Assert(switcher.ScheduledReports[1].S5_IsActive);
						AssertEquals(GlbBranch.CurrentBranch.PK, switcher.ScheduledReports[1].S5_GB);
					});
				}
			}

			public void TestCompanyLevel()
			{
				using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock(isMandatory: false, "111", "222", "333")))
				using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("111", "222", "333")))
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					var branch1 = Factory.NewWithValidTestData<GlbBranch>();
					var branch2 = Factory.NewWithValidTestData<GlbBranch>();
					var branch3 = Factory.NewWithValidTestData<GlbBranch>();
					company.Branches.Add(branch1);
					company.Branches.Add(branch2);

					var task1 = CreateStmServiceTask(branch1, "111");
					var task2 = CreateStmServiceTask(branch2, "222");
					_ = CreateStmServiceTask(branch3, "333");

					var report1 = CreateReportTask(branch1, Factory);
					var report2 = CreateReportTask(branch2, Factory);
					_ = CreateReportTask(branch3, Factory);

					Factory.Save();

					var switcher = new BranchSwitcherBusinessObject(company);

					AssertEquals(2, switcher.StmServiceTasks.Count);
					AssertContainsExactElementsInAnyOrder(new[] { task1.PK, task2.PK }, switcher.StmServiceTasks.Select(o => o.PK));

					AssertEquals(2, switcher.ScheduledReports.Count);
					AssertContainsExactElementsInAnyOrder(new[] { report1.PK, report2.PK }, switcher.ScheduledReports.Select(o => o.PK));
				}
			}

			StmServiceTask CreateStmServiceTask(GlbBranch branch, string taskCode)
			{
				var task = Factory.NewWithValidTestData<StmServiceTask>();
				task.SST_ServiceTaskCode = taskCode;
				task.SST_GB_Branch = branch.PK;
				task.SST_Active = true;
				return task;
			}

			IServiceTaskScheduleStatusProvider GetServiceTaskStatusProviderMock(params string[] codes)
			{
				var webStatus = new Dictionary<string, TaskInstanceStatus>();

				foreach (var code in codes)
				{
					webStatus.Add(code, new TaskInstanceStatus { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running" });
				}

				return Mock.Of<IServiceTaskScheduleStatusProvider>(o => o.GetServiceStatus() == webStatus);
			}

			IClientHostedServiceAttributeProvider GetClientHostedServiceAttributeProviderMock(bool isMandatory, params string[] codes)
			{
				var hostedServiceConfigMocks = codes.Select(GetHostedServiceAttributeMock).ToArray();
				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();

				foreach (var mock in hostedServiceConfigMocks)
				{
					hostedServiceProviderMock
						.Setup(p => p.GetClientHostedServiceAttribute(mock.Code))
						.Returns(mock);
				}

				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttributes())
					.Returns(hostedServiceConfigMocks);

				return hostedServiceProviderMock.Object;

				IHostedServiceAttribute GetHostedServiceAttributeMock(string code)
				{
					return Mock.Of<IHostedServiceAttribute>(o =>
						o.Code == code &&
						o.Description == "Dummy Description" &&
						o.Category == "TST" &&
						o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
						o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes") &&
						o.IsMandatory == isMandatory);
				}
			}
		}

		StmScheduleTask CreateServiceTask(GlbBranch branch)
		{
			return CreateStmScheduleTask(branch.PK, ZString.Empty, StmServiceHostSchema.Constants.Prefix, ZGuid.Empty, Factory);
		}

		StmScheduleTask CreateReportTask(GlbBranch branch)
		{
			return CreateReportTask(branch, Factory);
		}
		public static StmScheduleTask CreateReportTask(GlbBranch branch, BusinessObjectFactory factory)
		{
			return CreateStmScheduleTask(branch.PK, "REP", StmMenuItemSchema.Constants.Prefix, ZGuid.NewZGuid(), factory);
		}

		static StmScheduleTask CreateStmScheduleTask(ZGuid branchPK, ZString scheduleType, ZString parentTableCode, ZGuid parentID, BusinessObjectFactory factory)
		{
			var task = factory.NewWithValidTestData<StmScheduleTask>();
			task.S5_GB = branchPK;
			task.S5_ScheduleType = scheduleType;
			task.S5_ParentTableCode = parentTableCode;
			task.S5_ParentID = parentID;

			return task;
		}

		GlbStaff CreateGlbStaff(ZGuid branchPK)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branchPK;

			return staff;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var rightBranch = Factory.NewWithValidTestData<GlbBranch>();
			return new BranchSwitcherBusinessObject(rightBranch);
		}
	}
}
