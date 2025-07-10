using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RelatedOrganizationUserControl : ZUserControl
	{
		public RelatedOrganizationUserControl()
		{
			InitializeComponent();
			this.CustomFieldsControl1.NothingSetupMessageLabelText = Res.GetString("RelatedOrganizationUserControl|386DC846-3518-4459-84EC-E61400DA2A01", "To make use of this tab, please setup custom fields in Workflow Manager.");
			GridPartRelations.OnRemovingBizOFromList += new ZGrid.RemoveBizOFromListHandler(OnPartRelationRemoving);
			GridPartRelations.AfterBind += new EventHandler(zGridPartRelations_AfterBind);
			this.BindingSource.SetBindingMember(this.CustomFieldsControl1, "RelatedOrganisations");
		}

		void zGridPartRelations_AfterBind(object sender, EventArgs e)
		{
			UpdateForCurrentlySelectedRelation();
			GridPartRelations.ListManager.CurrentChanged += new EventHandler(PartRelations_CurrentChanged);
		}

		void barcodeParsingLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.BarcodeParsing);
			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Module", "Property", (ZString)"WHS"));
			if (currentRelation != null && currentRelation.Header != null)
			{
				defaults.Add(new FilterBusinessObjectDefault("OptionalBuyer", "Property", currentRelation.Header.PK));
			}
			module.FilterBusinessObject.SetExternalDefaults(defaults);
			module.ShowPopup();
		}

		OrgPartRelation currentRelation;

		void PartRelations_CurrentChanged(object sender, EventArgs e)
		{
			UpdateForCurrentlySelectedRelation();
		}

		void UpdateForCurrentlySelectedRelation()
		{
			OrgPartRelation currentlySelectedRelation = null;

			if (GridPartRelations.ListManager.Count > 0)
			{
				currentlySelectedRelation = (OrgPartRelation)GridPartRelations.ListManager.GetCurrent();
			}

			if (currentRelation != currentlySelectedRelation)
			{
				if (currentRelation != null)
				{
					currentRelation.OnOU_OHChanging -= new CancelEventHandler(currentRelation_OnOU_OHChanging);
				}

				currentRelation = currentlySelectedRelation;
				this.CustomFieldsControl1.SetDataBinding(currentRelation, "");

				if (currentRelation != null)
				{
					currentRelation.OnOU_OHChanging += new CancelEventHandler(currentRelation_OnOU_OHChanging);
				}
			}
		}

		void currentRelation_OnOU_OHChanging(object sender, CancelEventArgs e)
		{
			if (currentRelation != null)
			{
				if (currentRelation.HasRelatedPartPivots)
				{
					string countriesWithPartyOverrides = currentRelation.GetCountryCodesWithRelatedCusClassPartPivot();

					if (Globals.Message.Show(string.Format(PartyClassificationOverridesExist, countriesWithPartyOverrides), Res.GetString("e82e8c96-c63f-43f5-af96-2287aa761739", "Changing Party"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
					{
						currentRelation.DeleteRelatedCusClassPartPivots();
					}
					else
					{
						e.Cancel = true;
					}
				}
			}
		}

		ZGrid.ContinueWithRemove OnPartRelationRemoving(BusinessObject bizOToBeDeleted)
		{
			OrgPartRelation relationToDelete = (OrgPartRelation)bizOToBeDeleted;

			ZGrid.ContinueWithRemove result = ZGrid.ContinueWithRemove.Remove;

			if (relationToDelete.HasRelatedPartPivots)
			{
				string countriesWithPartyOverrides = relationToDelete.GetCountryCodesWithRelatedCusClassPartPivot();

				if (Globals.Message.Show(string.Format(PartyClassificationOverridesExist, countriesWithPartyOverrides), Res.GetString("2eee3ff7-0546-4411-acd7-86e26c94e962", "Deleting a relation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
				{
					result = ZGrid.ContinueWithRemove.CancelRemoval;
				}
			}

			if (result == ZGrid.ContinueWithRemove.Remove)
			{
				relationToDelete.OnOU_OHChanging -= new CancelEventHandler(currentRelation_OnOU_OHChanging);
			}

			return result;
		}

		internal static string PartyClassificationOverridesExist
		{
			get { return Res.GetString("5db14a99-4602-427b-b2d8-382a86a5389a", "There are classification details overridden for this party in {0:G}. If you proceed, all the classification details for this party in those countries/regions will be deleted. Are you sure you wish to continue?"); }
		}

		#region Test
#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void SelectTheFirstRelationAndClickLinkLabelForTesting()
		{
			GridPartRelations.Focus();
			GridPartRelations.Select(0);
			Application.DoEvents();

			barcodeParsingLinkLabel_LinkClicked(this, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void SelectTheRelationAtForTesting(int row)
		{
			GridPartRelations.Focus();
			GridPartRelations.ListManager.Position = row;
			Application.DoEvents();
		}

		public void FocusTheRelationsGridForTesting()
		{
			GridPartRelations.Focus();
		}

		public void SelectCustomFieldsTab()
		{
			MainTabControl.SelectedTab = CustomFieldsTabPage;
		}

		public void SelectAttributesTab()
		{
			MainTabControl.SelectedTab = AttributesTabPage;
		}

		public OrgPartRelation GetCurrentlySelectedRelationRowForTesting()
		{
			return (OrgPartRelation)GridPartRelations.ListManager.GetCurrent();
		}
#endif
		#endregion
	}
}
