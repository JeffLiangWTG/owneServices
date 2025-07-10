using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class IntercompanyTariffModule : RatingModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.IntercompanyTariffs;

		public override BusinessContext[] BusinessContexts => new[] { BusinessContext.Rating };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) =>
			ZControllerFactory.Create(ControllerIDs.IntercompanyTariffs);

		protected override IBusinessObjectCollection GetNewGridCollection() =>
			new IntercompanyTariffCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() =>
			new IntercompanyTariffFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() =>
			new IntercompanyTariffFilterControl(GridCollection, FilterBusinessObject);

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint =>
			Env.Security.IntercompanyTariffs;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore =>
			Env.Licence.Core;

		#endregion

		#region Action Menu / Toolbar Buttons

		protected virtual bool ShouldAddGRIUpdateMenuItem => true;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			if (ShouldAddGRIUpdateMenuItem)
			{
				MenuItem bulkUpdateMenuItem = new ZMenuItem(BulkRateCostUpdateText, new EventHandler(BulkUpdate_Click));
				MenuItem bulkUpdateSubMenu = new ZMenuItem(BulkUpdateSubMenuText, new MenuItem[] { bulkUpdateMenuItem });
				menuItemCollection.Add(new ZMenuItem("-"));
				menuItemCollection.Add(bulkUpdateSubMenu);
			}

			return menuItemCollection.ToArray();
		}

		#endregion

		#region Bulk Update

		void BulkUpdate_Click(object sender, EventArgs e)
		{
			GetNewBulkRateUpdatesController().ShowNewForm();
		}

		protected ZController GetNewBulkRateUpdatesController()
		{
			BulkRateUpdatesController result = (BulkRateUpdatesController)ZControllerFactory.Create(ControllerIDs.BulkRateUpdates);
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.IntercompanyTariff;
			return result;
		}

		#endregion
	}
}
