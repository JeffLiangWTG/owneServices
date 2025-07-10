using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class ClientRatesModule : RatingModule
	{
		const bool DoInterfaceConnectorLicenceCheck = false;

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.ClientRates;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ClientRateWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Rating };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ClientRates);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ClientRatesFilterControl(GridCollection, (ClientRatesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RateCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ClientRatesFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ClientRates;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Action Menu / Toolbar Buttons

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var newClientRateMenuItemName = ResString.GetMultilingualString("1a9c4e38-8086-4e19-acc4-9f630f679a22", "New Client Rate (Default)");
				var newClientRateMenuItem = new ZMenuItem(newClientRateMenuItemName, (s, e) => CreateAndShowNewClientRate());
				newClientRateMenuItem.DefaultItem = true;
				newClientRateMenuItem.Name = newClientRateMenuItemName;

				NewMenuItem.MenuItems.Add(newClientRateMenuItem);

				var newGlobalClientRateMenuItemName = ResString.GetMultilingualString("aaae544d-d6da-4673-a640-ca45202feaae", "New Global Client Rate");
				var newGlobalClientRateMenuItem = new ZMenuItem(newGlobalClientRateMenuItemName, (s, e) => CreateAndShowNewClientRate(true));
				newGlobalClientRateMenuItem.Name = newGlobalClientRateMenuItemName;

				NewMenuItem.MenuItems.Add(newGlobalClientRateMenuItem);
			}

			return result;
		}

		protected virtual bool ShouldAddGRIUpdateMenuItem => true;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			if (ShouldAddGRIUpdateMenuItem)
			{
				menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));

				MenuItem bulkUpdateMenuItem = new ZMenuItem(BulkRateCostUpdateText, new EventHandler(BulkUpdate_Click));
				MenuItem gRIUpdateMenuItem = new ZMenuItem(GRIUpdateText, new EventHandler(GRIUpdate_Click));
				MenuItem bulkUpdateSubMenu = new ZMenuItem(BulkUpdateSubMenuText, new MenuItem[] { bulkUpdateMenuItem, gRIUpdateMenuItem });
				menuItemCollection.Insert(Math.Max(0, menuItemCollection.Count - 1), bulkUpdateSubMenu);
				menuItemCollection.Insert(Math.Max(0, menuItemCollection.Count - 1), new ZMenuItem("-"));
			}

			return menuItemCollection.ToArray();
		}

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				List<ToolBarButton> buttons = new List<ToolBarButton>(base.ToolBarButtons);
				foreach (ZToolBarButton button in buttons)
				{
					if (button.Text == BulkUpdateSubMenuText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Briefcase);
					}
				}

				return buttons.ToArray();
			}
		}

		void CreateAndShowNewClientRate(bool isGlobal = false)
		{
			var controller = new ClientRatesController();
			if (isGlobal)
			{
				controller.SetNewRatingHeaderAsGlobal();
			}

			controller.ShowNewForm();
		}

		#region Bulk Update

		void GRIUpdate_Click(object sender, EventArgs e)
		{
			GetNewUpdateRatesController().ShowNewForm();
		}

		ZController GetNewUpdateRatesController()
		{
			return ZControllerFactory.Create(ControllerIDs.ClientRateUpdates);
		}

		static MultilingualString GRIUpdateText => ResString.GetMultilingualString("a8c28739-4c36-4894-9d14-6215cba32b6a", "Send Notifications");

		void BulkUpdate_Click(object sender, EventArgs e)
		{
			GetNewBulkRateUpdatesController().ShowNewForm();
		}

		ZController GetNewBulkRateUpdatesController()
		{
			BulkRateUpdatesController result = (BulkRateUpdatesController)ZControllerFactory.Create(ControllerIDs.BulkRateUpdates);
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.ClientRate;
			return result;
		}

		#endregion

		#endregion

		#region XmlImport

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new ClientRatesValueObjectDataAdapter(), DoInterfaceConnectorLicenceCheck).PromptUserAndImport(BillingInterfaceName.ClientRatesXmlImport); // Interface name for billing purposes
		}

		#endregion
	}
}

