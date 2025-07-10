using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsContainerColumnProvider : GridColumnProvider
	{
		public CustomsContainerColumnProvider(TrackingSiteUser siteUser)
		{
			this.siteUser = siteUser;
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsRequired(new ZTextEditColumn(Res.GetString("7ea72aed-4fa1-4910-a008-5d0653ae6504", "Container #"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_ContainerNumber) { ColumnKey = WebTracker.Grids.CustomsContainer.ContainerNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("8a8b628a-c7c9-43cc-a63f-4555884c7512", "Seal #"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_Seal) { ColumnKey = WebTracker.Grids.CustomsContainer.SealNumber });

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((BaseCusContainer)null).JobContainer.Consol.JK_BookingReference);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("be454f1f-f86f-4e8d-8dc8-deb69e978a00", "Booking Ref"), "JobContainer.Consol.JK_BookingReference") { ColumnKey = WebTracker.Grids.CustomsContainer.BookingReference });
			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("ffb96c3e-40aa-482e-a5ee-c20259719725", "Type"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_RC, "Lookups.ContainerTypeCollection") { ColumnKey = WebTracker.Grids.CustomsContainer.Type });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a269e4a7-2b93-4d2a-8053-162381cc5a7e", "Mode"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_FCL_LCL_AIR) { ColumnKey = WebTracker.Grids.CustomsContainer.Mode });
			if (SiteUser != null && !SiteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c29cf062-ae75-489c-8a54-757087a95c7c", "Weight"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_Weight) { ColumnKey = WebTracker.Grids.CustomsContainer.Weight });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6fef9032-e456-4b41-a7d2-7d9643b2cdd6", "Units"), Enterprise.Customs.Business.BaseCusContainer.Schema.CO_WeightUQ) { ColumnKey = WebTracker.Grids.CustomsContainer.WeightUnits });
			}
		}

		protected TrackingSiteUser SiteUser
		{
			get { return siteUser; }
		}

		readonly TrackingSiteUser siteUser;
	}
}
