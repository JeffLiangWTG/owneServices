using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectInformationControlTest : TestCaseWithFactory
	{
		public void TestSetFieldCaptions()
		{
			ProcessManagementRegistry.Instance.ProjectTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ONE");
			ProcessManagementRegistry.Instance.ProjectSubtypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TWO");
			ProcessManagementRegistry.Instance.ProjectModuleLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "THREE");
			ProcessManagementRegistry.Instance.ProjectPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOUR");
			var project = Factory.New<Project>();

			using (var form = new ZForm(project))
			using (var control = new ProjectInformationControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("ONE", control.GetProjectTypeDropEdit().GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("TWO", control.GetProjectSubTypeDropEdit().GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("THREE", control.GetProjectModuleDropEdit().GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("FOUR", control.GetPriorityDropEdit().GetExtension<LabelCaptionRenderer>().Caption);
			}
		}
	}
}
