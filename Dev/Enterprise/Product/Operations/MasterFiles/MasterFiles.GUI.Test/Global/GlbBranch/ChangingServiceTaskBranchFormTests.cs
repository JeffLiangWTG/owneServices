using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ChangingServiceTaskBranchForm))]
	sealed class ChangingServiceTaskBranchFormTests : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "JCD";

			Factory.NewWithValidTestData<ServiceTaskSchedule>().S5_GB = branch.PK;
			Factory.Save();

			return new ChangingServiceTaskBranchForm(new BranchSwitcherBusinessObject(branch));
		}

		public void TestLabelDoesntContainLiteralSlashN()
		{
			using (var form = GetFormToBash())
			{
				var label = (ZLabel)form.Controls.Find("instructionsLabel", true).Single();
				Assert("Label shouldn't contain a literal \\n instead of a real newline: " + label.Text, !(label.Text.Contains("\\n") || label.Text.Contains("\\r")));
			}
		}

		[RequiresSTA]
		public void TestClickingCancelUndosChanges()
		{
			var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();

			var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			task.S5_GB = oldBranch.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = oldBranch.PK;

			Factory.Save();

			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ChangingServiceTaskBranchForm(new BranchSwitcherBusinessObject(oldBranch)))
			{
				form.Show();

				form.BusinessEntity.ReplacementBranch = newBranch.PK;
				AssertEquals("PRE: Should have updated the linked task", newBranch.PK, task.S5_GB);
				AssertEquals("PRE: Should have updated the linked staff", newBranch.PK, staff.GS_GB_HomeBranch);

				((ZButton)form.Controls.Find("cancelButton", true).Single()).PerformClick();
			}

			AssertEquals("Any changes should have been undone by the cancel button", oldBranch.PK, task.S5_GB);
			AssertEquals("Any changes should have been undone by the cancel button", oldBranch.PK, staff.GS_GB_HomeBranch);
		}

		[RequiresSTA]
		public void TestClickingCancelUndosChanges_NewServiceTasksModule()
		{
			var code = "111";
			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(o => o.Code == code);
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(code))
				.Returns(hostedServiceAttribute);
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttributes())
				.Returns(new[] { hostedServiceAttribute });

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var oldBranch = Factory.NewWithValidTestData<GlbBranch>();
				var newBranch = Factory.NewWithValidTestData<GlbBranch>();

				var task = Factory.NewWithValidTestData<StmServiceTask>();
				task.SST_ServiceTaskCode = code;
				task.SST_GB_Branch = oldBranch.PK;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = oldBranch.PK;

				Factory.Save();
			
				using (var form = new ChangingServiceTaskBranchForm(new BranchSwitcherBusinessObject(oldBranch)))
				{
					form.Show();

					form.BusinessEntity.ReplacementBranch = newBranch.PK;
					AssertEquals("PRE: Should have updated the linked task", newBranch.PK, task.SST_GB_Branch);
					AssertEquals("PRE: Should have updated the linked staff", newBranch.PK, staff.GS_GB_HomeBranch);

					((ZButton)form.Controls.Find("cancelButton", true).Single()).PerformClick();
				}

				AssertEquals("Any changes should have been undone by the cancel button", oldBranch.PK, task.SST_GB_Branch);
				AssertEquals("Any changes should have been undone by the cancel button", oldBranch.PK, staff.GS_GB_HomeBranch);
			}
		}

		[RequiresSTA]
		public void TestServiceTasksAreDisplayed()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ChangingServiceTaskBranchForm())
			{
				form.Show();

				var serviceTasksGrid = form.Controls.Find("gridServiceTasks", searchAllChildren: true).Single();
				var stmServiceTasksGrid = form.Controls.Find("gridStmServiceTasks", searchAllChildren: true).Single();

				Assert(serviceTasksGrid.Visible);
				Assert(!stmServiceTasksGrid.Visible);
			}
		}

		[RequiresSTA]
		public void TestStmServiceTasksAreDisplayed()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ChangingServiceTaskBranchForm())
			{
				form.Show();

				var serviceTasksGrid = form.Controls.Find("gridServiceTasks", searchAllChildren: true).Single();
				var stmServiceTasksGrid = form.Controls.Find("gridStmServiceTasks", searchAllChildren: true).Single();

				Assert(stmServiceTasksGrid.Visible);
				Assert(!serviceTasksGrid.Visible);
			}
		}
	}
}
