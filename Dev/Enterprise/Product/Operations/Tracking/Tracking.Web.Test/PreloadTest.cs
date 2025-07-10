using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class PreloadTest : TestCaseWithFactory
	{
		public void TestResourcesPreloaded()
		{
			var mockPreloadService = new Mock<IWebTrackerPreloadService>();
			mockPreloadService.Setup(x => x.PreloadWebResources(It.IsAny<ZWebResource>()));
			ObjectFactory.Substitute(mockPreloadService.Object);

			var page = new PreloadForTest();
			page.OnLoadForTest();

			mockPreloadService.Verify(x => x.PreloadWebResources(It.Is<ZWebResource[]>(y =>
				y.Any(z => z.FileName.Contains("ZDocAddressControl.ascx")) &&
				y.Any(z => z.FileName.Contains("SearchControl.ascx")) &&
				y.Any(z => z.FileName.Contains("ZNavigationBar.ascx")) &&
				y.Any(z => z.FileName.Contains("ManageLayoutsPage.aspx")) &&
				y.Any(z => z.FileName.Contains("ChangeCategoryPage.aspx")) &&
				y.Any(z => z.FileName.Contains("SaveLayoutPage.aspx")) &&
				y.Any(z => z.FileName.Contains("eDocAttachPage.aspx")) &&
				y.Any(z => z.FileName.Contains("GridLayoutPage.aspx")) &&
				y.Any(z => z.FileName.Contains("ZFilterPage.aspx")) &&
				y.Any(z => z.FileName.Contains("ZFilterStripControl.ascx")) &&
				y.Any(z => z.FileName.Contains("LocationFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefCountryFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefCommodityCodeFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefVesselFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("OrganisationFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefServiceLevelFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("SimpleFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefUNLOCOFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefCurrencyFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("RefContainerFilterControl.ascx")) &&
				y.Any(z => z.FileName.Contains("ReportFilterControl.ascx")
			))));
			Assert("This is not an empty test", true);
		}
	}
}
