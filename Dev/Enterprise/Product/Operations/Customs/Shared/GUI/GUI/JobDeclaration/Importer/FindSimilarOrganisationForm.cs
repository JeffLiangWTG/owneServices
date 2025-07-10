using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class FindSimilarOrganisationForm : ZChildForm
	{
		public FindSimilarOrganisationForm()
		{
			InitializeComponent();
		}

		public FindSimilarOrganisationForm(OrganisationFinder finder)
			: base(finder.UnknownOrg)
		{
			InitializeComponent();

			this.finder = finder;

			if (!this.IsDesignMode())
			{
				HideNoRecordsLabel();
				HookFinderUnknownOrgEvents();
				if (finder.UnknownOrg.OH_Code.IsEmpty)
				{
					HeadingLabel.Text = HeadingLabelText;
				}
				else
				{
					OrganisationCodeLabel.Text = finder.UnknownOrg.OH_Code;
					finder.UnknownOrg.OH_Code = ZString.Empty;
				}

				SelectOrganisationLabel.CaptionResourceString = OrganisationLabelResourceString;
			}

			NoRecordsLabel.AllowOverlap(SimilarOrgMatchesBoundGrid);
		}

		public ZString OrganisationCode => fOrganisationCode;

		public ZBool SkipAll => fSkipAll;

		#region Form Overrides

		public override string FormHeading => Res.GetString("43423b95-d3f3-4462-932f-a397995013fb", "Select Organization");

		protected virtual string HeadingLabelText => Res.GetString("42a9cb3a-3812-459d-8c36-710e3b9d18f3", "Cannot find organization. No organization code provided.");

		protected virtual ResourceStringData OrganisationLabelResourceString => Res.GetData("68d8852f-7c99-41c6-a0f0-37edcbe27c54", "Organizations similar to the Organization details provided. Select an organization to set the Organization.");

		#endregion

		#region Implementation

		protected void HideNoRecordsLabel()
		{
			if (finder.UnknownOrg.SimilarOrgMatches.Count > 0)
			{
				NoRecordsLabel.Visible = false;
			}
			else
			{
				NoRecordsLabel.Visible = true;
			}
		}

		#region Event Handlers

		void HookFinderUnknownOrgEvents()
		{
			finder.UnknownOrg.OH_FullNameInfo.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_Address1Info.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_Address2Info.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_CityInfo.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_PhoneInfo.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_PostCodeInfo.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.PrimaryRegistrationNumber.NumberInfo.ValueChanged += OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.OH_RL_NKClosestPortInfo.ValueChanged += OH_RL_NKClosestPortInfo_ValueChanged;
		}

		void UnhookFinderUnknownOrgEvents()
		{
			finder.UnknownOrg.OH_FullNameInfo.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_Address1Info.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_Address2Info.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_CityInfo.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_PhoneInfo.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.MainAddress.OA_PostCodeInfo.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.PrimaryRegistrationNumber.NumberInfo.ValueChanged -= OrgHeaderProperties_ValueChanged;
			finder.UnknownOrg.OH_RL_NKClosestPortInfo.ValueChanged -= OH_RL_NKClosestPortInfo_ValueChanged;
		}

		protected void OrgHeaderProperties_ValueChanged(object sender, EventArgs e)
		{
			finder.FindSimilarOrganisations();
			HideNoRecordsLabel();
		}

		protected void OH_RL_NKClosestPortInfo_ValueChanged(object sender, EventArgs e)
		{
			// Only apply the UNLOCO filter if code contains the country code
			if (finder.UnknownOrg.OH_RL_NKClosestPort.Length >= 2)
			{
				OrgHeaderProperties_ValueChanged(sender, e);
			}
		}

		protected void SkipButton_Click(object sender, EventArgs e)
		{
			fOrganisationCode = ZString.Empty;
			DeleteFinderUnknownOrg();
			Close();
		}

		protected void SkipAllButton_Click(object sender, EventArgs e)
		{
			fOrganisationCode = ZString.Empty;
			fSkipAll = true;
			DeleteFinderUnknownOrg();
			Close();
		}

		protected void CreateNewButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				fOrganisationCode = finder.UnknownOrg.OH_Code;
				Close();
			}
		}

		protected void SelectOrganisationFromGrid(object sender, EventArgs e)
		{
			if (SimilarOrgMatchesBoundGrid.SelectedElements != null && SimilarOrgMatchesBoundGrid.SelectedElements.Length > 0)
			{
				var selectedOrg = (OrgPatternMatch)SimilarOrgMatchesBoundGrid.SelectedElements[0];
				fOrganisationCode = selectedOrg.OH_Code;
				DeleteFinderUnknownOrg();
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("f744b834-7aa0-4536-8a43-3fbbd5dcf15a", "Please select one organization in the grid."));
			}
		}

		void DeleteFinderUnknownOrg()
		{
			UnhookFinderUnknownOrgEvents();
			finder.UnknownOrg.Delete();
		}

		#endregion

		protected ZString fOrganisationCode;
		protected ZBool fSkipAll;
		protected OrganisationFinder finder;

		#endregion

		#region Disposing

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookFinderUnknownOrgEvents();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
