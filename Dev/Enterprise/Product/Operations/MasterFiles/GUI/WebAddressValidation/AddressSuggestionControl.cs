using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public partial class AddressSuggestionControl : ZUserControl, ISuggestionControl
	{
		public AddressSuggestionControl()
		{
			InitializeComponent();
		}

		public AddressSuggestionControl(bool isFixed) : this()
		{
			ValidateAddressButton.Enabled = IsFixedControl = isFixed;
			SetupListViewProperties();
		}

		#region Report Address
		void ReportToAddressButton_Click(object sender, EventArgs e)
		{
			ReportAddress();
		}

		void ReportAddress()
		{
			((IServiceRequestController)ControllerForReportToWTG).SetParentForm(this.ParentForm);
			ControllerForReportToWTG.ShowNewForm();
		}

		ZController ControllerForReportToWTG
		{
			get
			{
				if (controllerForReportToWTG == null)
				{
					controllerForReportToWTG = ZControllerFactory.Create(ControllerIDs.ServiceRequest);
				}
				return controllerForReportToWTG;
			}
		}

		ZController controllerForReportToWTG;

		#endregion

		public bool ShouldWarnOnManuallyVerify { get; set; } = true;

		public void ClearResultsList()
		{
			ValidateAddressButton.Enabled = false;
			AddressesListView.Items.Clear();
			InfoLabel.Visible = false;
		}

		public AddressSuggestionControl(
			ISupportWebAddressValidation address,
			List<ValidationResultItem> addressesItems,
			ValidationResultItem topRecommendedAddress,
			Form applicationForm,
			Control parentControl,
			Control referenceControl,
			int maxWidth = 0,
			int maxHeight = 0)
		{
			Address = address;
			MaxWidth = maxWidth;
			InitializeComponent();

			if (addressesItems != null && addressesItems.Count == 0)
			{
				ErrorReporter.ReportOnce(GetType().FullName + "ManuallyVerify", string.Format(CultureInfo.InvariantCulture, "AddressesListView is empty. Address being verified is:\r\n{0} {1} {2} {3} {4} {5}", Address.Address1, Address.Address2, Address.City, Address.Postcode, Address.State, Address.Country));
			}

			if (addressesItems != null && addressesItems.Count > 0 || topRecommendedAddress != null)
			{
				SetupListView(addressesItems, topRecommendedAddress);
			}
			else
			{
				HideListView();
			}

			UpdateSizeAndLocation(applicationForm, parentControl, referenceControl, 0, maxHeight);

			Focus();

			AddressSelected += HandleAddressSelected;
			Disposed += (sender, e) => AddressSelected -= HandleAddressSelected;
		}

		public void UpdateSizeAndLocation(Form applicationForm, Control parentControl, Control referenceControl, int maxWidth = 0, int maxHeight = 0)
		{
			if (!IsFixedControl)
			{
				UpdateSize(maxWidth);
			}
			UpdateLocation(applicationForm, parentControl, referenceControl, maxHeight);
		}

		public void HideListView(ISupportWebAddressValidation currentAddress = null, string message = null)
		{
			ValidateAddressButton.Enabled = false;
			InfoLabel.Visible = true;

			InfoLabel.Text = message ?? ((currentAddress != null && currentAddress.ValidationStatus == (ZString)AddressValidationStatus.ManuallyVerified)
				? Res.GetString("1119B371-9106-47F4-AA59-BC6276AFF6EB", "No suggestions provided as this has been manually verified. Press the Validation Icon to call the service.")
				: Res.GetString("c4b034fb-9bd5-4df5-931e-ed5c452052ab", "No suggestions have been found for the address that you entered. Please confirm as original or amend the address to receive suggestions."));
		}

		#region Properties and Fields

		int MaxWidth;
		public ISupportWebAddressValidation Address { get; private set; }
		public event EventHandler<AddressSelectedEventArgs> AddressSelected;

		public bool HasTopRecommendedItem { get; private set; }

#if DEBUG
		internal ListView AddressListViewExposedForTesting
		{
			get
			{
				return AddressesListView;
			}
		}
#endif
		public bool IsFixedControl { get; set; }
		#endregion

		#region AddressSelectedEventArgs

		public class AddressSelectedEventArgs : EventArgs
		{
			public ValidationResultItem SelectedAddress { get; set; }
			public bool PassAsEntered { get; set; }
			public ISupportWebAddressValidation Address { get; set; }
		}

		#endregion

		#region Methods

		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RelocateButtons();
		}

		public void Close()
		{
			if (!IsFixedControl)
			{
				if (Parent != null)
				{
					Parent.Controls.Remove(this);
				}

				if (IsHandleCreated)
				{
					// Don't Dispose synchronously since we could be in the middle of Control.WmMouseDown
					BeginInvoke(new MethodInvoker(Dispose));
				}
				else
				{
					// Will almost certainly only get here during unit tests
					Dispose();
				}
			}
		}

		string ConstructAddressForDisplay(ValidationResultItem address)
		{
			var result = string.Empty;
			if (Address?.Country != null && Address.Country.IsStateMustNotBeEntered)
			{
				result = string.Join(", ",
					(new string[] { address.Address1, address.Address2, address.City, address.Country, address.Postcode })
					.Where(s => !string.IsNullOrEmpty(s)));
			}
			else
			{
				result = string.Join(", ",
					(new string[] { address.Address1, address.Address2, address.City, address.State, address.Country, address.Postcode })
					.Where(s => !string.IsNullOrEmpty(s)));
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		internal void UpdateSize(int maxWidth = 0)
		{
			if (maxWidth != 0)
			{
				MaxWidth = maxWidth;
			}

			var maxItemLength = ControlDpiScalingHelper.ScaleToCurrentDpiX(240);

			using (var graphics = CreateGraphics())
			{
				foreach (var item in AddressesListView.Items)
				{
					if (item is ListViewItem listViewItem)
					{
						var textLength = (int)graphics.MeasureString(listViewItem.Text, listViewItem.Font).Width;

						if (!string.IsNullOrEmpty(listViewItem.ImageKey))
						{
							textLength += AddressesListView.SmallImageList.ImageSize.Width;
						}

						maxItemLength = Math.Max(maxItemLength, textLength);
					}
				}
			}

			var widthOffset = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			var controlWidth = maxItemLength + widthOffset;

			if (MaxWidth != 0 && controlWidth > MaxWidth)
			{
				controlWidth = MaxWidth;
			}

			AddressesListView.Width = Width = controlWidth;
			var scrollBarSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			AddressesListView.Columns[0].Width = controlWidth - AddressesListView.Margin.Left - AddressesListView.Margin.Right - scrollBarSize;
			AddressesListView.Height = AddressesPanel.Height - ManualVerifyButton.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(6);
			InfoLabel.Height = AddressesListView.Height - InfoLabel.Location.Y;
			InfoLabel.Width = AddressesListView.Width - InfoLabel.Location.X - InfoLabel.Location.X;

			UpdateManualVerifyButton(controlWidth);
		}

		void UpdateManualVerifyButton(int controlWidth)
		{
			var unscaledControlWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(controlWidth);
			var unscaledValidateAddressButtonWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ValidateAddressButton.Width);
			var unscaledMargin = 8;
			var padding = ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			ManualVerifyButton.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(Math.Min(125, unscaledControlWidth - unscaledValidateAddressButtonWidth - unscaledMargin));
			using (var graphics = CreateGraphics())
			{
				var shortCaption = Res.GetData("8e5dbd09-9c28-4816-90a3-92cbfd9b6d38", "Accept as ...");
				var fullCaption = Res.GetData("8e5dbd09-9c28-4816-90a3-92cbfd9b6858", "Accept as Entered");
				var toolTipCaption = ResString.GetMultilingualString("8e5dbd09-9c28-4816-90a3-92cbfd9b6dd8", "Accept as Entered");
				ManualVerifyButton.CaptionResourceString = fullCaption;
				var textLength = graphics.MeasureString(ManualVerifyButton.CaptionResourceString.Caption, ManualVerifyButton.Font).Width;
				if (ManualVerifyButton.Image != null)
				{
					textLength += ManualVerifyButton.Image.Width + padding;
				}

				ManualVerifyButton.CaptionResourceString = textLength > ManualVerifyButton.Width ? shortCaption : fullCaption;
				ManualVerifyButton.ToolTipCaption = toolTipCaption;
				ManualVerifyButton.UpdateCaption();
			}
		}

		void UpdateLocation(Form applicationForm, Control parentControl, Control referenceControl, int maxHeight = 0)
		{
			this.RelocateButtons();
			this.AdjustLocation(applicationForm, parentControl, referenceControl, Orientation.Horizontal, maxHeight);
		}

		void RelocateButtons()
		{
			ManualVerifyButton.Location = ControlDpiScalingHelper.NewScaledPoint(Width - ManualVerifyButton.Width - ValidateAddressButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(6), AddressesPanel.Height - ManualVerifyButton.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
			ValidateAddressButton.Location = ControlDpiScalingHelper.NewScaledPoint(Width - ValidateAddressButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3), ManualVerifyButton.Location.Y, false);
		}

		public ZButton ManuallyVerifyButton => this.ManualVerifyButton;

		public void SetupListView(ISupportWebAddressValidation address, WebAddressValidationResult result)
		{
			Address = address;
			AddressesListView.Visible = true;
			AddressesListView.Clear();
			SetupListView(result.SuggestedResults, result.TopRecommendedAddress);
		}

		public void SetupListView(List<ValidationResultItem> suggestedAddresses, ValidationResultItem topRecommendedAddress)
		{
			InfoLabel.Visible = false;
			string topRecommendedAddressText = string.Empty;

			// Top recommended address
			if (topRecommendedAddress != null)
			{
				HasTopRecommendedItem = true;

				// Standard entry without unmatched prefix/suffix
				var addressWithNoPrefixOrSuffix = topRecommendedAddress.ShallowCopy();
				topRecommendedAddressText = ConstructAddressForDisplay(addressWithNoPrefixOrSuffix);
				var viewItemTopRecommended = new ListViewItem(topRecommendedAddressText) { Tag = addressWithNoPrefixOrSuffix, ImageKey = "TopPick3232" };
				AddressesListView.Items.Add(viewItemTopRecommended);

				// Add an entry for unmatched prefix/suffix
				if (!string.IsNullOrEmpty(topRecommendedAddress.UnmatchedApartmentPrefix))
				{
					var addressWithPrefix = topRecommendedAddress.ShallowCopy();
					addressWithPrefix.UnmatchedApartmentPrefix = topRecommendedAddress.UnmatchedApartmentPrefix;
					addressWithPrefix.Address1 = string.Format(CultureInfo.CurrentCulture, "{0} {1}", topRecommendedAddress.UnmatchedApartmentPrefix, topRecommendedAddress.Address1);
					var viewItemWithPrefix = new ListViewItem(ConstructAddressForDisplay(addressWithPrefix)) { Tag = addressWithPrefix, ImageKey = "UnmatchedApartment3232" };
					AddressesListView.Items.Add(viewItemWithPrefix);
				}
				else if (!string.IsNullOrEmpty(topRecommendedAddress.UnmatchedApartmentSuffix))
				{
					var addressWithSuffix = topRecommendedAddress.ShallowCopy();
					addressWithSuffix.UnmatchedApartmentSuffix = topRecommendedAddress.UnmatchedApartmentSuffix;
					addressWithSuffix.Address1 = string.Format(CultureInfo.CurrentCulture, "{0} {1}", topRecommendedAddress.Address1, topRecommendedAddress.UnmatchedApartmentSuffix);
					var viewItemWithSuffix = new ListViewItem(ConstructAddressForDisplay(addressWithSuffix)) { Tag = addressWithSuffix, ImageKey = "UnmatchedApartment3232" };
					AddressesListView.Items.Add(viewItemWithSuffix);
				}
			}

			// Suggestions
			if (suggestedAddresses != null)
			{
				var groupHeaderFont = new Font(AddressesListView.Font.FontFamily, AddressesListView.Font.Size, FontStyle.Bold);
				var groups = suggestedAddresses.GroupBy(x => x.Group);

				foreach (var group in groups)
				{
					if (group.Count() == 1)
					{
						var suggestedAddress = group.First();
						var viewItem = new ListViewItem(ConstructAddressForDisplay(suggestedAddress)) { Tag = suggestedAddress };
						AddressesListView.Items.Add(viewItem);
					}
					else
					{
#if !WINZOR //TODO add ListViewItem to Winzor
						var listviewGroup = new ListViewGroup(GetGroupHeaderByStateProvinceValidationRule(group.First()));
						foreach (var suggestedAddress in group)
						{
							var addressForDisplay = ConstructAddressForDisplay(suggestedAddress);
							if (addressForDisplay != topRecommendedAddressText)
							{
								var item = new ListViewItem(addressForDisplay) { Tag = suggestedAddress };
								listviewGroup.Items.Add(item);
							}
						}
						AddressesListView.Groups.Add(listviewGroup);
#endif
					}
				}
			}

			AddressesListView.SetBounds(0, 0, AddressesListView.Width, AddressesListView.Height);

			SetupListViewProperties();
			SelectRecommendedAddress();
		}

#if !WINZOR //TODO add ListViewItem to Winzor
		string GetGroupHeaderByStateProvinceValidationRule(ValidationResultItem item)
		{
			var group = string.Empty;
			var apartment = item.Apartment ?? string.Empty;
			var streetNumber = item.StreetNumber ?? string.Empty;
			var street = item.Street ?? string.Empty;

			// Only include street number in the group if the apartment is populated
			if (String.IsNullOrEmpty(apartment))
			{
				streetNumber = string.Empty;
			}

			var city = item.City ?? string.Empty;
			city = city.Trim();

			var state = string.Empty;
			if (Address.Country == null || !Address.Country.IsStateMustNotBeEntered)
			{
				if (!string.IsNullOrEmpty(item.State))
				{
					state = item.State.Trim();
				}
			}

			var postcode = item.Postcode ?? string.Empty;
			postcode = postcode.Trim();

			group = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4}", streetNumber, street, city, state, postcode);

			do
			{
				group = group.Replace("  ", " ");
			} while (group.Contains("  "));

			return group;
		}
