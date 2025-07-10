using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class CompanyRestrictedGridFindBoxTest : TestCaseWithFactory
	{
		public void TestShowEditOrViewForm()
		{
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_GC = ZGuid.Empty;
			var sales = Factory.New<OrgSales>();
			var entitySales = EntitySalesWrapper.Get(sales, opp);

			using (var findBox = new CompanySpecificCurrencyFindBox())
			{
				findBox.CompanyColumnName = "CompanyForExchangeRate";
				findBox.SetDataBinding(entitySales, "TotalRevenueCurrencyCode");

				AssertEquals("Precondition", GlbCompany.CurrentCompany, findBox.Company);
				((IFindBoxUserControl)findBox).ShowEditOrViewForm();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				var anotherCompany = Factory.New<GlbCompany>();
				anotherCompany.GC_Code = "STL";
				anotherCompany.GC_Name = "SmartTech Local";
				opp.P8_GC = anotherCompany.PK;
				((IFindBoxUserControl)findBox).ShowEditOrViewForm();
				AssertEquals("The currency exchange rates shown here are for your currently logged in company. Please keep in mind that the used exchange rates will be those entered in SmartTech Local (STL).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
