using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class LegsGrid : ZGrid
	{
		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();
			UpdateVesselImoMenuItem = new ZMenuItem(ResString.GetMultilingualString("E35EC922-5B44-4F04-A974-75018D705465", "Update Vessel IMO"), UpdateVesselImoMenuItem_Click);
			CreateNewVesselMenuItem = new ZMenuItem(ResString.GetMultilingualString("253B2AF7-E10A-4B87-981F-5066F0A8AB2D", "Create New Vessel"), CreateNewVesselMenuItem_Click);
			AssignScacCodeToCarrierMenuItem = new ZMenuItem(ResString.GetMultilingualString("9D47916D-963A-46CD-A1B1-78C92AAFB57B", "Assign SCAC Code To Carrier"), AssignScacCodeToCarrierMenuItem_Click);
			ContextMenu.MenuItems.Add(UpdateVesselImoMenuItem);
			ContextMenu.MenuItems.Add(CreateNewVesselMenuItem);
			ContextMenu.MenuItems.Add(AssignScacCodeToCarrierMenuItem);
		}

#if DEBUG
		public
#endif
		ZMenuItem UpdateVesselImoMenuItem;

#if DEBUG
		public
#endif
		ZMenuItem CreateNewVesselMenuItem;

#if DEBUG
		public
