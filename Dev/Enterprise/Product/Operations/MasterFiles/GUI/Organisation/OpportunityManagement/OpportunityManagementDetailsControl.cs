using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityManagementDetailsControl : ZUserControl
	{
		public OpportunityManagementDetailsControl()
		{
			InitializeComponent();

			this.relatedCommunicationGrid.InnerGrid.ColorContextKey = RelatedCommunicationForm.RelatedCommunicationColorContextKey;
			DetailsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			OpportunityClientGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			OpportunityTabControl.PlugIns.AddPlugInAtTabPageIndex(GetPluginControllerID(), 1);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupContactPhoneDialler();
				SetupCommissionAgreementTab();
				SetupValueEstimationControl();
				SetupSalesRelationControl();
			}

			OpportunityCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("D40CD01A-5F8D-43E5-9C9C-6F16CC00EA8D", "To make use of this tab, please setup sales opportunity custom fields in Workflow Manager.");
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var dataItem = CurrentDataItem;
			if (dataItem != null)
			{
				dataItem.P8_SourceInfo.ValueChanged -= P8_Source_ValueChanged;
				dataItem.P8_StatusInfo.ValueChanged -= P8_StatusInfo_ValueChanged;
				dataItem.P8_OHInfo.ValueChanged -= P8_OH_P8_OC_ValueChanged;
				dataItem.P8_OCInfo.ValueChanged -= P8_OH_P8_OC_ValueChanged;

				foreach (var prop in CurrencyPropertyInfos)
				{
					prop.ValueChanged -= CurrencyValueChanged;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var dataItem = CurrentDataItem;
			if (dataItem != null)
			{
				SetupSourceDetailsControl();
				UpdateDeliveryStatusControls();

				dataItem.P8_SourceInfo.ValueChanged += P8_Source_ValueChanged;
				dataItem.P8_StatusInfo.ValueChanged += P8_StatusInfo_ValueChanged;
				dataItem.P8_OHInfo.ValueChanged += P8_OH_P8_OC_ValueChanged;
				dataItem.P8_OCInfo.ValueChanged += P8_OH_P8_OC_ValueChanged;

				foreach (var prop in CurrencyPropertyInfos)
				{
					prop.ValueChanged += CurrencyValueChanged;
				}
			}
		}

		void P8_OH_P8_OC_ValueChanged(object sender, EventArgs e)
		{
			UpdateDeliveryStatusControls();
		}

		void UpdateDeliveryStatusControls()
		{
			var orgContact = CurrentDataItem?.Contact;
			var emailAddress = orgContact?.EmailAddress;

			if (emailAddress != null && emailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
			{
				DeliveryStatusLabel.Text = Res.GetString("5E734144-3B6C-448E-8151-9A17C0A24335", "Non-Delivery Receipt: {0}", orgContact.DeliveryReportTimeUtc.ToLocalBranchTime().ToLongTimeString());
				DeliveryStatusLabel.ForeColor = ZArchitecture.GUI.Notifications.NotificationColorScheme.GetFontColor(NotificationType.Warning);

				if (!DeliveryStatusLabel.Visible)
				{
					DeliveryStatusLabel.Visible = true;
					DeliveryStatusButton.Visible = true;
					DeliveryStatusButton.NormalBackgroundImage = WarningImageRest;
					DeliveryStatusButton.HotBackgroundImage = WarningImageHot;

					AdjustControlsDeliveryStatus();
				}
			}
			else
			{
				if (DeliveryStatusLabel.Visible)
				{
					DeliveryStatusLabel.Text = string.Empty;
					DeliveryStatusLabel.Visible = false;
					DeliveryStatusButton.Visible = false;

					AdjustControlsNonDeliveryStatus();
				}
			}
		}

		void AdjustControlsDeliveryStatus()
		{
			ContactDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(383, 150);
			OpportunityClientGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(5, 238);
			LeadSourceGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(395, 238);
			OpportunityTabControl.Location = ControlDpiScalingHelper.NewScaledPoint(8, 383);
			ControlDpiScalingHelper.SetHeight(OpportunityTabControl, OpportunityTabControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
		}

		void AdjustControlsNonDeliveryStatus()
		{
			ContactDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(383, 127);
			OpportunityClientGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(5, 215);
			LeadSourceGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(395, 215);
			OpportunityTabControl.Location = ControlDpiScalingHelper.NewScaledPoint(8, 363);
			ControlDpiScalingHelper.SetHeight(OpportunityTabControl, OpportunityTabControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(20), false);
		}

		Bitmap WarningImageRest => warningImageRest ?? (warningImageRest = Icons.GetIcon(IconTypes.Warning).ToBitmap());
		Bitmap warningImageRest;

		Bitmap WarningImageHot
		{
			get
			{
				if (warningImageHot == null)
				{
					warningImageHot = new Bitmap(WarningImageRest.Width, WarningImageRest.Height);
					using (var g = Graphics.FromImage(warningImageHot))
					{
						var dst = ControlDpiScalingHelper.NewScaledRectangle(0, 0, WarningImageRest.Width, WarningImageRest.Height - 1, false);
						var src = ControlDpiScalingHelper.NewScaledRectangle(0, 1, WarningImageRest.Width, WarningImageRest.Height - 1, false);
						g.DrawImage(WarningImageRest, dst, src, GraphicsUnit.Pixel);
					}
				}
				return warningImageHot;
			}
		}
		Bitmap warningImageHot;

		public ZPlugIn TradeProfilePlugin
		{
			get { return OpportunityTabControl.PlugIns.GetPlugIn(ControllerIDs.TradeProfileForRelatedPlugin); }
		}

		public new OrgOpportunity CurrentDataItem
		{
			get { return (OrgOpportunity)base.CurrentDataItem; }
		}

		new OrgOpportunity DataSource
		{
			get { return base.DataSource as OrgOpportunity; }
		}

		ZForm ParentZForm
		{
			get { return ParentForm as ZForm; }
		}

		#region Overall Disposition Details

		void P8_StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			CurrentDataItem.P8_StatusInfo.ValueChanged -= P8_StatusInfo_ValueChanged;

			if (PromptUserToSetTradeDetailStatus(e) == DialogResult.OK)
			{
				PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(e);
				SetOverallDispositionLabelColor();
				TradeProfilePlugin?.RefreshData();
			}

			CurrentDataItem.P8_StatusInfo.ValueChanged += P8_StatusInfo_ValueChanged;
		}

		DialogResult PromptUserToSetTradeDetailStatus(EventArgs e)
		{
			if (BusinessEntity != null && (BusinessEntity.P8_StatusInfo.HasChanges || (!BusinessEntity.IsInDatabase && !BusinessEntity.P8_Status.IsEmpty)))
			{
				return (DialogResult)ObjectFactory.Get<ITradeDetailCommitmentUpdaterGUIManager>().ShowForm(BusinessEntity, e);
			}

			return DialogResult.OK;
		}

		void SetOverallDispositionLabelColor()
		{
			if (!BusinessEntity.IsClosed)
			{
				OverallDispositionLabel.BackColor = System.Drawing.Color.LimeGreen;
			}
			else if (BusinessEntity.IsClosed)
			{
				OverallDispositionLabel.BackColor = System.Drawing.Color.Red;
			}
		}

		void PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(EventArgs e)
		{
			if (isRevertingStatusChange || !(e is ValueChangedEventArgs valueChangedEventArgs))
			{
				return;
			}

			var previousStatus = (ZString)valueChangedEventArgs.OldValue;
			var newStatus = (ZString)valueChangedEventArgs.NewValue;
			var opportunity = (OrgOpportunity)valueChangedEventArgs.Info.BizObj;

			if (!opportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(newStatus, previousStatus))
			{
				return;
			}

			if (!opportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(newStatus))
			{
				isRevertingStatusChange = true;
				try
				{
					valueChangedEventArgs.Info.Value = previousStatus;
				}
				finally
				{
					isRevertingStatusChange = false;
				}
			}
		}

		bool isRevertingStatusChange;

		#endregion

		#region EstimatedValueCurrency

		protected virtual IEnumerable<ZPropertyInfo> CurrencyPropertyInfos
		{
			get
			{
				return new[] { CurrentDataItem?.P8_RX_NKEstimatedValueCurrencyInfo }.Where(x => x != null);
			}
		}

		void CurrencyValueChanged(object sender, EventArgs e)
		{
			var opp = BusinessEntity;
			var valueChangedEventArgs = e as ValueChangedEventArgs;
			if (opp != null && valueChangedEventArgs != null && !valueChangedEventArgs.NewValue.IsEmpty && !UpdateOpportunityDateForExchangeRateFormSuspender.IsSuspended && !opp.Factory.IsInTransaction)
			{
				var updateAction = new UpdateP8_DateForExchangeRateAction(opp);
				var updateDateForExchangeRateForm = new UpdateOpportunityDateForExchangeRateForm(updateAction);
				updateDateForExchangeRateForm.FormClosed += GetUpdateDateForExchangeRateFormClosedEventHandler(valueChangedEventArgs);
				ZFormModaliser.Show(updateDateForExchangeRateForm, ParentForm);
			}
		}

		FormClosedEventHandler GetUpdateDateForExchangeRateFormClosedEventHandler(ValueChangedEventArgs valueChangedEventArgs)
		{
			return (sender, e) =>
			{
				var form = (UpdateOpportunityDateForExchangeRateForm)sender;
				var opp = BusinessEntity;
				if (opp != null && form.DialogResult == DialogResult.Cancel)
				{
					using (UpdateOpportunityDateForExchangeRateFormSuspender.GetSuspender())
					{
						valueChangedEventArgs.Info.Value = valueChangedEventArgs.OldValue;
					}
				}
			};
		}

		readonly FunctionalitySuspender UpdateOpportunityDateForExchangeRateFormSuspender = new FunctionalitySuspender();

		#endregion

		#region Source Details

		void P8_Source_ValueChanged(object sender, EventArgs e)
		{
			if (BusinessEntity.P8_Source != string.Empty && BusinessEntity.Lookups.ActiveSourceDetails.Count > 0)
			{
				SwitchSourceDetailsControl(true);
			}
			else
			{
				SwitchSourceDetailsControl(false);
			}
		}

		void SwitchSourceDetailsControl(bool dropDownEnabled)
		{
			SourceDetailsDropEdit.Visible = dropDownEnabled;
			SourceDetailsTextBox.Visible = !dropDownEnabled;
		}

		void SetupSourceDetailsControl()
		{
			if (BusinessEntity.Lookups.SourceDetails.Count > 0)
			{
				SwitchSourceDetailsControl(true);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (warningImageHot != null)
			{
				warningImageHot.Dispose();
				warningImageHot = null;
			}

			if (warningImageRest != null)
			{
				warningImageRest.Dispose();
				warningImageRest = null;
			}

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		protected OrgOpportunity BusinessEntity
		{
			get { return (OrgOpportunity)base.DataSource; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!this.IsDesignMode())
			{
				P8_PackageTypeDropEdit.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new Binding("Caption", BusinessEntity, "ExtraCategoryDescription", true));
				RentalMultiplierCalcEdit.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new Binding("Caption", BusinessEntity, "RentalMultiplierDescription", true));
				RentalMultiplierCalcEdit.GetExtension<HintExtension>().Description = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
				DiscountCalcEdit.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new Binding("Caption", BusinessEntity, "TotalDiscountDescription", true));
				DiscountCalcEdit.GetExtension<HintExtension>().Description = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
				SetOverallDispositionLabelColor();

#if DEBUG
				TypeDescriptor.AddAttributes(P8_PackageTypeDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
				CalculateCloseCertaintyTrackbarValue();
				DisableCloseCertaintyTrackbarInViewMode();
			}
		}

		#endregion

		#region Close Certainty

		const byte closeCertaintyIncrement = 5;
		protected void CloseCertaintyTrackBar_Scroll(object sender, EventArgs e)
		{
			if (BusinessEntity.P8_CloseCertainty < 0 || BusinessEntity.P8_CloseCertainty > 100)
			{
				CloseCertaintyPercentageLabel.BackColor = System.Drawing.Color.Transparent;
			}

			BusinessEntity.P8_CloseCertainty = (ZByte)(CloseCertaintyTrackBar.Value * closeCertaintyIncrement);
			CloseCertaintyPercentageLabel.Text = BusinessEntity.CloseCertaintyAsPercentageString;
		}

		void DisableCloseCertaintyTrackbarInViewMode()
		{
			if (ParentZForm != null && ParentZForm.DisplayMode == ZArchitecture.Core.ODisplayMode.ReadOnly)
			{
				CloseCertaintyTrackBar.SetReadOnly(true);
			}
		}

		void CalculateCloseCertaintyTrackbarValue()
		{
			if (BusinessEntity.P8_CloseCertainty >= 0 && BusinessEntity.P8_CloseCertainty <= 100)
			{
				CloseCertaintyTrackBar.Value = BusinessEntity.P8_CloseCertainty / closeCertaintyIncrement;
			}
			else
			{
				CloseCertaintyTrackBar.Value = 0;
				CloseCertaintyPercentageLabel.BackColor = System.Drawing.Color.FromArgb(255, 215, 215);
			}
		}
		#endregion

		#region Created From Inquiry

		protected void CreatedFromInquiryLabel_Click(object sender, EventArgs e)
		{
			SalesEnquiry inquiry = BusinessEntity.CreatedFromInquiry;
			if (inquiry != null)
			{
				if (Env.Security.InquiryManagerEdit.IsAllowed)
				{
					InquiryController.ShowEditForm(inquiry);
				}
				else if (Env.Security.InquiryManagerView.IsAllowed)
				{
					InquiryController.ShowViewForm(inquiry);
				}
				else
				{
					Env.Security.InquiryManagerView.ShowError();
				}
			}
		}

		protected ZController InquiryController
		{
			get { return inquiryController ?? (inquiryController = ZControllerFactory.Create(ControllerIDs.SalesEnquiry)); }
		}
		ZController inquiryController;

		#endregion

		#region ContactPhoneDialler

		void SetupContactPhoneDialler()
		{
			ContactPhoneDiallerUserControl.Dialling += ContactPhoneDialler_Dialling;
		}

		void ContactPhoneDialler_Dialling(object sender, PhoneDiallingEventArgs e)
		{
			e.CreateRelatedCommunication = OrganisationsDataRegistry.Instance.CreateCommunicationOnOpportunityContactCall.Value;
		}

		#endregion

		#region Commission Agreement Tab

		void SetupCommissionAgreementTab()
		{
			CommissionAgreementTabPage.SetupSecurity(Env.Security.CommissionAgreementView, Env.Licence.CommissionManager);
		}

		#endregion

		#region Plugins / Extensions

		void SetupValueEstimationControl()
		{
			var valueEstimationCtrl = CreateValueEstimationControl();
			if (valueEstimationCtrl != null)
			{
				EstimatedValueCalcFindBox.Visible = false;
				DiscountCalcEdit.Visible = false;
				RentalMultiplierCalcEdit.Visible = false;
				MonetaryValueGroupBox.Visible = false;

				BindingSource.SetBindingMember(DiscountCalcEdit, "");
				BindingSource.SetBindingMember(EstimatedValueCalcFindBox, "");
				BindingSource.SetBindingMember(RentalMultiplierCalcEdit, "");
				EstimatedValueCalcFindBox.BindToAmount = "";
				EstimatedValueCalcFindBox.BindToUnit = "";

				valueEstimationCtrl.SetDataBinding(DataSource, "");
				valueEstimationCtrl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 95, true);
				valueEstimationCtrl.TabIndex = 4;
				valueEstimationCtrl.Name = "ValueEstimationControl";

				DetailsGroupBox.Controls.Add(valueEstimationCtrl);
				DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 208, true);
			}
		}

		protected virtual ControllerID GetPluginControllerID()
		{
			return ControllerIDs.TradeProfileForRelatedPlugin;
		}

		protected virtual ZUserControl CreateValueEstimationControl()
		{
			return null;
		}

		void SetupSalesRelationControl()
		{
			var salesRelationControl = CreateSalesRelationControl();
			if (salesRelationControl != null)
			{
				salesRelationControl.AllowDrop = true;
				BindingSource.SetBindingMember(salesRelationControl, nameof(OrgOpportunity.SalesRelationModel));

				salesRelationControl.Dock = DockStyle.Fill;
				salesRelationControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
				salesRelationControl.Margin = ControlDpiScalingHelper.NewScaledPadding(3, true);
				salesRelationControl.Name = "salesRelationControl";
				salesRelationControl.NameOfATreeElement = Res.GetData("e0bdcf6a-9125-4f4d-b8a1-f1a0ad65448c", "Relatable Activity");
				salesRelationControl.NameOfTreeElementsPlural = Res.GetData("d5f8070a-7663-4373-9277-ccb79a2c0f65", "Relatable Activities");
				salesRelationControl.ShowPopupButton = false;
				salesRelationControl.Size = ControlDpiScalingHelper.NewScaledSize(765, 215, true);
				salesRelationControl.TabIndex = 0;

				SalesRelationsTabPage.Controls.Add(salesRelationControl);
			}
		}

		protected virtual SalesRelationControl CreateSalesRelationControl()
		{
			return new SalesRelationControl();
		}

		#endregion
	}
}
