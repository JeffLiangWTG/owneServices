using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PhoneNumberUserControl : ZUserControl
	{
		public PhoneNumberUserControl()
		{
			InitializeComponent();
		}

		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (UseNumberTypeCaption)
			{
				NumberTextBox.CaptionResourceString = CaptionResourceString;
			}

			SetLocationsForControlsThatCanToggle();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				NumberTextBox.PhoneNumberProperty = CurrentDataItem.FormattedInfo;
				SetPhoneNumberTooltipPropertyIfNeeded();

				if (ValidationCanBeCalled(CurrentDataItem))
				{
					((IBusinessObjectInternals)CurrentDataItem).Validate(nameof(CurrentDataItem.FormattedForBinding));
				}
				if (Enabled)
				{
					NumberTextBox.ColorChanger.SetValidColorIfNeeded();
					CurrentDataItem.FormattedForBindingInfo.ValueChanged += FormattedForBindingInfo_ValueChanged;
				}

				CurrentDataItem.FormattedForBindingInfo.RefreshBinding();
			}
			else
			{
				NumberTextBox.ResetText();
				NumberTextBox.ColorChanger.ClearValidColorIfNeeded();
			}
		}

		bool ValidationCanBeCalled(PhoneNumber currentDataItem)
		{
			var hash = currentDataItem.ZPropertyInfoHash;
			if (hash.ContainsKey(currentDataItem.FormattedForBinding))
			{
				var bizoRow = (hash[currentDataItem.FormattedForBinding].BizObj as INeedRow).Row;
				return (bizoRow.RowState != System.Data.DataRowState.Deleted && bizoRow.RowState != System.Data.DataRowState.Detached);
			}
			return false;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FormattedForBindingInfo.ValueChanged -= FormattedForBindingInfo_ValueChanged;
			}
		}

		void FormattedForBindingInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshNumberTextBoxValidationColor();
		}

		void RefreshNumberTextBoxValidationColor()
		{
			var zForm = ParentForm as ZForm;
			if (zForm != null && zForm.DisplayMode != ODisplayMode.ReadOnly && CurrentDataItem != null && !CurrentDataItem.FormattedForBindingInfo.ReadOnly && Enabled)
			{
				var invalidPhoneNumberFormatNotificationMessage = PhoneNumberFormatterAndValidator.GetInvalidPhoneNumberFormatNotificationMessage(CurrentDataItem.FormattedInfo.Notifications);
				var isTextValid = string.IsNullOrEmpty(invalidPhoneNumberFormatNotificationMessage) && !CurrentDataItem.FormattedForBinding.IsEmpty;
				if (isTextValid)
				{
					NumberTextBox.ColorChanger.SetValidColorIfNeeded();
				}
				else
				{
					NumberTextBox.ColorChanger.ClearValidColorIfNeeded();
				}
			}
		}

		[DefaultValue(32767)]
		public int NumberTextBoxMaxLength
		{
			set
			{
				NumberTextBox.MaxLength = value;
			}
			get
			{
				return NumberTextBox.MaxLength;
			}
		}

		void NumberTextBox_Enter(object sender, EventArgs e)
		{
			if (CurrentDataItem != null && CurrentDataItem.IsManuallyVerifiedInfo != null)
			{
				var invalidPhoneNumberFormatNotificationMessage = PhoneNumberFormatterAndValidator.GetInvalidPhoneNumberFormatNotificationMessage(CurrentDataItem.FormattedInfo.Notifications);
				if (!string.IsNullOrEmpty(invalidPhoneNumberFormatNotificationMessage))
				{
					var scaledValidationOverrideControlHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(212);
					var locationOnForm = ParentForm.PointToClient(NumberTextBox.Parent.PointToScreen(NumberTextBox.Location));
					var location = PointToScreen(ControlDpiScalingHelper.NewScaledPoint(
						NumberTextBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(2),
						locationOnForm.Y + scaledValidationOverrideControlHeight > ParentForm.ClientSize.Height ? NumberTextBox.Bottom - scaledValidationOverrideControlHeight : NumberTextBox.Top,
						false));

					PhoneNumberValidationOverrideControlHelper.ShowConfirmationMessage(CurrentDataItem.IsManuallyVerifiedInfo, this, location, ParentForm.Controls, RefreshNumberTextBoxValidationColor, invalidPhoneNumberFormatNotificationMessage, CurrentDataItem.FormattedForBinding);
				}
			}
		}

		void NumberTextBox_Leave(object sender, EventArgs e)
		{
			if (CurrentDataItem != null && CurrentDataItem.IsManuallyVerifiedInfo != null && ParentForm != null && !IsConfirmationFormFocused(ParentForm))
			{
				PhoneNumberValidationOverrideControlHelper.CloseConfirmationForm(ParentForm.Controls);
			}

			if (IsMobileNumberTextBox())
			{
				NumberTextBox.Modified = !NumberTextBox.Modified;
			}
		}

		void NumberTextBox_Validated(object sender, EventArgs e)
		{
			if (IsMobileNumberTextBox() && !NumberTextBox.Modified)
			{
				var cDataSource = (DataSource as PhoneContactItem)?.Contact;
				var pDataSource = (DataSource as GlbPerson);
				var aDataSource = (DataSource as HRJobApplicant);
				var sDataSource = (DataSource as GlbStaff);

				var person = cDataSource?.Person ?? pDataSource ?? aDataSource?.Person ?? sDataSource?.Person;
				var mobileInfo = cDataSource?.OC_MobileInfo ?? pDataSource?.PER_MobilePhoneInfo ?? aDataSource?.HA_MobilePhoneInfo ?? sDataSource?.GS_MobilePhoneInfo;
				if (person != null && mobileInfo != null)
				{
					CheckRelatedMobiles(mobileInfo, person);
				}
			}
		}

		void NumberTextBox_TextChanged(object sender, EventArgs e)
		{
			if (ParentForm != null && CurrentDataItem != null && CurrentDataItem.IsManuallyVerifiedInfo != null)
			{
				PhoneNumberValidationOverrideControlHelper.CloseConfirmationForm(ParentForm.Controls);
			}
		}

		static bool IsConfirmationFormFocused(Control control)
		{
			return PhoneNumberValidationOverrideControlHelper.IsConfirmationFormFocused(control.Controls);
		}

		bool IsMobileNumberTextBox()
		{
			var phoneContactItem = DataSource as PhoneContactItem;
			return DataMember.Contains((NoResString)"Mobile") || (phoneContactItem != null && phoneContactItem.OI_Description == PhoneContactItemDescriptionList.Codes.Mobile);
		}

		public void CheckRelatedMobiles(ZPropertyInfo mobileInfo, GlbPerson person)
		{
			var originalMobile = mobileInfo.OriginalValue.ToString();
			var newMobile = mobileInfo.Value.ToString();
			if (person != null && originalMobile == person.PER_MobilePhoneInternal || newMobile == person.PER_MobilePhoneInternal)
			{
				if (mobileInfo.HasChanges && person.MobileExistsOnMultipleRelatedRecords(originalMobile, newMobile))
				{
					var dialogMessage = Res.GetString("F1916099-521C-44D6-B0E3-186ED639EFC7", "The superseded mobile number exists on multiple records associated to this Person. Would you like to update all of these records?");
					DialogResult result = Globals.Message.Show(dialogMessage, Res.GetString("1AD21404-8280-4B53-B096-68D805224E80", "Update Related Numbers"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					if (result == DialogResult.Yes)
					{
						person.ShouldUpdateMobileOnRelatedRecords = true;
					}
					else
					{
						person.ShouldUpdateMobileOnRelatedRecords = false;
					}
					NumberTextBox.Modified = false;
				}
			}
		}

		public new PhoneNumber CurrentDataItem
		{
			get { return (PhoneNumber)base.CurrentDataItem; }
		}

		#endregion

		#region Methods

		public void SetReadOnly(bool value)
		{
			NumberTextBox.ReadOnly = value;
			PublishCheckEdit.ReadOnly = value;
		}

		void LocalNumberLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var number = CurrentDataItem.FormattedForBinding;
			if (!number.IsEmpty)
			{
				var defaultDialingProtocol = SystemDataRegistry.Instance.PhoneDialingUriProtocols.Value.Default;
				PhoneDialler.Dial(number, defaultDialingProtocol.Code);
			}
		}

		protected PhoneDialler PhoneDialler
		{
			get { return phoneDialler ?? (phoneDialler = GetNewPhoneDialler()); }
		}
		PhoneDialler phoneDialler;

		protected virtual PhoneDialler GetNewPhoneDialler()
		{
			return new PhoneDialler();
		}

		public void SetDialling(EventHandler<PhoneDiallerUserControl.DiallingEventArgs> diallingEvent)
		{
			PhoneDiallerControl.Dialling += diallingEvent;
		}

		public void SetDiallingRelatedCommunicationContactDeciding(EventHandler<PhoneDiallerUserControl.DiallingRelatedCommunicationContactDecidingArgs> diallingEvent)
		{
			PhoneDiallerControl.DiallingRelatedCommunicationContactDeciding += diallingEvent;
		}

		#endregion

		#region Properties

		[DefaultValue(true)]
		public bool ShowDiallerControl
		{
			get { return showDiallerControl; }
			set
			{
				if (showDiallerControl != value)
				{
					showDiallerControl = value;
					PhoneDiallerControl.Visible = ShowDiallerControl;
				}
			}
		}
		bool showDiallerControl = true;

		[DefaultValue(false)]
		public bool EnableValidStateColor
		{
			get { return NumberTextBox.EnableValidStateColor; }
			set { NumberTextBox.EnableValidStateColor = value; }
		}

		[DefaultValue(true)]
		public bool ShowLocalNumberLabel
		{
			get { return showLocalNumberLabel; }
			set
			{
				if (showLocalNumberLabel != value)
				{
					showLocalNumberLabel = value;
					LocalNumberLinkLabel.Visible = ShowLocalNumberLabel;
				}
			}
		}
		bool showLocalNumberLabel = true;

		[DefaultValue(true)]
		public bool ShowPublishedCheckBox
		{
			get { return showPublishedCheckBox; }
			set
			{
				if (showPublishedCheckBox != value)
				{
					showPublishedCheckBox = value;
					PublishCheckEdit.Visible = ShowPublishedCheckBox;
				}
			}
		}
		bool showPublishedCheckBox = true;

		[DefaultValue(false)]
		public bool ShowToolTip
		{
			get { return showToolTip; }
			set
			{
				showToolTip = value;
				SetPhoneNumberTooltipPropertyIfNeeded();
			}
		}
		bool showToolTip;

		void SetPhoneNumberTooltipPropertyIfNeeded()
		{
			if (CurrentDataItem != null && ShowToolTip)
			{
				NumberTextBox.PhoneNumberTooltipProperty = CurrentDataItem.FormattedLocalNumberIfLoggedInSameCountryInfo;
			}
		}

		[DpiState(DpiState.Unscaled)]
		[DefaultValue(0)]
		public int UnscaledLeftPadding { get; set; } = 0;

		[DefaultValue(true)]
		public bool UseNumberTypeCaption
		{
			get { return useNumberTypeCaption; }
			set
			{
				if (useNumberTypeCaption != value)
				{
					useNumberTypeCaption = value;
					NumberTextBox.GetExtension<ILabelCaptionRenderer>().Visible = UseNumberTypeCaption;
				}
			}
		}
		bool useNumberTypeCaption = true;

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<PhoneNumberUserControl>()
				.Property("ShowDiallerControl", false, false)
				.Property("EnableValidStateColor", false, false)
				.Property("ShowLocalNumberLabel", false, false)
				.Property("ShowPublishedCheckBox", false, false)
				.Property("ShowToolTip", false, false)
				.Property("UnscaledLeftPadding", 0, false)
				.Property("UseNumberTypeCaption", false, false)
				.Result;
		}

		#endregion

		#region Synchronise Child Controls Appearance

		void SetLocationsForControlsThatCanToggle()
		{
			SuspendLayout();
			try
			{
				int paddingBetweenControls = ControlDpiScalingHelper.ScaleToCurrentDpiX(6);

				if (!ShowDiallerControl && !ShowLocalNumberLabel)
				{
					ControlDpiScalingHelper.SetLeft(ref PublishCheckEdit, PhoneDiallerControl.Left, false);
				}
				else if (!ShowDiallerControl)
				{
					var left = LocalNumberLinkLabel.Left - (PhoneDiallerControl.Width + paddingBetweenControls);
					ControlDpiScalingHelper.SetLeft(ref LocalNumberLinkLabel, left, false);
				}
				else if (!ShowLocalNumberLabel)
				{
					var left = PublishCheckEdit.Left - (LocalNumberLinkLabel.Width + paddingBetweenControls);
					ControlDpiScalingHelper.SetLeft(ref PublishCheckEdit, left, false);
				}

				if (UnscaledLeftPadding != 0)
				{
					var scaledLeftPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(UnscaledLeftPadding);
					var left = NumberTextBox.Left + scaledLeftPadding;
					ControlDpiScalingHelper.SetLeft(ref NumberTextBox, left, false);
					left = PhoneDiallerControl.Left + scaledLeftPadding;
					ControlDpiScalingHelper.SetLeft(ref PhoneDiallerControl, left, false);
					left = LocalNumberLinkLabel.Left + scaledLeftPadding;
					ControlDpiScalingHelper.SetLeft(ref LocalNumberLinkLabel, left, false);
					left = PublishCheckEdit.Left + scaledLeftPadding;
					ControlDpiScalingHelper.SetLeft(ref PublishCheckEdit, left, false);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region class PhoneNumberUserControl + PhoneNumberTextBox

		internal class PhoneNumberTextBox : ZTextBox
		{
			#region Properties

			public ZPropertyInfo PhoneNumberProperty { get; set; }

			public ZPropertyInfo PhoneNumberTooltipProperty { get; set; }

			#endregion

			#region Implementations

			protected override void OnMouseEnter(EventArgs e)
			{
				base.OnMouseEnter(e);
				if (PhoneNumberProperty != null && PhoneNumberTooltipProperty != null)
				{
					shouldShowBalloonTip = true;
					ShowBalloonTip();
				}
			}

			protected override void OnTextChanged(EventArgs e)
			{
				base.OnTextChanged(e);
				if (PhoneNumberProperty != null && PhoneNumberTooltipProperty != null)
				{
					ShowBalloonTip();
				}
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				base.OnMouseLeave(e);
				if (PhoneNumberProperty != null && PhoneNumberTooltipProperty != null)
				{
					shouldShowBalloonTip = false;
					HideBalloonTip();
				}
			}

			void ShowBalloonTip()
			{
				if (PhoneNumberProperty != null && PhoneNumberTooltipProperty != null)
				{
					var phoneNumber = Text;
					Balloon.Instance.Hide();

					if (shouldShowBalloonTip && !PhoneNumberProperty.Notifications.Any() && !string.IsNullOrEmpty(phoneNumber) && phoneNumber == (ZString)PhoneNumberProperty.Value)
					{
						BalloonDescriptor descriptor = new BalloonDescriptor(this, (ZString)PhoneNumberTooltipProperty.Value, string.Empty, Array.Empty<INotification>())
						{
							HideWhenMouseOverBalloon = true
						};
						Balloon.Instance.Show(descriptor);
					}
				}
			}

			void HideBalloonTip()
			{
				if (!shouldShowBalloonTip)
				{
					Balloon.Instance.Hide();
				}
			}

			bool shouldShowBalloonTip;

			#endregion
		}

		#endregion
	}
}
