using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsigneeDetailsUserControl : OrganisationContainerControl
	{
		public ConsigneeDetailsUserControl()
		{
			InitializeComponent();
			SetControlVisibility();
			SetPaymentMethodCaptionString();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Parent == null)
			{
				return;
			}

			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				var country = Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				switch (country)
				{
					case Constants.CountryCodes.Australia:
						AUPaymentDetailsTabPage.IsAccessible = true;
						AUPaymentDetailsTabPage.TabVisible = true;
						break;

					case Constants.CountryCodes.Brazil:
						PaymentDetailsTabControl.Visible = false;
						BottomPanel.Controls.Add(CustomsDefaultsGroupBox);

						CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn);
						CustomsDefaultsGroupBox.Visible = true;
						break;
					case Constants.CountryCodes.India:
						BottomPanel.Controls.Add(CustomsDefaultsGroupBox);
						CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn);
						CustomsDefaultsGroupBox.Visible = true;
						break;
				}

				if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(country))
				{
					PaymentDetailsTabControl.Visible = false;
					BottomPanel.Controls.Add(CustomsDefaultsGroupBox);

					CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.EU.OrganisationConsigneePlugIn);
					switch (country)
					{
						case Constants.CountryCodes.UnitedKingdom:
							CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn);
							break;
						case Constants.CountryCodes.Germany:
							CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn);
							break;
						case Constants.CountryCodes.France:
							CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn);
							break;
						case Constants.CountryCodes.Spain:
							CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.ES.OrganisationConsigneePlugIn);
							break;
						case Constants.CountryCodes.Netherlands:
							CustomsDefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn);
							break;
					}

					CustomsDefaultsGroupBox.Visible = true;
				}

				PaymentDetailsTabControl.PlugIns.Add(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn);

				var countriesWhichRequirePaymentDetailsTabControl =
					new List<string>
						{
							Constants.CountryCodes.Australia,
							Constants.CountryCodes.UnitedStates,
							Constants.CountryCodes.UnitedKingdom,
							Constants.CountryCodes.Canada
						};

				if (!countriesWhichRequirePaymentDetailsTabControl.Contains(country))
				{
					PaymentDetailsTabControl.Visible = false;
				}
				splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;

				ContainerFillingRatioTolerancesTabPage.TabVisible = AdvOrmFeatureHelper.IsEnabled;
			}

			var orgHeader = ((OrgHeader)CurrentDataItem);
			if (orgHeader != null)
			{
				orgHeader.CompanyData.ImporterOverrideInfo.ValueChanged += ImporterOverrideValueChanged;
				ImporterOverrideValueChanged(null, null);
			}

			if (!Env.Security.OrgConsigneeModifyDetails.IsAllowed)
			{
				this.SetReadOnlyIncludingChildren(true);
			}
		}

		void ImporterOverrideValueChanged(object sender, EventArgs e)
		{
			var orgHeader = ((OrgHeader)CurrentDataItem);
			if (orgHeader != null)
			{
				ASNDefaultFieldTypeGrid.ReadOnly = !orgHeader.CompanyData?.ImporterOverride ?? false;
			}
		}

		void cfxUpliftEditLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if ((ParentForm is ZOrganisationsForm form) && form.OrganisationsTabControl != null)
			{
				form.OrganisationsTabControl.SelectedTab = form.ReceivablesTabPage;
				if (form.ReceivablesControl?.ARTabControl != null)
				{
					form.ReceivablesControl.ARTabControl.SelectedIndex = 0;
				}
			}
		}

		#region Security

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				OwnersRefInitialNumberButton.Enabled = ((OrgHeader)CurrentDataItem).SecurityProvider.HasModifyConsigneeDetailsSecurity;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			var orgHeader = ((OrgHeader)CurrentDataItem);
			if (orgHeader != null)
			{
				orgHeader.CompanyData.ImporterOverrideInfo.ValueChanged -= ImporterOverrideValueChanged;
			}

			if (disposing && (components != null))
			{
				if (CustomsDefaultsGroupBox != null)
				{
					CustomsDefaultsGroupBox.Dispose();
				}
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Setup

		void SetControlVisibility()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var consigneeDefaultDropEditVisible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia;
				ACRConsigneeDefaultDropEdit.Visible = consigneeDefaultDropEditVisible;
				SCRConsigneeDefaultDropEdit.Visible = consigneeDefaultDropEditVisible;

				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Constants.CountryCodes.SouthAfrica:
					case Constants.CountryCodes.NewZealand:
					case Constants.CountryCodes.Singapore:
						OM_IMPaymentMethodDropEdit.Visible = true;
						ProductImportAuditDropEdit.Visible = true;
						OB_CusPaidByDropEdit.Visible = false;
						break;
					case Constants.CountryCodes.Canada:
						OM_IMPaymentMethodDropEdit.Visible = false;
						ProductImportAuditDropEdit.Visible = false;
						OB_CusPaidByDropEdit.Visible = false;
						break;
					case Constants.CountryCodes.Taiwan:
						OM_IMPaymentMethodDropEdit.Visible = false;
						ProductImportAuditDropEdit.Visible = true;
						OB_CusPaidByDropEdit.Visible = true;
						break;
					default:
						OM_IMPaymentMethodDropEdit.Visible = false;
						ProductImportAuditDropEdit.Visible = true;
						OB_CusPaidByDropEdit.Visible = false;
						break;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "GetData caption")]
		void SetPaymentMethodCaptionString()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var currentCountryIsSouthAfrica = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.SouthAfrica;

				if (currentCountryIsSouthAfrica)
				{
					this.OM_IMPaymentMethodDropEdit.CaptionResourceString = Res.GetData("ConsigneeDetailsUserControl|75d6876e-fce3-453b-a34b-a58d41247a7c", "Paid By");
				}
				else
				{
					this.OM_IMPaymentMethodDropEdit.CaptionResourceString = Res.GetData("ConsigneeDetailsUserControl|b7448da3-7518-472f-af8a-ad3ab019d687", "Payment Method");
				}
			}
		}

		#endregion

		#region Owners Ref Number Fountain

		protected void OwnersRefInitialNumberButton_Click(object sender, EventArgs e)
		{
			OrgHeader org = (OrgHeader)((ZForm)ParentForm).BusinessEntity;
			var fountain = Env.NumberFountains.ImporterJobReference(org.PK.ToGuid());
			long oldValue = fountain.PeekPreliminary(((IDbConnected)org.Factory).Connection);

			ZForm fountainDialog = new NewNumberFountainDialog(fountain);

			if (org.IsInDatabase)
			{
				fountainDialog.Closed +=
					(s1, e1) =>
					{
						long newValue = fountain.PeekPreliminary(((IDbConnected)org.Factory).Connection);
						if (fountainDialog.DialogResult == DialogResult.OK && newValue != oldValue)
						{
							BusinessObjectFactory logsFactory = new BusinessObjectFactory { RefreshEnabled = false };
							OrgHeader orgInLogsFactory = logsFactory.Load<OrgHeader>(org.PK);
							if (orgInLogsFactory != null)
							{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
								orgInLogsFactory.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord,
									"Importer Job Reference Next No: " + newValue.ToString(CultureInfo.InvariantCulture));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							}
							logsFactory.Save();
						}
					};
			}

			ZFormModaliser.Show(fountainDialog, ParentForm);
		}

		#endregion

		#region Tabs Initialization

		void AUPaymentDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox = new ZCheckBox();
			this.OM_IMEftQuarantineFromImportCheckBox = new ZCheckBox();
			this.OM_IMEFTBankAccountBoundTextBox = new ZTextBox();
			this.OM_IMEFTBankBSBBoundTextBox = new ZTextBox();
			this.OM_IMMinEFTAmountBoundCalcEdit = new ZCalcEdit();
			this.OM_IMMaxEFTAmountBoundCalcEdit = new ZCalcEdit();
			this.OM_IMEftCustomsFromImportBoundCheckEdit = new ZCheckBox();
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit = new ZCheckBox();
			this.OM_IMIsGSTDeferredBoundCheckBox = new ZCheckBox();
			this.IsDutyDeferredCheckBox = new ZCheckBox();
			this.AUPaymentDetailsTabPage.SuspendLayout();
			this.AUPaymentDetailsTabPage.Controls.Add(this.IsDutyDeferredCheckBox);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMShowDutyOnWarehouseEntriesCheckBox);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMEftQuarantineFromImportCheckBox);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMEFTBankAccountBoundTextBox);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMEFTBankBSBBoundTextBox);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMMinEFTAmountBoundCalcEdit);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMMaxEFTAmountBoundCalcEdit);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMEftCustomsFromImportBoundCheckEdit);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit);
			this.AUPaymentDetailsTabPage.Controls.Add(this.OM_IMIsGSTDeferredBoundCheckBox);
			// 
			// OM_IMShowDutyOnWarehouseEntriesCheckBox
			// 
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMShowDutyOnWarehouseEntriesCheckBox, "MiscServ+OM_IMShowDutyOnWarehouseEntries");
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgMiscServ|OM_IMShowDutyOnWarehouseEntriesCheckBox", "Show Duty && Tax on N20 Lines");
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 107, true);
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.Name = "OM_IMShowDutyOnWarehouseEntriesCheckBox";
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
			this.OM_IMShowDutyOnWarehouseEntriesCheckBox.TabIndex = 9;
			// 
			// OM_IMEftQuarantineFromImportCheckBox
			// 
			this.OM_IMEftQuarantineFromImportCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMEftQuarantineFromImportCheckBox, "MiscServ+OM_IMEftQuarantineFromImport");
			this.OM_IMEftQuarantineFromImportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMEftQuarantineFromImportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 88, true);
			this.OM_IMEftQuarantineFromImportCheckBox.Name = "OM_IMEftQuarantineFromImportCheckBox";
			this.OM_IMEftQuarantineFromImportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 17, true);
			this.OM_IMEftQuarantineFromImportCheckBox.TabIndex = 8;
			// 
			// OM_IMEFTBankAccountBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMEFTBankAccountBoundTextBox, "MiscServ.OM_IMEFTBankAccount");
			this.OM_IMEFTBankAccountBoundTextBox.CaptionResourceString = null;
			this.OM_IMEFTBankAccountBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 95, true);
			this.OM_IMEFTBankAccountBoundTextBox.Name = "OM_IMEFTBankAccountBoundTextBox";
			this.OM_IMEFTBankAccountBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMEFTBankAccountBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.OM_IMEFTBankAccountBoundTextBox.TabIndex = 3;
			// 
			// OM_IMEFTBankBSBBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMEFTBankBSBBoundTextBox, "MiscServ.OM_IMEFTBankBSB");
			this.OM_IMEFTBankBSBBoundTextBox.CaptionResourceString = null;
			this.OM_IMEFTBankBSBBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 68, true);
			this.OM_IMEFTBankBSBBoundTextBox.Name = "OM_IMEFTBankBSBBoundTextBox";
			this.OM_IMEFTBankBSBBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMEFTBankBSBBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.OM_IMEFTBankBSBBoundTextBox.TabIndex = 2;
			// 
			// OM_IMMinEFTAmountBoundCalcEdit
			// 
			this.OM_IMMinEFTAmountBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMMinEFTAmountBoundCalcEdit, "MiscServ.OM_IMMinEFTAmount");
			this.OM_IMMinEFTAmountBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMMinEFTAmountBoundCalcEdit.DecimalPlaces = 2;
			this.OM_IMMinEFTAmountBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 14, true);
			this.OM_IMMinEFTAmountBoundCalcEdit.Name = "OM_IMMinEFTAmountBoundCalcEdit";
			this.OM_IMMinEFTAmountBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMMinEFTAmountBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.OM_IMMinEFTAmountBoundCalcEdit.TabIndex = 0;
			this.OM_IMMinEFTAmountBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_IMMaxEFTAmountBoundCalcEdit
			// 
			this.OM_IMMaxEFTAmountBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMMaxEFTAmountBoundCalcEdit, "MiscServ.OM_IMMaxEFTAmount");
			this.OM_IMMaxEFTAmountBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMMaxEFTAmountBoundCalcEdit.DecimalPlaces = 2;
			this.OM_IMMaxEFTAmountBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 41, true);
			this.OM_IMMaxEFTAmountBoundCalcEdit.Name = "OM_IMMaxEFTAmountBoundCalcEdit";
			this.OM_IMMaxEFTAmountBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMMaxEFTAmountBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.OM_IMMaxEFTAmountBoundCalcEdit.TabIndex = 1;
			this.OM_IMMaxEFTAmountBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_IMEftCustomsFromImportBoundCheckEdit
			// 
			this.OM_IMEftCustomsFromImportBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMEftCustomsFromImportBoundCheckEdit, "MiscServ.OM_IMEftCustomsFromImport");
			this.OM_IMEftCustomsFromImportBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMEftCustomsFromImportBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 69, true);
			this.OM_IMEftCustomsFromImportBoundCheckEdit.Name = "OM_IMEftCustomsFromImportBoundCheckEdit";
			this.OM_IMEftCustomsFromImportBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 17, true);
			this.OM_IMEftCustomsFromImportBoundCheckEdit.TabIndex = 7;
			// 
			// OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit
			// 
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit, "MiscServ.OM_IMEftHoldUntilPayAuthorised");
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 51, true);
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.Name = "OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit";
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit.TabIndex = 6;
			// 
			// OM_IMIsGSTDeferredBoundCheckBox
			// 
			this.OM_IMIsGSTDeferredBoundCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMIsGSTDeferredBoundCheckBox, "MiscServ.OM_IMIsGSTDeferred");
			this.OM_IMIsGSTDeferredBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMIsGSTDeferredBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 32, true);
			this.OM_IMIsGSTDeferredBoundCheckBox.Name = "OM_IMIsGSTDeferredBoundCheckBox";
			this.OM_IMIsGSTDeferredBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.OM_IMIsGSTDeferredBoundCheckBox.TabIndex = 5;
			// 
			// IsDutyDeferredCheckBox
			// 
			this.IsDutyDeferredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDutyDeferredCheckBox, "AUIsDutyDeferred");
			this.IsDutyDeferredCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("867b9697-d163-4c67-a261-e9c18a1f3497", "Duty Deferred");
			this.IsDutyDeferredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDutyDeferredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 14, true);
			this.IsDutyDeferredCheckBox.Name = "IsDutyDeferredCheckBox";
			this.IsDutyDeferredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.IsDutyDeferredCheckBox.TabIndex = 4;
			this.AUPaymentDetailsTabPage.PerformLayout();
			this.AUPaymentDetailsTabPage.ResumeLayout(true);
		}

		void ContainerPenaltiesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZDropEditColumnStyleInfo penaltyTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo penaltyDescTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo carrierFindBoxColumnStyleInfo = new ZOrganisationFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			FreeDayExclusionColumnStyleInfo freeDayExclusionColumnStyleInfo = new FreeDayExclusionColumnStyleInfo();
			DurationExclusionColumnStyleInfo durationExclusionColumnStyleInfo = new DurationExclusionColumnStyleInfo();
			this.ContainerDetentionGrid = new ZGrid();
			this.ContainerPenaltiesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerDetentionGrid)).BeginInit();
			this.ContainerDetentionGrid.SuspendLayout();
			this.ContainerPenaltiesTabPage.Controls.Add(this.ContainerDetentionGrid);
			// 
			// ContainerDetentionGrid
			// 
			this.ContainerDetentionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainerDetentionGrid, "ConsigneeContainerPenalties");
			this.ContainerDetentionGrid.CaptionVisible = false;
			penaltyTypeDropEditColumnStyleInfo.ColumnName = "PD_PenaltyType";
			penaltyTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			penaltyDescTextBoxColumnStyleInfo.ColumnName = "PenaltyDescription";
			penaltyDescTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			carrierFindBoxColumnStyleInfo.ColumnName = "PD_OH_Carrier";
			carrierFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "PD_OriginPortOrCountry";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo8.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c27b1149-0b32-4ed1-af42-86413044effe", "1st Free Day");
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			freeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			freeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			durationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			durationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContainerDetentionGrid.ColumnStyles.Add(penaltyTypeDropEditColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(penaltyDescTextBoxColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(carrierFindBoxColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.ContainerDetentionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainerDetentionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ContainerDetentionGrid.ColumnStyles.Add(freeDayExclusionColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(durationExclusionColumnStyleInfo);

			this.ContainerDetentionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerDetentionGrid.GridId = "801d3c9c-e6af-4bba-9611-b825a3058b9d";
			this.ContainerDetentionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerDetentionGrid.LayoutKey = "zGrid1";
			this.ContainerDetentionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerDetentionGrid.Name = "ContainerDetentionGrid";
			this.ContainerDetentionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 206, true);
			this.ContainerDetentionGrid.TabIndex = 4;
			this.ContainerPenaltiesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerDetentionGrid)).EndInit();
			this.ContainerDetentionGrid.ResumeLayout(false);
			this.ContainerDetentionGrid.PerformLayout();
			this.ContainerPenaltiesTabPage.ResumeLayout(true);
		}

		void CTOStoragesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new ZCodeFindBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo4 = new ZOrganisationFindBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo6 = new ZOrganisationFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZDropEditColumnStyleInfo();
			FreeDayExclusionColumnStyleInfo freeDayExclusionColumnStyleInfo = new FreeDayExclusionColumnStyleInfo();
			DurationExclusionColumnStyleInfo durationExclusionColumnStyleInfo = new DurationExclusionColumnStyleInfo();
			this.CTOStoragesGrid = new ZGrid();
			this.CTOStoragesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CTOStoragesGrid)).BeginInit();
			this.CTOStoragesGrid.SuspendLayout();
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("17714a3a-9746-4ffc-b2e5-6db12598a1fa", "Class");
			zDropEditColumnStyleInfo9.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.CTOStoragesTabPage.Controls.Add(this.CTOStoragesGrid);
			// 
			// CTOStoragesGrid
			// 
			this.CTOStoragesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CTOStoragesGrid, "ConsigneeCTOStorages");
			this.CTOStoragesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c00f6979-3bb2-4b85-b899-9b57c4b764bb", "Load Port/Country(Region)");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "PD_OriginPortOrCountry";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ac237e85-9b7d-44b2-906f-3e1f14b3cce7", "Port/Country(Region)", "Storage Port/Country(Region)", "");
			zCodeFindBoxColumnStyleInfo6.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo4.ColumnName = "PD_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("14f16ba2-c36d-4806-9520-9a498923f29c", "CTO");
			zOrganisationFindBoxColumnStyleInfo6.ColumnName = "PD_OH_CTO";
			zOrganisationFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0a0d23c4-ac2b-4d28-822b-a55cf6085262", "Free Days");
			zCalcEditColumnStyleInfo2.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5d5c7aa2-9fc4-498a-ba6d-9bba2b2c95a7", "1st Free Day");
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			freeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			freeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			durationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			durationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CTOStoragesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.CTOStoragesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.CTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.CTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			this.CTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo6);
			this.CTOStoragesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.CTOStoragesGrid.ColumnStyles.Add(freeDayExclusionColumnStyleInfo);
			this.CTOStoragesGrid.ColumnStyles.Add(durationExclusionColumnStyleInfo);

			this.CTOStoragesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CTOStoragesGrid.GridId = "801d3c9c-e6af-4bba-9611-b825a3058b9d";
			this.CTOStoragesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CTOStoragesGrid.LayoutKey = "zGrid1";
			this.CTOStoragesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CTOStoragesGrid.Name = "CTOStoragesGrid";
			this.CTOStoragesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 206, true);
			this.CTOStoragesGrid.TabIndex = 5;
			this.CTOStoragesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CTOStoragesGrid)).EndInit();
			this.CTOStoragesGrid.ResumeLayout(false);
			this.CTOStoragesGrid.PerformLayout();
			this.CTOStoragesTabPage.ResumeLayout(true);
		}

		void ContainerFillingRatioTolerancesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ContainerMinFillingRatioTolerancesCalcEdit = new ZCalcEdit();
			this.ContainerMaxFillingRatioTolerancesCalcEdit = new ZCalcEdit();
			this.ContainerFillingRatioTolerancesTabPage.SuspendLayout();
			this.ContainerFillingRatioTolerancesTabPage.Controls.Add(this.ContainerMinFillingRatioTolerancesCalcEdit);
			this.ContainerFillingRatioTolerancesTabPage.Controls.Add(this.ContainerMaxFillingRatioTolerancesCalcEdit);
			// 
			// ContainerMinFillingRatioTolerancesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerMinFillingRatioTolerancesCalcEdit, "MiscServ.OM_ORDMinimumContainerFillingPercentage");
			this.ContainerMinFillingRatioTolerancesCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|ACABE9BE-6067-47F4-A5AF-D990F49157BB", "Minimum container filling ratio acceptable (%)");
			this.ContainerMinFillingRatioTolerancesCalcEdit.DecimalPlaces = 2;
			this.ContainerMinFillingRatioTolerancesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 20, true);
			this.ContainerMinFillingRatioTolerancesCalcEdit.Name = "ContainerMinFillingRatioTolerancesCalcEdit";
			this.ContainerMinFillingRatioTolerancesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 15, true);
			this.ContainerMinFillingRatioTolerancesCalcEdit.TabIndex = 1;
			this.ContainerMinFillingRatioTolerancesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerMinFillingRatioTolerancesCalcEdit.MaxValue = 100m;
			this.ContainerMinFillingRatioTolerancesCalcEdit.AllowNegative = false;
			// 
			// ContainerMaxFillingRatioTolerancesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerMaxFillingRatioTolerancesCalcEdit, "MiscServ.OM_ORDMaximumContainerFillingPercentage");
			this.ContainerMaxFillingRatioTolerancesCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|8116CBDE-B5BC-427F-9C68-8E4D8636BB7A", "Maximum container filling ratio acceptable (%)");
			this.ContainerMaxFillingRatioTolerancesCalcEdit.DecimalPlaces = 2;
			this.ContainerMaxFillingRatioTolerancesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 46, true);
			this.ContainerMaxFillingRatioTolerancesCalcEdit.Name = "ContainerMaxFillingRatioTolerancesCalcEdit";
			this.ContainerMaxFillingRatioTolerancesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 15, true);
			this.ContainerMaxFillingRatioTolerancesCalcEdit.TabIndex = 3;
			this.ContainerMaxFillingRatioTolerancesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerMaxFillingRatioTolerancesCalcEdit.MaxValue = 100m;
			this.ContainerMaxFillingRatioTolerancesCalcEdit.AllowNegative = false;
			this.ContainerFillingRatioTolerancesTabPage.PerformLayout();
			this.ContainerFillingRatioTolerancesTabPage.ResumeLayout(true);
		}

		#endregion
	}
}
