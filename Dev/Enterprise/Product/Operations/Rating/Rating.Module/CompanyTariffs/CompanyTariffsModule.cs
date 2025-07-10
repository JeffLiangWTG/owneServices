using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
	public class CompanyTariffsModule : RatingModule
	{
		const bool DoInterfaceConnectorLicenceCheck = false;

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.GlobalRates;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlobalRates);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CompanyTariffsFilterControl(GridCollection, (CompanyTariffsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CompanyTariffCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CompanyTariffsFilterBusinessObject();
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode;

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CompanyTariffRates;

		#endregion

		#region XmlImport

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new CompanyTariffValueObjectDataAdapter(), DoInterfaceConnectorLicenceCheck).PromptUserAndImport(BillingInterfaceName.GloblRatesXmlImport); // Interface name for billing purposes
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Action Menu / Toolbar Buttons

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var newCompanyTariffMenuItemName = ResString.GetMultilingualString("3772c6e1-7dd4-49b1-aad6-1be1bfabafb2", "New Company Tariff (Default)");
				var newCompanyTariffMenuItem = new ZMenuItem(newCompanyTariffMenuItemName, (s, e) => CreateAndShowNewTariff());
				newCompanyTariffMenuItem.DefaultItem = true;
				newCompanyTariffMenuItem.Name = newCompanyTariffMenuItemName;

				NewMenuItem.MenuItems.Add(newCompanyTariffMenuItem);

				var newGlobalTariffMenuItemName = ResString.GetMultilingualString("66ae70dc-8d8b-4cfb-b4d7-5e8e6e02a738", "New Global Tariff");
				var newGlobalTariffMenuItem = new ZMenuItem(newGlobalTariffMenuItemName, (s, e) => CreateAndShowNewTariff(true));
				newGlobalTariffMenuItem.Name = newGlobalTariffMenuItemName;

				NewMenuItem.MenuItems.Add(newGlobalTariffMenuItem);
			}

			return result;
		}

		protected virtual bool ShouldAddGRIUpdateMenuItem => true;

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click), DoInterfaceConnectorLicenceCheck);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			if (ShouldAddGRIUpdateMenuItem)
			{
				menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));

				MenuItem bulkUpdateMenuItem = new ZMenuItem(BulkUpdateText, new EventHandler(BulkUpdate_Click));
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
				}

				return buttons.ToArray();
			}
		}

		void CreateAndShowNewTariff(bool isGlobal = false)
		{
			var controller = new CompanyTariffsController();
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
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.Tariff;
			return result;
		}

		static MultilingualString BulkUpdateText => ResString.GetMultilingualString("9f421f3a-8b83-4b68-aae4-02647e463bff", "Bulk Company Tariff Update");

		#endregion

		#endregion
	}
}

