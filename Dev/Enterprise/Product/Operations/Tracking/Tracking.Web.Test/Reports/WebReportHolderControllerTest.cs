using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WebReportHolderControllerTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestControllerUpdatesModes()
		{
			WebReportHolder holderForTest = new WebReportHolder(Factory, TestSiteUser);
			WebReportHolderController controllerForTest = new WebReportHolderController(holderForTest);
			controllerForTest.UpdateBindingMembers(WebReportModes.Freight);
			AssertEquals(holderForTest.Modes.Count, 2);
			Assert(holderForTest.Modes.Contains(nameof(ModuleId.FreightReport)));
			Assert(holderForTest.Modes.Contains(nameof(ModuleId.OrdersReport)));
			controllerForTest.UpdateBindingMembers(WebReportModes.Customs);
			AssertEquals(holderForTest.Modes.Count, 1);
			Assert(holderForTest.Modes.Contains(nameof(ModuleId.CustomsReport)));
			controllerForTest.UpdateBindingMembers(WebReportModes.Warehouse);
			AssertEquals(holderForTest.Modes.Count, 1);
			Assert(holderForTest.Modes.Contains(nameof(ModuleId.WhsReport)));
			controllerForTest.UpdateBindingMembers(WebReportModes.All);
			AssertEquals(holderForTest.Modes.Count, 0);
		}

		public void TestReportsCaption()
		{
			WebReportHolder holderForTest = new WebReportHolder(Factory, TestSiteUser);
			WebReportHolderController contollerForTest = new WebReportHolderController(holderForTest);

			AssertEquals("Reports", contollerForTest.ReportsCaption(WebReportModes.All));
			AssertEquals("Forwarding Reports", contollerForTest.ReportsCaption(WebReportModes.Freight));
			AssertEquals("Liner & Agency Reports", contollerForTest.ReportsCaption(WebReportModes.LinerAgency));
			AssertEquals("Customs Reports", contollerForTest.ReportsCaption(WebReportModes.Customs));
			AssertEquals("Warehouse Reports", contollerForTest.ReportsCaption(WebReportModes.Warehouse));
			AssertEquals("Transport Reports", contollerForTest.ReportsCaption(WebReportModes.Transport));
		}

		public void TestReportMode()
		{
			WebReportHolder holderForTest = new WebReportHolder(Factory, TestSiteUser);
			WebReportHolderController contollerForTest = new WebReportHolderController(holderForTest);

			AssertEquals(WebReportModes.All, contollerForTest.ReportMode(null));
			AssertEquals(WebReportModes.All, contollerForTest.ReportMode(""));
			AssertEquals(WebReportModes.All, contollerForTest.ReportMode("All"));
			AssertEquals(WebReportModes.All, contollerForTest.ReportMode("Whatever"));

			AssertEquals(WebReportModes.Customs, contollerForTest.ReportMode("Customs"));
			AssertEquals(WebReportModes.Freight, contollerForTest.ReportMode("Freight"));
			AssertEquals(WebReportModes.LinerAgency, contollerForTest.ReportMode("LinerAgency"));
			AssertEquals(WebReportModes.Transport, contollerForTest.ReportMode("Transport"));
			AssertEquals(WebReportModes.Warehouse, contollerForTest.ReportMode("Warehouse"));
		}

		OrgContactWebUser TestSiteUser
		{
			get
			{
				if (fTestSiteUser == null)
				{
					ZWebTestHelper helper = new ZWebTestHelper(Factory);
					fTestSiteUser = helper.TestSiteUser as OrgContactWebUser;
					helper.TestContact.OC_Email = "test@cargowise.com";
					helper.TestContact.SetHashedPassword("test");
					helper.TestContact.OC_WebAccessEnabled = true;
					helper.TestContact.Factory.Save();

					fTestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
					if (WebEnv.AppInstance is DummyHttpApplication dummyHttpApplication)
					{
						dummyHttpApplication.SetSiteUser(fTestSiteUser);
					}
				}
				return fTestSiteUser;
			}
		}
		OrgContactWebUser fTestSiteUser;
	}
}
