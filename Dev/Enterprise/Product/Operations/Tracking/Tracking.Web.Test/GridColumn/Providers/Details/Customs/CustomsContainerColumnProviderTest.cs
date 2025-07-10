using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsContainerColumnProvider))]
	[HttpContextEnabledTest]
	sealed class CustomsContainerColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZTextEditColumn("Container #", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_ContainerNumber) { ColumnKey = WebTracker.Grids.CustomsContainer.ContainerNumber });
			AddDefaultsColumn(new ZTextEditColumn("Seal #", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_Seal) { ColumnKey = WebTracker.Grids.CustomsContainer.SealNumber });

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((BaseCusContainer)null).JobContainer.Consol.JK_BookingReference);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
			AddDefaultsColumn(new ZTextEditColumn("Booking Ref", "JobContainer.Consol.JK_BookingReference") { ColumnKey = WebTracker.Grids.CustomsContainer.BookingReference });
			AddDefaultsColumn(new ZFindBoxColumn("Type", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_RC, "Lookups.ContainerTypeCollection") { ColumnKey = WebTracker.Grids.CustomsContainer.Type });
			AddDefaultsColumn(new ZTextEditColumn("Mode", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_FCL_LCL_AIR) { ColumnKey = WebTracker.Grids.CustomsContainer.Mode });

			if (LoggedSiteUser != null && !LoggedSiteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Weight", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_Weight) { ColumnKey = WebTracker.Grids.CustomsContainer.Weight });
				AddDefaultsColumn(new ZTextEditColumn("Units", Enterprise.Customs.Business.BaseCusContainer.Schema.CO_WeightUQ) { ColumnKey = WebTracker.Grids.CustomsContainer.WeightUnits });
			}
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.CustomsContainer.Type
		};

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsContainerColumnProvider(LoggedSiteUser);
		}
	}
}
