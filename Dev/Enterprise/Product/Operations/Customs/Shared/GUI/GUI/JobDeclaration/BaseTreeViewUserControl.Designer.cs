namespace Enterprise.Customs.GUI
{
	partial class BaseTreeViewUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.Windows.Forms.ImageList CustomsImageList;
		System.Windows.Forms.ContextMenu TreeViewContextMenu;
		Enterprise.ZArchitecture.GUI.ZMenuItem AddGroupMenuItem;
		Enterprise.ZArchitecture.GUI.ZMenuItem DeleteMenuItem;
		public Enterprise.Customs.GUI.InvoiceTreeView TreeView;
		System.ComponentModel.IContainer components = null;

		#region Designer generated code
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseTreeViewUserControl));
			this.CustomsImageList = new System.Windows.Forms.ImageList(this.components);
			this.TreeViewContextMenu = new System.Windows.Forms.ContextMenu();
			this.AddGroupMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.DeleteMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.TreeView = new Enterprise.Customs.GUI.InvoiceTreeView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CustomsImageList
			// 
			this.CustomsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("CustomsImageList.ImageStream")));
			this.CustomsImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.CustomsImageList.Images.SetKeyName(0, "");
			this.CustomsImageList.Images.SetKeyName(1, "");
			this.CustomsImageList.Images.SetKeyName(2, "");
			// 
			// TreeViewContextMenu
			// 
			this.TreeViewContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.AddGroupMenuItem,
			this.DeleteMenuItem });
			this.TreeViewContextMenu.Popup += new System.EventHandler(this.TreeViewContextMenu_Popup);
			// 
			// AddGroupMenuItem
			// 
			this.AddGroupMenuItem.Index = 0;
			this.AddGroupMenuItem.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CFE132B4-71A2-44AB-A1D2-B5FFB73ACA4D", "Add &Group");
			this.AddGroupMenuItem.Click += new System.EventHandler(this.AddGroupMenuItem_Click);
			// 
			// DeleteMenuItem
			// 
			this.DeleteMenuItem.Index = 1;
			this.DeleteMenuItem.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("386FC810-263E-419C-B90B-CBEC624CC4FE", "&Delete Item");
			this.DeleteMenuItem.Click += new System.EventHandler(this.DeleteMenuItem_Click);
			// 
			// TreeView
			// 
			this.TreeView.AllowDrop = true;
			this.TreeView.ContextMenu = this.TreeViewContextMenu;
			this.TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeView.ImageIndex = 0;
			this.TreeView.ImageList = this.CustomsImageList;
			this.TreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TreeView.Name = "TreeView";
			this.TreeView.SelectedImageIndex = 0;
			this.TreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 110, true);
			this.TreeView.TabIndex = 0;
			this.TreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
			this.TreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterSelect);
			this.TreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
			this.TreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeView_KeyDown);
			this.TreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
			// 
			// BaseTreeViewUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TreeView);
			this.Name = "BaseTreeViewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 110, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
