using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Testing
{
	[TestedType(typeof(ProjectContactPhoneDiallerUserControlFormForTest))]
	class ProjectContactPhoneDiallerUserControlTest : ZFormBasherTest
	{
		public void TestDeleteProject_NoExceptions()
		{
			var project = Factory.NewWithValidTestData<Project>();
			Factory.Save();
			using (var form = new ProjectFormForTest(project))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Delete;
				form.Show();
				AssertNoExceptionThrown(() => form.AcceptButton.PerformClick());
				Assert("Post-condition: project successfully deleted", project.IsDeleted);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_OC_Contact = contact.PK;
			Factory.Save();

			return new ProjectContactPhoneDiallerUserControlFormForTest(project);
		}

		#region Classes

		public class ProjectFormForTest : ProjectForm
		{
			public ProjectFormForTest(Project project)
				: base(project)
			{
			}

			protected override DialogResult ShowConfirmationForDelete()
			{
				return DialogResult.OK;
			}
		}

		public class ProjectContactPhoneDiallerUserControlFormForTest : ZForm
		{
			public ProjectContactPhoneDiallerUserControlFormForTest(Project project)
				: base(project)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				ProjectContactPhoneDiallerUserControl = new ProjectContactPhoneDiallerUserControl();
				Controls.Add(ProjectContactPhoneDiallerUserControl);
				BindingSource.SetBindingMember(ProjectContactPhoneDiallerUserControl, "WKP_OC_Contact");
				CaptionRenderingEnabled = true;
			}

			public ProjectContactPhoneDiallerUserControl ProjectContactPhoneDiallerUserControl;
		}

		#endregion

		#endregion
	}
}
