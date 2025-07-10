using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class NameAndAddressControlForTesting : NameAndAddressDetailsUserControl
	{
		public void FillAddressFields(NameAndAddressControlForTesting control, OrgHeader org)
		{
			control.Address1TextBox.Text = org.MainAddress.Address1;
			control.Address2TextBox.Text = org.MainAddress.Address2;
			control.StateDropEdit.Text = org.MainAddress.StateCode;
			control.CityTextBox.Text = org.MainAddress.City;
			control.PostCodeTextBox.Text = org.MainAddress.Postcode;
		}

		public bool DeduplicationStatusIconVisible => DuplicateDetectionStatusLabel.Visible && DuplicateDetectionStatusIcon.Visible;

		public bool DeduplicationStatusLabelReads(string text) => DuplicateDetectionStatusLabel.Text.Equals(text);

		public bool HasTimeoutToolTip => "Duplicate results for this record can be accessed in the MDM admin panel".Equals(ToolTipService.GetToolTip(this.DuplicateDetectionStatusLabel));

		public bool DeduplicationStatusLabelColorIs(Color color) =>
			this.DuplicateDetectionStatusLabel.ForeColor.Equals(color);

		public bool DeduplicationStatusLabelStyleIs(FontStyle style) =>
			DuplicateDetectionStatusLabel.Font.Style.Equals(style);

		public bool DeduplicationStatusLabelCursorIs(Cursor cursor) =>
		DuplicateDetectionStatusLabel.Cursor.Equals(cursor);

		public void DuplicateDetectionLabelMouseClick()
		{
			DuplicateDetectionStatusLabelOnClick(this, EventArgs.Empty);
		}

		public void DuplicateDetectionLabelMouseEnter()
		{
			DuplicateDetectionStatusLabelOnMouseEnter(this, EventArgs.Empty);
		}

		public void DuplicateDetectionLabelMouseLeave()
		{
			DuplicateDetectionStatusLabelOnMouseLeave(this, EventArgs.Empty);
		}

		public bool SimulateIsOnSelectedTab { get; set; }

		public bool StateDropDownVisible
		{
			get { return StateBoundDropEdit.Visible; }
		}

		public ZButton NavigateToWeb_Exposed
		{
			get { return base.GoToUrlButton; }
		}

		public ZTextBox FullNameTextBox
		{
			get { return FullNameTextBox; }
		}

		public ZTextBox Address1TextBox
		{
			get { return Address1BoundTextBox; }
		}

		public ZTextBox Address2TextBox
		{
			get { return Address2BoundTextBox; }
		}

		public ZTextBox CityTextBox
		{
			get { return CityBoundTextBox; }
		}

		public ZDropEdit StateDropEdit
		{
			get { return StateBoundDropEdit; }
		}

		public ZTextBox PostCodeTextBox
		{
			get { return PostCodeBoundTextBox; }
		}

		public ZButton ScreenButton_Exposed
		{
			get { return ScreenButton; }
		}

		public ZButton ValidateAddressButton_Exposed
		{
			get { return ValidateAddressButton; }
		}

		public ZTextBox RegistrationNumberTextBox_Exposed
		{
			get { return RegistrationNumberTextBox; }
		}

		public ZDropEdit RegistrationNumberTypeDropEdit_Exposed
		{
			get { return RegistrationNumberTypeDropEdit; }
		}

		public ZLabel DuplicateExclusionsLabel_Exposed => DuplicateExclusionsLabel;

		public Control.ControlCollection AddressFieldControls => MainAddressDetailsGroupBox.Controls;

		public CharacterCasing AllowedCharacterCasingForControl(string controlName)
		{
			CharacterCasing casing = CharacterCasing.Lower;
			foreach (Control control in MainAddressDetailsGroupBox.Controls)
			{
				if (control.Name == controlName)
				{
					casing = ((ZTextBox)control).CharacterCasing;
				}
			}
			return casing;
		}

		protected override void NavigateToWeb(string webAddress)
		{
			WebWasNavigated = true;
		}

		public bool WebWasNavigated;

		public ZLinkLabel AllBranchesLinkExp
		{
			get { return AllBranchesLink; }
		}

		public void PerformAllBranchesLinkClick()
		{
			AllBranchesLink_LinkClicked(this, null);
		}

		public void PerformValidateAddressClick()
		{
			ValidateAddressButton_Click(this, null);
		}

		public void PerformClearFieldsClick()
		{
			ClearFieldsButton_Click(this, null);
		}

		public override bool IsOnCurrentSelectedTab()
		{
			if (SimulateIsOnSelectedTab)
			{
				return true;
			}

			return base.IsOnCurrentSelectedTab();
		}

		public IDuplicationEventArgs CurrentDuplicationEventArgs
		{
			get { return currentDuplicationEventArgs; }
			set { currentDuplicationEventArgs = value; }
		}

		public void FireNameAndAddressDetailsUserControl_Resize()
		{
			NameAndAddressDetailsUserControl_Resize(null, null);
		}

		protected override void NameAndAddressDetailsUserControl_Resize(object sender, EventArgs e)
		{
			var suggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);
			if (suggestionControl != null && suggestionControl.Visible)
			{
				resizeAddressSuggestionControl = true;
			}
		}
		public bool resizeAddressSuggestionControl;

		public DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEditForTest => ScreeningStatusDropEdit;
	}
}
