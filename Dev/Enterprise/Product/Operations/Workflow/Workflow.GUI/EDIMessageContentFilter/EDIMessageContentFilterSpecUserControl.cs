using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class EDIMessageContentFilterSpecUserControl : ZUserControl
	{
		public EDIMessageContentFilterSpecUserControl(string code)
		{
			InitializeComponent(code);
			CaptionRenderingEnabled = true;
		}

		void InitializeComponent(string code)
		{
			InitializeComponent();
			InitializeSchemaElementsTabs();
			InitializeDocumentsTabs(code);
			InitializeAdditionalConfigurationTab(code);
		}

		void InitializeSchemaElementsTabs()
		{
			var control = new EDIMessageContentFilterLineUserControl();
			control.Dock = System.Windows.Forms.DockStyle.Fill;
			var page = new ZTabPage
			{
				Text = Res.GetString("4E7D4269-F2B9-4F3A-A500-70556981DE9B", "Schema Elements")
			};
			page.Controls.Add(control);
			tabControl1.TabPages.Add(page);
			BindingSource.SetBindingMember(control, ".");
		}

		void InitializeDocumentsTabs(string code)
		{
			if (EDIMessageContentFilterSpec.SupportsDocuments(code))
			{
				var control = new EDIMessageFilterContentFilterDocumentUserControl();
				control.Dock = System.Windows.Forms.DockStyle.Fill;
				var page = new ZTabPage
				{
					Text = Res.GetString("7ADD700E-EDC9-4CA5-BADD-F508F0EB510F", "Document Types")
				};
				page.Controls.Add(control);
				tabControl1.TabPages.Add(page);
				BindingSource.SetBindingMember(control, ".");
			}
		}

		void InitializeAdditionalConfigurationTab(string code)
		{
			if (EDIMessageContentFilterSpec.SupportsAdditionalConfiguration(code))
			{
				var control = new EDIMessageContentFilterAdditionalConfigurationUserControl();
				control.Dock = System.Windows.Forms.DockStyle.Fill;
				var page = new ZTabPage
				{
					Text = Res.GetString("9acf2cd8-8009-48ff-abbc-12e876d9508b", "Additional Configuration")
				};
				page.Controls.Add(control);
				tabControl1.TabPages.Add(page);
				BindingSource.SetBindingMember(control, ".");
			}
		}
	}
}
