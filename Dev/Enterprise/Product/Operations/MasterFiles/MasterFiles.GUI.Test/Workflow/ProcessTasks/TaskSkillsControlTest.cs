using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class TaskSkillsControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var task = Factory.New<ProcessTask>();
			return CreateForm(task);
		}

		static ZForm CreateForm(ProcessTask task)
		{
			var result = new ZEmptyFormForBasherTest
			{
				CaptionRenderingEnabled = true,
				ControllerID = ControllerIDs.ProcessTasks,
			};
			var userControl = new TaskSkillsControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(task, null);
			result.Size = new Size(1000, 700);
			return result;
		}

		public void TestBindTo()
		{
			using (var control = new TaskSkillsControl())
			{
				var startingBindTo = control.BindTo;
				control.BindTo = "TasksView";
				AssertEquals("Should set same BindTo as control", "TasksView", control.BindTo);
				AssertStartsWith("JobSkillsGrid.BindToGridList should bet set", "TasksView.", control.JobSkillsGrid.BindTo);
			}
		}

		public void TestOpenLearningCentre_ForWiseTechAcademySubject()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DEA";
			Factory.Save();

			var task = Factory.New<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();
			pivot.P9S_WiseTechAcademySubjectCode = "123";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = CreateForm(task))
			{
				form.Show();
				var control = form.FindSingle<TaskSkillsControl>();
				control.OpenLearningCentre();
				AssertEquals("https://myaccount-portal.cargowise.com/myaccount/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx?path=assess&target=123", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestOpenLearningCentre_ForAspect()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DEA";
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			var task = Factory.New<ProcessTask>();
			task.SkillsPivots.AddAspect(aspectPK);

			var assessServiceClientMock = new Mock<IAssessServiceClient>();
			assessServiceClientMock
				.Setup(c => c.GetLearningUnitNameAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "123"));
			assessServiceClientMock
				.Setup(c => c.GetLearningUnitUrlAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "https://nothing/assess/123"));

			using (ObjectFactory.Substitute(assessServiceClientMock.Object))
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = CreateForm(task))
			{
				form.Show();
				var control = form.FindSingle<TaskSkillsControl>();
				control.OpenLearningCentre();
				AssertEquals("https://nothing/assess/123", WebUrlLauncher.LastUrlLaunched);
			}
		}

		[RequiresSTA]
		public void TestOpenLearningCentre_ForPivotWithAspectAndWTASubject()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DEA";
			Factory.Save();

			var aspectPK = Guid.NewGuid();
			var task = Factory.New<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();
			pivot.P9S_Aspect = aspectPK;
			pivot.P9S_WiseTechAcademySubjectCode = "123";

			var assessServiceClientMock = new Mock<IAssessServiceClient>();
			assessServiceClientMock
				.Setup(c => c.GetLearningUnitNameAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "123"));
			assessServiceClientMock
				.Setup(c => c.GetLearningUnitUrlAsync(aspectPK, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "https://nothing/assess/123"));

			using (ObjectFactory.Substitute(assessServiceClientMock.Object))
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var form = CreateForm(task))
			{
				form.Show();
				var control = form.FindSingle<TaskSkillsControl>();
				control.OpenLearningCentre();
				AssertEquals("https://nothing/assess/123", WebUrlLauncher.LastUrlLaunched);
			}
		}
	}
}
