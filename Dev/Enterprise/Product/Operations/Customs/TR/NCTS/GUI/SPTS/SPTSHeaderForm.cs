using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class SPTSHeaderForm : ZTemplateForm
	{
		public SPTSHeaderForm(SPTSHeader header) : base(header)
		{
			InitializeComponent();
			AddSPTSMenu();
			AddItemToActionsMenu();
		}

		public new SPTSHeader BusinessEntity => (SPTSHeader)base.BusinessEntity;

		void AddSPTSMenu()
		{
			var actionMenuItemIndex = MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			var sptsMenu = new TRSPTSMenu(BusinessEntity);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 1, sptsMenu);
		}

		void AddItemToActionsMenu()
		{
			var actionMenuItem = MainMenu.MenuItems[2];
			var sptsActionMenu = new TRSPTSActionsMenu(BusinessEntity, (ZMenuItem)actionMenuItem);
			if (sptsActionMenu != null)
			{
				var manualRegistrationMenuItem = sptsActionMenu.MenuItems[0];
				actionMenuItem.MenuItems.Add(manualRegistrationMenuItem);
			}
		}

		public override string FormCaption => BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption;

		void MessagesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessagesUserControl = new SPTSMessagesTabUserControl();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.MessagesUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1099, 672, true);
			this.MessagesUserControl.TabIndex = 0;
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);
		}
	}
}