#endif

		void SetupListViewProperties()
		{
			if (ListViewInitialized)
			{
				return;
			}

#if !WINZOR
			AddressesListView.ShowGroups = true;
#endif
			AddressesListView.View = View.Details;
			AddressesListView.MultiSelect = false;
			AddressesListView.DoubleClick += addressesListView_DoubleClick;
			AddressesListView.KeyDown += addressesListView_KeyDown;
			AddressesListView.SelectedIndexChanged += addressesListView_SelectedIndexChanged;
			ListViewInitialized = true;
		}

		bool ListViewInitialized;

		void addressesListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateAddressButton.Enabled = AddressesListView.SelectedItems.Count > 0;
		}

		void SelectRecommendedAddress()
		{
			if (HasTopRecommendedItem && AddressesListView.Items.Count > 0)
			{
				AddressesListView.Items[0].Selected = true;
				AddressesListView.Select();
				AddressesListView.HideSelection = false;
				AddressesListView.Focus();
			}
		}

		#endregion

		#region Event Handlers

		void addressesListView_DoubleClick(object sender, EventArgs e)
		{
			SelectAddressAndClose();
		}

		void addressesListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				SelectAddressAndClose();
			}
		}

		void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			if (AddressesListView.SelectedItems.Count > 0)
			{
				SelectAddressAndClose();
			}
			else
			{
				Globals.Message.Show(Res.GetString("e8a65ca4-ebbf-4f84-a430-8d715a5b46df", "Please select an address."));
			}
		}

		void HandleAddressSelected(object sender, AddressSelectedEventArgs e)
		{
			e.Address.IsUpdatingCityTown = true;
			AddressValidationService.SetSuggestedAddressToAddressForValidation(e.SelectedAddress, e.PassAsEntered, e.Address);
			if (AddressValidationService.Check_UnrestrictedAdditionalAddressInformationExceedMaxLength(e.Address))
			{
				Globals.Message.Show(AddressValidationService.Constants.ExceedLengthOfAdditionalAddress);
			}

			if (e.Address.ValidationStatus == AddressValidationStatus.ManuallyVerified)
			{
				e.Address.ValidatePostcodeAndStateForAddress();
			}
			e.Address.IsUpdatingCityTown = false;
		}

		public void SelectAddressAndClose()
		{
			if (AddressesListView.SelectedItems.Count > 0)
			{
				if (AddressesListView.SelectedItems[0].Tag is ValidationResultItem selectedItem && AddressSelected != null)
				{
					AddressSelected(this, new AddressSelectedEventArgs() { SelectedAddress = selectedItem, PassAsEntered = false, Address = Address });
					Close();
				}
			}
		}

		public void MoveSelection(int amountToMove)
		{
			var count = AddressesListView.Items.Count;
			if (count > 0 && amountToMove != 0)
			{
				var currentItemIndex = AddressesListView.SelectedItems.Count == 0 ? AddressesListView.Items[0].Index : AddressesListView.SelectedItems[0].Index;
				var newSelectedItemIndex = currentItemIndex + amountToMove;
				newSelectedItemIndex = ((newSelectedItemIndex % count) + count) % count;

				if (AddressesListView.SelectedItems.Count != 0)
				{
					AddressesListView.SelectedItems[0].Selected = false;
				}

				AddressesListView.Items[newSelectedItemIndex].Selected = true;
#if !WINZOR //TODO add ListViewItem to Winzor
				AddressesListView.Items[newSelectedItemIndex].EnsureVisible();
#endif
			}
		}

