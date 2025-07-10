using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class CostingModule : RatingModule
	{
		const bool DoInterfaceConnectorLicenceCheck = false;

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.Costing;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Rating };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Costing);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CostingFilterControl(GridCollection, (CostingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CostingCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CostingFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CostingRates;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Action Menu / Toolbar Buttons

		protected virtual bool ShouldAddGRIUpdateMenuItem => true;

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var newCostingMenuItemName = ResString.GetMultilingualString("659b3f4f-c3e5-4984-a69e-e3b42c3e8a62", "New Costing (Default)");
				var newCostingMenuItem = new ZMenuItem(newCostingMenuItemName, (s, e) => CreateAndShowNewCosting());
				newCostingMenuItem.DefaultItem = true;
				newCostingMenuItem.Name = newCostingMenuItemName;

				NewMenuItem.MenuItems.Add(newCostingMenuItem);

				var newGlobalCostingMenuItemName = ResString.GetMultilingualString("19c2a4fd-cbaa-4e92-9a18-19ad4c5188e2", "New Global Costing");
				var newGlobalCostingMenuItem = new ZMenuItem(newGlobalCostingMenuItemName, (s, e) => CreateAndShowNewCosting(true));
				newGlobalCostingMenuItem.Name = newGlobalCostingMenuItemName;

				NewMenuItem.MenuItems.Add(newGlobalCostingMenuItem);
			}

			return result;
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			if (ShouldAddGRIUpdateMenuItem)
			{
				menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));

				menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem(CostsComparerText, new EventHandler(CostsComparer_Click)));

				MenuItem bulkUpdateMenuItem = new ZMenuItem(BulkRateCostUpdateText, new EventHandler(BulkUpdate_Click));
				MenuItem bulkUpdateSubMenu = new ZMenuItem(BulkUpdateSubMenuText, new MenuItem[] { bulkUpdateMenuItem });
				menuItemCollection.Insert(menuItemCollection.Count - 1, bulkUpdateSubMenu);

				menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));
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
					else if (button.Text == CostsComparerText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Dollar);
					}
				}

				return buttons.ToArray();
			}
		}

		void CreateAndShowNewCosting(bool isGlobal = false)
		{
			var controller = new CostingController();
			if (isGlobal)
			{
				controller.SetNewRatingHeaderAsGlobal();
			}

			controller.ShowNewForm();
		}

		#region Bulk Update

		void BulkUpdate_Click(object sender, EventArgs e)
		{
			GetNewBulkRateUpdatesController().ShowNewForm();
		}

		ZController GetNewBulkRateUpdatesController()
		{
			BulkRateUpdatesController result = (BulkRateUpdatesController)ZControllerFactory.Create(ControllerIDs.BulkRateUpdates);
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.Costing;
			return result;
		}

		#endregion

		#region Costs Comparison

		void CostsComparer_Click(object sender, EventArgs e)
		{
			GetNewCostsComparerController().ShowNewForm();
		}

		ZController GetNewCostsComparerController()
		{
			return ZControllerFactory.Create(ControllerIDs.CostsComparer);
		}

		static MultilingualString CostsComparerText => ResString.GetMultilingualString("3e6e6e74-c021-4d59-8800-92270c885512", "Comparison");

		#endregion

		#endregion

		#region XmlImport

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new CostingValueObjectDataAdapter(), DoInterfaceConnectorLicenceCheck).PromptUserAndImport(BillingInterfaceName.CostingXmlImport); // Interface name for billing purposes
		}

		#endregion
	}
}