#endif
		ZMenuItem AssignScacCodeToCarrierMenuItem;

		protected override void HookContextMenu()
		{
			base.HookContextMenu();
			ContextMenu.Popup += ContextMenu_Popup;
		}

		protected override void UnHookContextMenu()
		{
			base.UnHookContextMenu();
			ContextMenu.Popup -= ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			CreateNewVesselMenuItem.Enabled = NewVesselCanBeCreated;
			UpdateVesselImoMenuItem.Enabled = VesselImoCanBeUpdated;
			AssignScacCodeToCarrierMenuItem.Enabled = ScacCodeCanBeAssignedToCarrier;
		}

		bool NewVesselCanBeCreated
		{
			get
			{
				var result = false;

				if (Env.Security.VesselsModify.IsAllowed && IsMouseOnAValidRow)
				{
					var currentLeg = (Leg)ListManager.GetCurrent();

					if (currentLeg != null)
					{
						existingVesselWithSameImoButDifferentVesselName = currentLeg.ExistingVesselWithSameImoButDifferentVesselName;
						result = (existingVesselWithSameImoButDifferentVesselName != null);
					}
				}

				return result;
			}
		}

		RefVessel existingVesselWithSameImoButDifferentVesselName;

		bool VesselImoCanBeUpdated
		{
			get
			{
				var result = false;

				if (Env.Security.VesselsModify.IsAllowed && IsMouseOnAValidRow)
				{
					var currentLeg = (Leg)ListManager.GetCurrent();

					if (currentLeg != null)
					{
						var vessels = currentLeg.GetExistingVesselsByImo();

						if (vessels != null && vessels.Length == 0)
						{
							existingVesselWithSameVesselNameButDifferentImo = currentLeg.ExistingVesselWithSameVesselNameButDifferentImo;
							result = (existingVesselWithSameVesselNameButDifferentImo != null);
						}
					}
				}

				return result;
			}
		}

		RefVessel existingVesselWithSameVesselNameButDifferentImo;

		bool ScacCodeCanBeAssignedToCarrier
		{
			get
			{
				var result = false;

				if (Env.Security.OrgConfigModifyRegistrationNumbers.IsAllowed && IsMouseOnAValidRow)
				{
					var currentLeg = (Leg)ListManager.GetCurrent();
					result = (currentLeg != null && currentLeg.ScacCodeCanBeAssginedToCarrier);
				}

				return result;
			}
		}

		void CreateNewVesselMenuItem_Click(object sender, EventArgs e)
		{
			var currentLeg = (Leg)ListManager.GetCurrent();

			if (currentLeg == null)
			{
				Globals.Message.ShowError(LegIsNotSelectedError);
				return;
			}

			var message = Res.GetString("C5E238A1-CA98-489C-A927-722403CA9C3B", "During this operation the existing vessel with name {0} will be deactivated and the vessel with name {1} will be created if it does not exist or activated if it is inactive. Are you sure you want to proceed?", existingVesselWithSameImoButDifferentVesselName.RV_Name, currentLeg.VesselName);
			var dialogResult = Globals.Message.Show(message, Res.GetString("B4BDA44F-4A61-472A-8E61-6AEFDDAD8E13", "Create new vessel name"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dialogResult == DialogResult.Yes)
			{
				ApplyMenuAction((out string resultMessage) => currentLeg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			}
		}

		void UpdateVesselImoMenuItem_Click(object sender, EventArgs e)
		{
			var currentLeg = (Leg)ListManager.GetCurrent();

			if (currentLeg == null)
			{
				Globals.Message.ShowError(LegIsNotSelectedError);
				return;
			}

			var message = Res.GetString("BEE853F3-A26D-4AEA-8280-452BACF4D965", "During this operation the IMO {0} of existing vessel with name {1} will be substituted with IMO {2}. Are you sure you want to proceed?", existingVesselWithSameVesselNameButDifferentImo.RV_LloydsNumber, currentLeg.VesselName, currentLeg.LloydsNumber);
			var dialogResult = Globals.Message.Show(message, Res.GetString("A967D4FF-C06E-4D5E-9D86-E48EADCD8CB6", "Update vessel IMO"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dialogResult == DialogResult.Yes)
			{
				ApplyMenuAction((out string resultMessage) => currentLeg.UpdateVesselImo(existingVesselWithSameVesselNameButDifferentImo, out resultMessage));
			}
		}

		void AssignScacCodeToCarrierMenuItem_Click(object sender, EventArgs e)
		{
			var currentLeg = (Leg)ListManager.GetCurrent();

			if (currentLeg == null)
			{
				Globals.Message.ShowError(LegIsNotSelectedError);
				return;
			}

			var orgModule = GetOrgModule();
			var orgPopup = new EmbeddedModulePopup(orgModule);
			var findBox = new ZSimpleFindBox(new OrganisationsFindBoxCollection(new BusinessObjectFactory()), orgPopup);
			var provider = new LegsPopupModuleDecisionProvider(findBox, currentLeg.CarrierSCAC);
			orgModule.OverrideModuleDecisionProvider(provider);
			orgPopup.EmbeddedModulePopupOKButtonStrategy = provider;
			orgPopup.Selected += new EmbeddedModulePopup.SelectedEventHandler(OrgPopupSelected);

			ZFormModaliser.Show(orgPopup, null);
		}

		void OrgPopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var currentLeg = (Leg)ListManager.GetCurrent();

			if (currentLeg == null)
			{
				Globals.Message.ShowError(LegIsNotSelectedError);
				return;
			}

			var carrier = (OrgHeader)e.SelectedBusinessObjects[0];
			ApplyMenuAction((out string resultMessage) => currentLeg.AssignScacCodeToCarrier(carrier, out resultMessage));
		}

		protected ZFilterModule GetOrgModule()
		{
			var orgModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation);

			var defaults = new FilterBusinessObjectDefaults();
			var orgTypesFilterDefault = new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True, false);
			defaults.Add(orgTypesFilterDefault);
			var orgSecondaryTypeFilterDefault = new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)OrgConstants.FilterControl.SecondaryOrgType.ShippingLine);
			defaults.Add(orgSecondaryTypeFilterDefault);
			var orgNameFilterDefault = new FilterBusinessObjectDefault("Name", "Property", ZString.Empty);
			defaults.Add(orgNameFilterDefault);
			var orgCodeFilterDefault = new FilterBusinessObjectDefault("Code", "Property", ZString.Empty);
			defaults.Add(orgCodeFilterDefault);
			orgModule.FilterBusinessObject.SetExternalDefaults(defaults);

			return orgModule;
		}

		public event EventHandler LegValidityChanged;

		void OnLegValidityChanged(EventArgs e)
		{
			var handler = LegValidityChanged;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		delegate bool MenuActionDelegate(out string resultMessage);

		void ApplyMenuAction(MenuActionDelegate menuAction)
		{
			string resultMessage;

			if (menuAction(out resultMessage))
			{
				OnLegValidityChanged(EventArgs.Empty);
				Globals.Message.Show(resultMessage);
			}
			else
			{
				Globals.Message.ShowError(resultMessage);
			}
		}

		string LegIsNotSelectedError => Res.GetString("429FE94D-5865-401D-A329-8FF2F28918F3", "Leg is not selected");
	}
}
