using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI
{
	partial class ZOrganisationControlWithMiscellaneous
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		ZTabPage MiscellaneousTab;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.MiscellaneousTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsTabControl.SuspendLayout();
			this.GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Controls.Add(this.MiscellaneousTab);
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 112, true);
			this.DetailsTabControl.Controls.SetChildIndex(this.MiscellaneousTab, 0);
			this.DetailsTabControl.Controls.SetChildIndex(this.ContactInfoTab, 0);
			this.DetailsTabControl.Controls.SetChildIndex(this.AddressTab, 0);
			// 
			// AddressTab
			// 
			this.AddressTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 85, true);
			// 
			// GroupBox
			// 
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			// 
			// MiscellaneousTab
			// 
			this.MiscellaneousTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscellaneousTab.Name = "MiscellaneousTab";
			this.MiscellaneousTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 85, true);
			this.MiscellaneousTab.TabIndex = 3;
			this.MiscellaneousTab.CaptionResourceString = Res.GetData("1f1c8e94-c40b-46da-8c1f-f82e8d830349", "Miscellaneous");
			// 
			// ZOrganisationControlWithMiscellaneous
			// 
			this.Name = "ZOrganisationControlWithMiscellaneous";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.DetailsTabControl.ResumeLayout(false);
			this.GroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
