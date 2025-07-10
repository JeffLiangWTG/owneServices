using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobAirSailingModule : JobSailingModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobAirSailing; }
		}

		protected override ControllerID ControllerID => ControllerIDs.JobAirSailing;

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobAirSailingFilterControl(GridCollection, (JobAirSailingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobAirSailingFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.FlightSchedule; }
		}

		#endregion

		#region Bulk Copy

		protected override void AddCopyMenuItem(List<MenuItem> menu)
		{
			MenuItem copyMenuItem = new ZMenuItem(CopyMenuItemText, delegate
			{ });
			copyMenuItem.MenuItems.Add(new ZMenuItem(SimpleCopyMenuItemText, HandleTemplateCopyClick));
			copyMenuItem.MenuItems.Add(new ZMenuItem(BulkCopyMenuItemText, BulkCopyClick));
			menu.Add(copyMenuItem);
		}

		internal void BulkCopyClick(object sender, EventArgs e)
		{
			BaseJobSailing sailing = CurrentBusinessObjectInGrid as BaseJobSailing;

			if (Grid.ListManager.Position >= 0 && sailing != null)
			{
				using (BulkCopyCriteria bulkCopyCriteria = (BulkCopyCriteria)Activator.CreateInstance(
					ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingBulkCopyCriteria>(), new object[] { sailing.PK }))
				{
					ZFormModaliser.ShowDialogAndDispose(new BulkScheduleCopyForm(bulkCopyCriteria));
				}

				Grid.Refresh();
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected MultilingualString CopyMenuItemText { get { return ResString.GetMultilingualString("Freight|JobAirSailingModule|CopyMenuItem", "&Copy"); } }
		protected MultilingualString SimpleCopyMenuItemText { get { return ResString.GetMultilingualString("Freight|JobAirSailingModule|SingleCopyMenuItem", "&Single Copy"); } }
		protected MultilingualString BulkCopyMenuItemText { get { return ResString.GetMultilingualString("Freight|JobAirSailingModule|BulkCopyMenuItem", "&Bulk Copy"); } }

		#endregion

		#region For Testing
#if DEBUG

		public IBusinessObjectCollection GridCollection_ExposedForTest
		{
			get { return GridCollection; }
		}

		public ZDisplayGrid Grid_ExposedForTest
		{
			get { return Grid; }
		}

#endif
		#endregion
	}
}
