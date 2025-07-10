using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RelatedPartiesUserControl : ZUserControl
	{
		public RelatedPartiesUserControl()
		{
			InitializeComponent();

			RelatedPartiesGrid.GetDeleteMenuVisibleMethod =
				delegate
				{
					BusinessObject obj = (BusinessObject)RelatedPartiesGrid.List[RelatedPartiesGrid.CurrentCell.RowNumber];
					return obj is OrgRelatedParty && ((OrgRelatedParty)obj).IsAlrightToAddOrDelete;
				};
			RelatedPartiesGrid.ContextMenu.Popup += RelatedPartiesGridContextMenu_Popup;
		}

		void RelatedPartyFindButton_Click(object sender, EventArgs e)
		{
			OrgHeader header = DataSource as OrgHeader;
			if (header != null)
			{
				header.AllParentPartiesView.FilterByPartyType(header.FilterPartyType, header.FilterFreightDirection);
				header.AllRelatedPartiesView.FilterByPartyType(header.FilterPartyType, header.FilterFreightDirection);
			}
		}

		void RelatedPartyClearButton_Click(object sender, EventArgs e)
		{
			OrgHeader header = DataSource as OrgHeader;
			if (header != null)
			{
				this.RelatedPartyGridFilterPartyTypeDropEdi.Text = ZString.Empty;
				this.RelatedPartyGridFilterDirectionDropEdi.Text = ZString.Empty;
				header.FilterFreightDirection = ZString.Empty;
				header.FilterPartyType = ZString.Empty;
				header.AllParentPartiesView.ClearRelatedPartyFilters();
				header.AllRelatedPartiesView.ClearRelatedPartyFilters();
			}
		}

		internal void RelatedPartiesGridContextMenu_Popup(object sender, EventArgs e)
		{
			var gridMenuItems = RelatedPartiesGrid.ContextMenu.MenuItems;
			if (RelatedPartiesGrid.SelectedElements.Length > 0 &&
				RelatedPartiesGrid.SelectedElements.All(element => element is OrgRelatedParty &&
					((OrgRelatedParty)element).IsCSARelatedPartyType &&
					((OrgRelatedParty)element).PR_CustomsStatus != CSARelatedPartyStatusList.Codes.Added))
			{
				if (!gridMenuItems.Contains(SetRelatedPartyAddedMenuItem))
				{
					gridMenuItems.Add(SetRelatedPartyAddedMenuItem);
				}
				if (!gridMenuItems.Contains(RefreshRelatedPartyMenuItem))
				{
					gridMenuItems.Add(RefreshRelatedPartyMenuItem);
				}
			}
			else
			{
				if (gridMenuItems.Contains(SetRelatedPartyAddedMenuItem))
				{
					gridMenuItems.Remove(SetRelatedPartyAddedMenuItem);
				}
				if (gridMenuItems.Contains(RefreshRelatedPartyMenuItem))
				{
					gridMenuItems.Remove(RefreshRelatedPartyMenuItem);
				}
			}
		}

		#region SetRelatedPartyAddedMenuItem

		MenuItem SetRelatedPartyAddedMenuItem
		{
			get
			{
				if (fSetRelatedPartyAddedMenuItem == null)
				{
					fSetRelatedPartyAddedMenuItem = new ZMenuItem(ResString.GetMultilingualString("RelatedPartiesUserControl|735c9f9d-7959-4fff-8146-6f4ca324f325", "Set Related Party as already added"),
					new EventHandler(OnSetRelatedPartyAddedClicked));
				}

				return fSetRelatedPartyAddedMenuItem;
			}
		}

		MenuItem fSetRelatedPartyAddedMenuItem;

		void OnSetRelatedPartyAddedClicked(object sender, EventArgs e)
		{
			var message = Res.GetString("47be0c3b-3b58-488e-a071-49ebdae1ff31", "You should only manually change the CSA Status to Added if the related party was registered with Customs externally to {0}. Do you wish to continue?", BrandingFactory.Instance.ProductName);
			if (RelatedPartiesGrid.SelectedElements.Length > 0 && Globals.Message.ShowConfirmation(message, ConfirmationCaption, ConfirmationString, MessageBoxIcon.Question) == DialogResult.OK)
			{
				foreach (OrgRelatedParty relatedParty in RelatedPartiesGrid.SelectedElements)
				{
					if (relatedParty != null)
					{
						relatedParty.UpdateCSAStatusAsAddedIfApplicable();
					}
				}
			}
		}

		#endregion

		#region RefreshRelatedPartyMenuItem

		MenuItem RefreshRelatedPartyMenuItem
		{
			get
			{
				if (fRefreshRelatedPartyMenuItem == null)
				{
					fRefreshRelatedPartyMenuItem = new ZMenuItem(ResString.GetMultilingualString("RelatedPartiesUserControl|64e3b3eb-8400-4f25-9956-58b35cee4081", "Refresh the Related Party"),
					new EventHandler(OnRefreshRelatedPartyClicked));
				}

				return fRefreshRelatedPartyMenuItem;
			}
		}

		MenuItem fRefreshRelatedPartyMenuItem;

		void OnRefreshRelatedPartyClicked(object sender, EventArgs e)
		{
			var message = Res.GetString("c81c3d51-dddf-4926-8667-899c8d38c36b", "This will result in the details at Customs being refreshed by first sending a delete message and if that is accepted a subsequent add message will be sent. Do you wish to continue?");
			if (RelatedPartiesGrid.SelectedElements.Length > 0 && Globals.Message.ShowConfirmation(message, ConfirmationCaption, ConfirmationString, MessageBoxIcon.Question) == DialogResult.OK)
			{
				foreach (OrgRelatedParty relatedParty in RelatedPartiesGrid.SelectedElements)
				{
					if (relatedParty != null)
					{
						relatedParty.UpdateCSAStatusAsRefreshPendingIfApplicable();
					}
				}
			}
		}

		#endregion

		internal static string ConfirmationCaption
		{
			get { return Res.GetString("01f29433-e76c-45f8-bf77-30e4d9d72229", "Continue"); }
		}

		internal static string ConfirmationString
		{
			get { return Res.GetString("31fc3a36-e18e-4dad-82c3-278c0183df67", "Yes"); }
		}
	}
}
