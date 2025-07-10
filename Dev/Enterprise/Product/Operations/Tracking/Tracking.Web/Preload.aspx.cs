using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class Preload : ZPage
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var preloadService = ObjectFactory.Get<IWebTrackerPreloadService>();

			preloadService.PreloadWebResources(DocAddressControlResource,
				SearchControlResource,
				NavigationBarResource,
				new ZWebResource(typeof(ManageLayoutsPopup), "ManageLayoutsPage.aspx", Page),
				new ZWebResource(typeof(ChangeCategoryPopup), "ChangeCategoryPage.aspx", Page),
				new ZWebResource(typeof(SaveLayoutPopup), "SaveLayoutPage.aspx", Page),
				new ZWebResource(typeof(eDocAttachPopup), "eDocAttachPage.aspx", Page),
				new ZWebResource(typeof(ZGridLayoutControl), "GridLayoutPage.aspx", Page),
				new ZWebResource(typeof(ZFindBox), "ZFilterPage.aspx", Page),
				new ZWebResource(typeof(ZFilterStripGridModule), "ZFilterStripControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips", typeof(ZFilterStripControl).Assembly),
				new ZWebResource(typeof(ZFindBox), "LocationFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location"),
				new ZWebResource(typeof(ZFindBox), "RefCountryFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location"),
				new ZWebResource(typeof(ZFindBox), "RefCommodityCodeFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Commodity"),
				new ZWebResource(typeof(ZFindBox), "RefVesselFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Vessel"),
				new ZWebResource(typeof(ZFindBox), "OrganisationFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Organisation"),
				new ZWebResource(typeof(ZFindBox), "RefServiceLevelFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.ServiceLevel"),
				new ZWebResource(typeof(ZFindBox), "SimpleFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Base"),
				new ZWebResource(typeof(ZFindBox), "RefUNLOCOFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location"),
				new ZWebResource(typeof(ZFindBox), "RefCurrencyFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Currency"),
				new ZWebResource(typeof(ZFindBox), "RefContainerFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Container"),
				new ZWebResource(typeof(Reports), "ReportFilterControl.ascx", Page, "Enterprise.Tracking.Web", typeof(ReportFilterControl).Assembly));
		}
	}
}
