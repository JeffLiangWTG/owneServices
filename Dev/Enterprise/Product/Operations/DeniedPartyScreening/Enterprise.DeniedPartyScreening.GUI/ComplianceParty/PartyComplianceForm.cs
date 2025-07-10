using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public enum PartyComplianceButton
	{
		Cancel = 0,
		ForceRescreen = 1,
		Screen = 2
	}

	public partial class PartyComplianceForm : ZChildForm
	{
		public PartyComplianceForm(PartyComplianceWrapperFilteredCollection collection, bool forComplianceRisk = false)
			: base(collection)
		{
			InitializeComponent();
			ScreenButton.Visible = collection.UnprocessedParties.Any(u => ((PartyComplianceWrapper)u).OrgCode != unmatchedOrgCode);
			ForceRescreenButton.Visible = collection.UnprocessedParties.Any(u => ((PartyComplianceWrapper)u).OrgCode != unmatchedOrgCode) || collection.ProcessedParties.Any(p => ((PartyComplianceWrapper)p).OrgCode != unmatchedOrgCode);
			CloseButton.Select();

			SetExcludeScreenMessageLable();

			InitializeGrid(ProcessedWrappersGrid);
			InitializeGrid(UnprocessedWrappersGrid);

			collection.UnprocessedParties.FirstOrDefault(u => ((PartyComplianceWrapper)u).OrgCode == unmatchedOrgCode)?.AddRowWarning(unmatchedOrgWarning);
			collection.ProcessedParties.FirstOrDefault(u => ((PartyComplianceWrapper)u).OrgCode == unmatchedOrgCode)?.AddRowWarning(unmatchedOrgWarning);

			if (forComplianceRisk)
			{
				CaptionResourceString = Res.GetData("e9bc1890-e858-4ba4-be8b-5bd75ae100ca", "Party Risk");
			}
		}

		readonly string unmatchedOrgCode = OrgHeader.UnmatchedOrganisationCode;
		readonly string unmatchedOrgWarning = DeniedPartyScreeningHelper.GetUnmatchedOrganizationMessage;

		DeniedPartyGridColorHelper GridColorHelper => gridColorHelper ?? (gridColorHelper = new DeniedPartyGridColorHelper());
		DeniedPartyGridColorHelper gridColorHelper;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to be localizable")]
		const string editOrganizationMenuName = "Edit Organization";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to be localizable")]
		const string ScreenSelectedMenuName = "Screen";

		void InitializeGrid(ZGrid grid)
		{
			grid.ColourDeciding += Grid_ColorDeciding;
			grid.Hotkeys.RegisterHotKey(Keys.F3, () => EditOrganization(grid));
			grid.ContextMenu.MenuItems.InsertRange(3, new[]
			{
				new ZMenuItem(ResString.GetMultilingualString("36FEEC98-6831-4003-99A3-148BA36CDA7C", "Edit Organization"), EditOrganization_Click) { Name = editOrganizationMenuName, Enabled = false },
				new ZMenuItem(ResString.GetMultilingualString("344EB6F0-0067-4A5F-B184-0784E88BE2CB", "Screen"), ScreenSelectedParties) { Name = ScreenSelectedMenuName, Enabled = false },
			});
			grid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var contextMenu = sender as ContextMenu;
			if (contextMenu?.SourceControl is ZGrid parentGrid)
			{
				var editOrganizationMenuItem = contextMenu.MenuItems.FindByName(editOrganizationMenuName);
				editOrganizationMenuItem.Enabled = GetSelectedOrgHeader(parentGrid) != null;

				var screenSelectedPartiesMenuItem = contextMenu.MenuItems.FindByName(ScreenSelectedMenuName);
				screenSelectedPartiesMenuItem.Enabled = GetSelectedParties(parentGrid).Length > 0;
			}
		}

		OrgHeader GetSelectedOrgHeader(ZGrid grid)
		{
			OrgHeader header = null;
			var selectedObject = grid?.GetCurrent() as PartyComplianceWrapper;
			if (selectedObject != null && !selectedObject.OrgCode.IsEmpty && selectedObject.OrgCode != "MISC")
			{
				header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, selectedObject.OrgCode);
			}

			return header;
		}

		ScreeningParty[] GetSelectedParties(ZGrid grid)
		{
			var selectedParties = Array.Empty<ScreeningParty>();
			if (grid?.SelectedElements?.Length > 0)
			{
				selectedParties = grid.SelectedElements.OfType<PartyComplianceWrapper>().Select(u => u.WrappedScreeningParty).ToArray();
			}

			return selectedParties;
		}

		void EditOrganization_Click(object sender, EventArgs e)
		{
			EditOrganization((sender as ZMenuItem)?.ParentControl as ZGrid);
		}

		void ScreenSelectedParties(object sender, EventArgs e)
		{
			if ((sender as ZMenuItem)?.ParentControl is ZGrid grid)
			{
				var selectedParties = GetSelectedParties(grid);
				if (selectedParties.Length > 0)
				{
					if (selectedParties.All(u => u.CurrentScreeningStatus == ScreeningStatusesList.Codes.PermanentClear))
					{
						Globals.Message.ShowWarning(Res.GetString("2F342AFC-FBC0-426F-97C1-6D8B03A9D0FA", "Unable to screen parties that are in Permanent Clear status"));
					}
					else
					{
						if (grid == ProcessedWrappersGrid)
						{
							if (DpsSecurityRights.IsGrantedScreeningForceReScreenWithShowError())
							{
								PerformButtonClick = PartyComplianceButton.ForceRescreen;
								SelectedParties = selectedParties;
								Close();
							}
						}
						else
						{
							PerformButtonClick = PartyComplianceButton.Screen;
							SelectedParties = selectedParties;
							Close();
						}
					}
				}
			}
		}

		void EditOrganization(ZGrid parentGrid)
		{
			if (!Env.Security.OrganisationModify.IsAllowed)
			{
				Env.Security.OrganisationModify.ShowError();
			}
			else
			{
				var organisation = GetSelectedOrgHeader(parentGrid);
				if (organisation != null)
				{
					var organisationController = ZControllerFactory.Create(ControllerIDs.Organisation);
					organisationController.SetFormsModalTo(parentGrid.FindForm());
					organisationController.ShowEditForm(organisation);

					var organisationForm = (ZForm)ZApplication.GetOpenForms().Where(f => f.Name == "ZOrganisationsForm").FirstOrDefault();
					if (organisationForm != null)
					{
						organisationForm.BusinessEntity.Factory.Saved += delegate
						{
							organisationForm.FormClosed += delegate
							{
								Globals.Message.ShowInformation(ResString.GetMultilingualString("FC1AE6F7-ECA9-45D2-B13E-ADDCD8891F23", "The organization data has been modified, please re-open Party Compliance form."));
								this.Close();
							};
						};
					}
				}
			}
		}

		protected void Grid_ColorDeciding(object sender, ColourDecidingEventArgs e)
		{
			var status = (PartyComplianceWrapper)e.ObjectAtRow;
			e.Colour = GridColorHelper.GetColorForResultStatus(status);
		}

		public override string FormVerb => string.Empty;

		void SetExcludeScreenMessageLable()
		{
			var excludedOrganizations = new StringBuilder();

			if (!Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
			{
				var filteredCollection = (PartyComplianceWrapperFilteredCollection)CurrentDataItem;
				filteredCollection.ProcessedParties
					.Cast<PartyComplianceWrapper>()
					.Where(screeningParty => screeningParty.ScreeningStatus.Equals(ScreeningStatusesList.Codes.PermanentClear))
					.ForEach(currentScreeningParty =>
						excludedOrganizations.AppendLine(FormattableString.Invariant($"	* {currentScreeningParty.OrgCode}    {currentScreeningParty.Code}"))
					);
			}

			if (string.IsNullOrWhiteSpace(excludedOrganizations.ToString()))
			{
				MessagePanel.Visible = false;
				MainPanel.Dock = DockStyle.Fill;
			}
			else
			{
				MessageDetailLabel.Text = excludedOrganizations.ToString();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			PerformButtonClick = PartyComplianceButton.Cancel;
			Close();
		}

		void ForceRescreenButton_Click(object sender, EventArgs e)
		{
			if (DpsSecurityRights.IsGrantedScreeningForceReScreenWithShowError())
			{
				PerformButtonClick = PartyComplianceButton.ForceRescreen;
				Close();
			}
		}

		void ScreenButton_Click(object sender, EventArgs e)
		{
			PerformButtonClick = PartyComplianceButton.Screen;
			Close();
		}

		public PartyComplianceButton PerformButtonClick = PartyComplianceButton.Cancel;

		public ScreeningParty[] SelectedParties { get; set; }
	}
}
