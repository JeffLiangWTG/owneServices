namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	partial class MAFeBACCaContainerUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MPINumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerPackingAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainerPackLocation = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SealingPartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ContainerTypeDropEdit.SuspendLayout();
			this.ContainerPackingAddressGroupBox.SuspendLayout();
			this.ContainerPackLocation.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.CusContainerCollection);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.SealingPartyTextBox);
			this.DetailsGroupBox.Controls.Add(this.ContainerTypeDropEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("AA0DD2C2-7A8D-4E50-9FEF-CD89132AA676", "Details");
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 80, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ContainerTypeDropEdit
			// 
			this.ContainerTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerTypeDropEdit, "CO_MAF_ContainerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).CO_MAF_ContainerType)));
			this.ContainerTypeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaContainerUserControl|7b549a4e-8132-4753-8fa7-4d9af43b0995", "Container Type", "MPI Container Type", "The type of container using the list of Container Types specified by MPI.");
			this.ContainerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 15, true);
			this.ContainerTypeDropEdit.Name = "ContainerTypeDropEdit";
			this.ContainerTypeDropEdit.PreBoundMaxLength = 3;
			this.ContainerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.ContainerTypeDropEdit.TabIndex = 0;
			// 
			// MPINumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MPINumberTextBox, "CO_MPIApprovedSystemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).CO_MPIApprovedSystemNumber)));
			this.MPINumberTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("61f8b430-2a72-402c-ba90-76e7143c10e6", "", "MPI Number", "MPI Approved System Number", "");
			this.MPINumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 105, true);
			this.MPINumberTextBox.Name = "MPINumberTextBox";
			this.MPINumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.MPINumberTextBox.TabIndex = 1;
			// 
			// ContainerPackingAddressGroupBox
			// 
			this.ContainerPackingAddressGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ContainersUserControl|7A618C09-815A-457D-9DA4-47078A3C5966", "Container Pack Location");
			this.ContainerPackingAddressGroupBox.Controls.Add(this.ContainerPackLocation);
			this.ContainerPackingAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 3, true);
			this.ContainerPackingAddressGroupBox.Name = "ContainerPackingAddressGroupBox";
			this.ContainerPackingAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 80, true);
			this.ContainerPackingAddressGroupBox.TabIndex = 11;
			this.ContainerPackingAddressGroupBox.TabStop = false;
			// 
			// ContainerPackLocation
			// 
			this.ContainerPackLocation.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerPackLocation, "CO_OA_PackingLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).CO_OA_PackingLocation)));
			this.ContainerPackLocation.BindToOrgList = "JobContainer.Lookups+ContainerYardList";
			this.ContainerPackLocation.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5d1ebe62-d400-4901-bc51-97bfc5290ce3", "", "For containerized cargo state the name of the party that packed the container");
			this.ContainerPackLocation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerPackLocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainerPackLocation.Name = "ContainerPackLocation";
			this.ContainerPackLocation.PopupCaption = "";
			this.ContainerPackLocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 58, true);
			this.ContainerPackLocation.TabIndex = 0;
			// 
			// SealingPartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.SealingPartyTextBox, "CO_SealingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).CO_SealingParty)));
			this.SealingPartyTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4a138deb-4d9b-4c5a-bb35-63bd952815e0", "", "Sealing Party", "Sealing Party Name", "State the name of the party affixing the Customs-approved seal");
			this.SealingPartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 48, true);
			this.SealingPartyTextBox.Name = "SealingPartyTextBox";
			this.SealingPartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.SealingPartyTextBox.TabIndex = 1;
			// 
			// MAFeBACCaContainerUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MPINumberTextBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.ContainerPackingAddressGroupBox);
			this.Name = "MAFeBACCaContainerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ContainerTypeDropEdit.ResumeLayout(true);
			this.ContainerTypeDropEdit.PerformLayout();
			this.ContainerPackingAddressGroupBox.ResumeLayout(false);
			this.ContainerPackingAddressGroupBox.PerformLayout();
			this.ContainerPackLocation.ResumeLayout(true);
			this.ContainerPackLocation.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ContainerTypeDropEdit;
		internal ZArchitecture.ZTextBox MPINumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ContainerPackingAddressGroupBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ContainerPackLocation;
		private ZArchitecture.ZTextBox SealingPartyTextBox;
	}
}
