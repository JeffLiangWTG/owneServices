using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayController))]
	sealed class CountryStatesGlbHolidayControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.CountryStatesGlbHoliday, new CountryStatesGlbHolidayController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new CountryStatesGlbHolidayController();
			AssertEquals("For New", Env.Security.CountryStatesGlbHolidayNew, controller.CheckPointForNewExposedForTest);
			AssertEquals("For View", Env.Security.CountryStatesGlbHolidayView, controller.CheckPointForViewExposedForTest);
			AssertEquals("For Edit", Env.Security.CountryStatesGlbHolidayModify, controller.CheckPointForEditExposedForTest);
			AssertEquals("For Delete", Env.Security.CountryStatesGlbHolidayDelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_ParentID = country.PK;
			glbHoliday.GH_ParentTableCode = country.TablePrefix;
			Factory.Save();
			var holidayBizo = new CountryStatesGlbHolidayBizo(Factory, glbHoliday);
			return holidayBizo;
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CountryStatesGlbHoliday;
		}

		#endregion
	}
}