#if DEBUG
		internal
#endif
		void ManualVerifyButton_Click(object sender, EventArgs e)
		{
			if (Env.Security.OrgAddressesAllowedManualVerification.IsAllowed)
			{
				if (ShouldWarnOnManuallyVerify)
				{
					var message = string.Empty;
					if (this.HasTopRecommendedItem)
					{
						message = Res.GetString("FC30FB06-7E98-42A8-85CA-1924B01E83C6", @"You have chosen to manually confirm that the address as entered is valid. Only click OK if you are certain the address entered is valid and none of the suggestions match the address.");
					}
					else
					{
						message = Res.GetString("C90A4EA7-FD54-4C3C-AACD-20BB75A1F9F5", @"You have chosen to manually confirm that the address as entered is valid. Only click OK if you are certain the address entered is valid and none of the suggestions match the address.

	Please Note: Manually verified addresses do not store coordinates. This can affect functions that rely on coordinates.");
					}

					var title = Res.GetString("E6EE9394-7505-4E22-8F0E-3C457251F28D", "Confirm Original Address is Correct");
					if (Globals.Message.Show(message, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
					{
						return;
					}
				}
				if (AddressSelected != null)
				{
					ValidationResultItem address = null;

					if (this.HasTopRecommendedItem && AddressesListView.Items.Count > 0)
					{
						address = AddressesListView.Items[0].Tag as ValidationResultItem;
					}
					AddressSelected(this, new AddressSelectedEventArgs() { SelectedAddress = address, PassAsEntered = true, Address = this.Address });

					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("c3251529-60d7-436c-8d2c-310a0ea8a2e6", @"You do not have the appropriate security rights to manually verify an address.

If you require access to manually verify an address, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.OrgAddressesAllowedManualVerification.DisplayTextPathToSecurityRight), ResString.GetMultilingualString("dce9a287-e7cb-48a6-9b77-a0b838bc17ed", "Access Denied: {0}", Env.Security.OrgAddressesAllowedManualVerification.DisplayText)
);
			}
		}

		#endregion
	}
}
