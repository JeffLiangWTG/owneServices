namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class InBondUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.InBondGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportInBondDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MexicanPedimentoNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EstimatedDateOfUSExitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ForeignPortOfDestinationDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ForeignPortOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransferCarrierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InBondNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondedCarrierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OnwardCarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InbondDestinationDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InbondDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InBondEntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InBondGroupBox.SuspendLayout();
			this.ExportInBondDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.InBond);
			// 
			// InBondGroupBox
			// 
			this.InBondGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|63e72334-454d-46de-b58b-1746a02a3549", "In-Bond Details");
			this.InBondGroupBox.Controls.Add(this.ExportInBondDetailsGroupBox);
			this.InBondGroupBox.Controls.Add(this.TransferCarrierTextBox);
			this.InBondGroupBox.Controls.Add(this.InBondNumberTextBox);
			this.InBondGroupBox.Controls.Add(this.BondedCarrierTextBox);
			this.InBondGroupBox.Controls.Add(this.OnwardCarrierCodeFindBox);
			this.InBondGroupBox.Controls.Add(this.InbondDestinationDCodeFindBox);
			this.InBondGroupBox.Controls.Add(this.InbondDestinationCodeFindBox);
			this.InBondGroupBox.Controls.Add(this.InBondEntryTypeDropEdit);
			this.InBondGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InBondGroupBox.Name = "InBondGroupBox";
			this.InBondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 269, true);
			this.InBondGroupBox.TabIndex = 0;
			this.InBondGroupBox.TabStop = false;
			// 
			// ExportInBondDetailsGroupBox
			// 
			this.ExportInBondDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ExportInBondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|321742d5-c429-4684-a2d2-de966e0212fe", "I.E. or T&&E");
			this.ExportInBondDetailsGroupBox.Controls.Add(this.MexicanPedimentoNumberTextBox);
			this.ExportInBondDetailsGroupBox.Controls.Add(this.EstimatedDateOfUSExitDateEdit);
			this.ExportInBondDetailsGroupBox.Controls.Add(this.ForeignPortOfDestinationDCodeFindBox);
			this.ExportInBondDetailsGroupBox.Controls.Add(this.ForeignPortOfDestinationCodeFindBox);
			this.ExportInBondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 171, true);
			this.ExportInBondDetailsGroupBox.Name = "ExportInBondDetailsGroupBox";
			this.ExportInBondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 95, true);
			this.ExportInBondDetailsGroupBox.TabIndex = 7;
			this.ExportInBondDetailsGroupBox.TabStop = false;
			// 
			// MexicanPedimentoNumberTextBox
			// 
			this.MexicanPedimentoNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MexicanPedimentoNumberTextBox, "BM_PedimentoNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_PedimentoNumber)));
			this.MexicanPedimentoNumberTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|20491a8f-4f62-427b-b477-136e56570bab", "Mexican Pedimento Number", "The Mexican entry number: required for all Immediate Export in-bonds to Mexico from Mexican border ports.");
			this.MexicanPedimentoNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 71, true);
			this.MexicanPedimentoNumberTextBox.Name = "MexicanPedimentoNumberTextBox";
			this.MexicanPedimentoNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MexicanPedimentoNumberTextBox.TabIndex = 3;
			// 
			// EstimatedDateOfUSExitDateEdit
			// 
			this.EstimatedDateOfUSExitDateEdit.AllowDrop = true;
			this.EstimatedDateOfUSExitDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedDateOfUSExitDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedDateOfUSExitDateEdit, "BM_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_ExportDate)));
			this.EstimatedDateOfUSExitDateEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|18edc7d3-532c-4f27-b9f1-65448a06ad24", "Estimated Date of US Exit", "Estimated date of exit from the US-for use with In-transit, Export, and Inbond TE.");
			this.EstimatedDateOfUSExitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 45, true);
			this.EstimatedDateOfUSExitDateEdit.Name = "EstimatedDateOfUSExitDateEdit";
			this.EstimatedDateOfUSExitDateEdit.TabIndex = 2;
			// 
			// ForeignPortOfDestinationDCodeFindBox
			// 
			this.ForeignPortOfDestinationDCodeFindBox.AllowDrop = true;
			this.ForeignPortOfDestinationDCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ForeignPortOfDestinationDCodeFindBox, "BM_ForeignDestPortKCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_ForeignDestPortKCode)));
			this.ForeignPortOfDestinationDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|3a78dddf-c249-4e0c-b72e-7240a9635d98", "Schedule K", "Foreign Port of Destination (Schedule K)", "Foreign Port of destination of cargo (Schedule K). Required for T&E and IE cargo.");
			this.ForeignPortOfDestinationDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 19, true);
			this.ForeignPortOfDestinationDCodeFindBox.Name = "ForeignPortOfDestinationDCodeFindBox";
			this.ForeignPortOfDestinationDCodeFindBox.PreBoundMaxLength = 5;
			this.ForeignPortOfDestinationDCodeFindBox.ShowDescriptionBox = false;
			this.ForeignPortOfDestinationDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.ForeignPortOfDestinationDCodeFindBox.TabIndex = 1;
			// 
			// ForeignPortOfDestinationCodeFindBox
			// 
			this.ForeignPortOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForeignPortOfDestinationCodeFindBox, "BM_RL_NKForeignDestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_RL_NKForeignDestPort)));
			this.ForeignPortOfDestinationCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|4fe74c5c-f83b-4320-8fb8-7f70cad3a7f6", "Foreign Port of Destination", "Foreign Port of destination of cargo (UNLOCO). Required for T&E and IE cargo.");
			this.ForeignPortOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 19, true);
			this.ForeignPortOfDestinationCodeFindBox.Name = "ForeignPortOfDestinationCodeFindBox";
			this.ForeignPortOfDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.ForeignPortOfDestinationCodeFindBox.ShowDescriptionBox = false;
			this.ForeignPortOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.ForeignPortOfDestinationCodeFindBox.TabIndex = 0;
			// 
			// TransferCarrierTextBox
			// 
			this.TransferCarrierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransferCarrierTextBox, "BM_TransferCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_TransferCarrier)));
			this.TransferCarrierTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|efbc1166-641f-40de-bc28-f00096ba2e8b", "Transfer Carrier (IRS)", "IRS number of transfer carrier which is going to locally transfer the cargo to another carrier.");
			this.TransferCarrierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 149, true);
			this.TransferCarrierTextBox.Name = "TransferCarrierTextBox";
			this.TransferCarrierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransferCarrierTextBox.TabIndex = 6;
			// 
			// InBondNumberTextBox
			// 
			this.InBondNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InBondNumberTextBox, "InBondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).InBondNumber)));
			this.InBondNumberTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|70d939aa-020a-455b-889f-a6d340cf937d", "Inbond 7512 Number", "Number issued for In-bond cargo.");
			this.InBondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 123, true);
			this.InBondNumberTextBox.Name = "InBondNumberTextBox";
			this.InBondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.InBondNumberTextBox.TabIndex = 5;
			// 
			// BondedCarrierTextBox
			// 
			this.BondedCarrierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BondedCarrierTextBox, "BM_InBondCarrierID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_InBondCarrierID)));
			this.BondedCarrierTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|00d1825b-a650-4222-bdbc-29f01ac0b4b6", "Bonded Carrier (IRS)", "A code representing the identification number of the bonded carrier. Also referred to as the Importer or IRS number. This is the carrier used to transfer domestic within the US.");
			this.BondedCarrierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 97, true);
			this.BondedCarrierTextBox.Name = "BondedCarrierTextBox";
			this.BondedCarrierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BondedCarrierTextBox.TabIndex = 4;
			// 
			// OnwardCarrierCodeFindBox
			// 
			this.OnwardCarrierCodeFindBox.AllowDrop = true;
			this.OnwardCarrierCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OnwardCarrierCodeFindBox, "BM_OnwardCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_OnwardCarrier)));
			this.OnwardCarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|e43e5e8a-7e31-409d-9465-586f17c1c860", "Onward Carrier (SCAC)", "SCAC code of Onward carrier to whom in bond goods are being transferred, if applicable. This is the carrier who takes it out of the country for an Inbond TE or IE.");
			this.OnwardCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 71, true);
			this.OnwardCarrierCodeFindBox.Name = "OnwardCarrierCodeFindBox";
			this.OnwardCarrierCodeFindBox.PreBoundMaxLength = 4;
			this.OnwardCarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OnwardCarrierCodeFindBox.TabIndex = 3;
			// 
			// InbondDestinationDCodeFindBox
			// 
			this.InbondDestinationDCodeFindBox.AllowDrop = true;
			this.InbondDestinationDCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InbondDestinationDCodeFindBox, "BM_DestinationPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_DestinationPortCode)));
			this.InbondDestinationDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|f7f17549-0091-4401-90d1-298e20f65e17", "Schedule D", "Inbond Destination (Schedule D)", "In-bond port/point of where Entry is filed or the port/point of exit - where the cargo clears (Schedule D).");
			this.InbondDestinationDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 45, true);
			this.InbondDestinationDCodeFindBox.Name = "InbondDestinationDCodeFindBox";
			this.InbondDestinationDCodeFindBox.PreBoundMaxLength = 4;
			this.InbondDestinationDCodeFindBox.ShowDescriptionBox = false;
			this.InbondDestinationDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.InbondDestinationDCodeFindBox.TabIndex = 2;
			// 
			// InbondDestinationCodeFindBox
			// 
			this.InbondDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InbondDestinationCodeFindBox, "BM_RL_NKDestinationPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_RL_NKDestinationPort)));
			this.InbondDestinationCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|fc6c7555-4c38-47ea-8024-dbd88fcd1f3c", "Inbond Destination", "In-bond port/point of where Entry is filed or the port/point of exit - where the cargo clears (UNLOCO).");
			this.InbondDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 45, true);
			this.InbondDestinationCodeFindBox.Name = "InbondDestinationCodeFindBox";
			this.InbondDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.InbondDestinationCodeFindBox.ShowDescriptionBox = false;
			this.InbondDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.InbondDestinationCodeFindBox.TabIndex = 1;
			// 
			// InBondEntryTypeDropEdit
			// 
			this.InBondEntryTypeDropEdit.AllowDrop = true;
			this.InBondEntryTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InBondEntryTypeDropEdit, "BM_InBondEntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.InBond)(null)).BM_InBondEntryType)));
			this.InBondEntryTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("InBondUserControl|d812c002-9930-4315-87a2-ba2a45e7e493", "In-Bond Type", "Code creating entry for consignment. Needed by Express or Courier Consignment if doing Express Releases.");
			this.InBondEntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
			this.InBondEntryTypeDropEdit.Name = "InBondEntryTypeDropEdit";
			this.InBondEntryTypeDropEdit.PreBoundMaxLength = 2;
			this.InBondEntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.InBondEntryTypeDropEdit.TabIndex = 0;
			// 
			// InBondUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InBondGroupBox);
			this.Name = "InBondUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 269, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InBondGroupBox.ResumeLayout(false);
			this.InBondGroupBox.PerformLayout();
			this.ExportInBondDetailsGroupBox.ResumeLayout(false);
			this.ExportInBondDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox InBondGroupBox;
		private ZArchitecture.GUI.ZDropEdit InBondEntryTypeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox InbondDestinationCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox InbondDestinationDCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox OnwardCarrierCodeFindBox;
		private ZArchitecture.ZTextBox BondedCarrierTextBox;
		private ZArchitecture.ZTextBox InBondNumberTextBox;
		private ZArchitecture.ZTextBox TransferCarrierTextBox;
		private ZArchitecture.GUI.ZGroupBox ExportInBondDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox ForeignPortOfDestinationDCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ForeignPortOfDestinationCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit EstimatedDateOfUSExitDateEdit;
		private ZArchitecture.ZTextBox MexicanPedimentoNumberTextBox;
	}
}
