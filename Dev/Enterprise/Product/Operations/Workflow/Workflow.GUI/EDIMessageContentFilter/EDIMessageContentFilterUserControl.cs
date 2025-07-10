using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class EDIMessageContentFilterUserControl : ZUserControl
	{
		public EDIMessageContentFilterUserControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				SuspendLayout();
				InitializeSchemaTab(EDIMessageContentFilterLineSchemas.Codes.UniversalEvent, EDIMessageContentFilterLineSchemas.Descriptions.UniversalEvent, nameof(EDIMessageContentFilter.UniversalEvent));
				InitializeSchemaTab(EDIMessageContentFilterLineSchemas.Codes.UniversalShipment, EDIMessageContentFilterLineSchemas.Descriptions.UniversalShipment, nameof(EDIMessageContentFilter.UniversalShipment));
				InitializeSchemaTab(EDIMessageContentFilterLineSchemas.Codes.UniversalTransaction, EDIMessageContentFilterLineSchemas.Descriptions.UniversalTransaction, nameof(EDIMessageContentFilter.UniversalTransaction));
				InitializeSharedAdditionalConfigurationTab();
				ResumeLayout();
			}

			CaptionRenderingEnabled = true;
		}

		void InitializeSchemaTab(string code, MultilingualString description, string bindingMember)
		{
			var control = new EDIMessageContentFilterSpecUserControl(code);
			control.Dock = System.Windows.Forms.DockStyle.Fill;
			var page = new ZTabPage
			{
				Text = Res.GetString("EDIMessageContentFilterUserControl|TabFormat", "{0} - {1}", code, description)
			};
			page.Controls.Add(control);
			tabControl1.TabPages.Add(page);
			BindingSource.SetBindingMember(control, bindingMember);
		}

		void InitializeSharedAdditionalConfigurationTab()
		{
			var control = new EDIMessageContentFilterSharedAdditionalConfigurationUserControl();
			control.Dock = System.Windows.Forms.DockStyle.Fill;
			var page = new ZTabPage
			{
				Text = Res.GetString("63D173EC-F1E6-46E4-B641-F384F46FE4E1", "Shared Additional Configuration")
			};
			page.Controls.Add(control);
			tabControl1.TabPages.Add(page);
			BindingSource.SetBindingMember(control, nameof(EDIMessageContentFilter.Config));
		}
	}
}
