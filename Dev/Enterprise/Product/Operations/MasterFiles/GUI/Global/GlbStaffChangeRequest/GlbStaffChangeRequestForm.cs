using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffChangeRequestForm : ZTemplateForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.System;

		GlbStaffChangeRequest changeRequest => (GlbStaffChangeRequest)DataSource;

		protected override bool SupportsEDocs => false;

		public GlbStaffChangeRequestForm(GlbStaffChangeRequest businessEntity) : base(businessEntity)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(businessEntity);
			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.GlbStaffChangeRequest;
			}
		}

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TemplateGuidFindBox = new ZGuidFindBox();
			this.StatusTextBox = new ZTextBox();
			this.DetailsGroupBox = new ZGroupBox();
			this.ChangeRequestButton = new ZButton();
			this.MainTabPage.SuspendLayout();
			this.TemplateGuidFindBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// TemplateGuidFindBox
			// 
			this.TemplateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateGuidFindBox, "GCR_GSG_Template");
			this.TemplateGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5359c325-b74b-4e80-9990-1bb8c4795a4a", "Change Request Template");
			this.TemplateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 32, true);
			this.TemplateGuidFindBox.Name = "TemplateGuidFindBox";
			this.TemplateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TemplateGuidFindBox.ParentType = null;
			this.TemplateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 30, true);
			this.TemplateGuidFindBox.TabIndex = 0;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "GCR_Status");
			this.StatusTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7c2d02f8-d0f6-4473-8954-cbf771e00fc2", "Change Request Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 82, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 30, true);
			this.StatusTextBox.TabIndex = 1;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("51dc654b-7a95-404c-a251-359cdd7fc86d", "Details");
			this.DetailsGroupBox.Controls.Add(this.ChangeRequestButton);
			this.DetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.TemplateGuidFindBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 441, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ChangeRequestButton
			// 
			this.ChangeRequestButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bac985b6-3327-4a78-84f9-58249fef4354", "View Change Request in GLOW");
			this.ChangeRequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 142, true);
			this.ChangeRequestButton.Name = "ChangeRequestButton";
			this.ChangeRequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 30, true);
			this.ChangeRequestButton.TabIndex = 2;
			this.ChangeRequestButton.ToolTipCaption = null;
			this.ChangeRequestButton.UseVisualStyleBackColor = true;
			this.ChangeRequestButton.Click += new EventHandler(this.ChangeRequestButton_Click);
			this.MainTabPage.PerformLayout();
			this.TemplateGuidFindBox.ResumeLayout(true);
			this.TemplateGuidFindBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			TemplateGuidFindBox.ReadOnly = true;
			StatusTextBox.ReadOnly = true;
		}

		void ChangeRequestButton_Click(object sender, EventArgs e)
		{
			OpenInBrowser(changeRequest.PK);
		}

		static void OpenInBrowser(ZGuid changeRequestPK)
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrEmpty(baseURL))
			{
				var errorMessage = ResString.GetMultilingualString("A783BAD0-456A-48A2-9CAC-E53ACB84DFDC",
@"The Change Request record cannot be opened in a browser as GLOW has not been configured for this client.
Registry: ") + GlowRegistry.Instance.GlowPortalsUri.Category + "/" + GlowRegistry.Instance.GlowPortalsUri.Caption;
				Globals.Message.ShowError(errorMessage);
				return;
			}

			var url = UrlBuilder.GenerateURL(new Uri(baseURL), "goto/changeRequest", additionalQueryStrings: new[] { ("changeRequestPK", changeRequestPK.ToString()) });
			WebUrlLauncher.Launch(url.ToString());
		}
	}
}
