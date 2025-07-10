using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusRefTradeGroupControlTest : TestCaseWithFactory
	{
		public void TestTradeGroupCountryGrid()
		{
			var tradeGroup = Factory.NewWithValidTestData<CusRefTradeGroup>();
			using (var form = new ZForm(tradeGroup))
			using (var control = new CusRefTradeGroupControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tradeGroupCountryGrid = control.FindSingleOrDefault<ZGrid>("TradeGroupCountryGrid");
				AssertEquals("CRA_RN_NKTradeGroupCountryCode", typeof(ZCodeFindBoxColumnStyle), tradeGroupCountryGrid.Columns["CRA_RN_NKTradeGroupCountryCode"].ColumnStyle.GetType());
				AssertEquals("CRA_Description", typeof(ZTextBoxColumnStyle), tradeGroupCountryGrid.Columns["CRA_Description"].ColumnStyle.GetType());
				AssertEquals("CRA_StartDate", typeof(ZDateEditColumnStyle), tradeGroupCountryGrid.Columns["CRA_StartDate"].ColumnStyle.GetType());
				AssertEquals("CRA_EndDate", typeof(ZDateEditColumnStyle), tradeGroupCountryGrid.Columns["CRA_EndDate"].ColumnStyle.GetType());
			}
		}

		public void TestTradeGroupCountryCodeFindBoxColumnStyle_PopupSelected()
		{
			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = "C1";
			country1.RN_Desc = "C1 Desc";
			var country2 = Factory.New<RefCountry>();
			country2.RN_Code = "C2";
			country2.RN_Desc = "C2 Desc";
			var country3 = Factory.New<RefCountry>();
			country3.RN_Code = "C3";
			country3.RN_Desc = "C3 Desc";
			Factory.Save();
			var tradeGroup = Factory.NewWithValidTestData<CusRefTradeGroup>();
			using (var form = new ZForm(tradeGroup))
			using (var control = new CusRefTradeGroupControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tradeGroupCountryGrid = control.FindSingleOrDefault<ZGrid>("TradeGroupCountryGrid");
				tradeGroupCountryGrid.Focus();
				tradeGroupCountryGrid.GetNextControl(tradeGroupCountryGrid, false).Focus();
				var tradeGroupCountryCodeColumnStyle = tradeGroupCountryGrid.Columns["CRA_RN_NKTradeGroupCountryCode"].ColumnStyle as ZCodeFindBoxColumnStyle;
				var findBox = (ZGridFindBox)(tradeGroupCountryCodeColumnStyle.EditControl);
				var codeBox = findBox.CodeBox;
				var popupDecisionProvider = new PopupModuleDecisionProviderWithMultipleSelect(findBox);
				CombineAssertions(() =>
				{
					popupDecisionProvider.HandleFindBoxOKButton(new[] { country1, country2 });
					var collection = (CusRefTradeGroupCountryCollection)tradeGroupCountryGrid.List;
					AssertEquals("After selection", 2, collection.Count);
					AssertEquals("First selected one and show on codeBox", "C1", codeBox.Text);
					AssertArrayEqualsByElements("select C1 and C2", new ZString[] { "C1", "C2" }, collection.Select(x => x.CRA_RN_NKTradeGroupCountryCode).ToArray());
					popupDecisionProvider.HandleFindBoxOKButton(new[] { country2, country3, country1 });
					collection.RefreshBinding();
					AssertEquals("Select coutnry3 which is not duplicate", 4, collection.Count);
					AssertEquals("First one always selected regardless duplication and show on codeBox", "C2", codeBox.Text);
					AssertArrayEqualsByElements("select C3", new ZString[] { "C2", "C2", "C3", "C1" }, collection.Select(x => x.CRA_RN_NKTradeGroupCountryCode).ToArray());
					popupDecisionProvider.HandleFindBoxOKButton(new[] { country3, country2, country1 });
					collection.RefreshBinding();
					AssertEquals("Nothing new added due to duplication of country2 and country1", 4, collection.Count);
					AssertEquals("First one always selected regardless duplication and show on codeBox", "C3", codeBox.Text);
					AssertArrayEqualsByElements("C2 codeBox changed to C3", new ZString[] { "C3", "C2", "C3", "C1" }, collection.Select(x => x.CRA_RN_NKTradeGroupCountryCode).ToArray());
					popupDecisionProvider.HandleFindBoxOKButton(new[] { country1 });
					collection.RefreshBinding();
					AssertEquals("CodeBox changed to the selected one", "C1", codeBox.Text);
					AssertArrayEqualsByElements("C3 codeBox changed to C1", new ZString[] { "C1", "C2", "C3", "C1" }, collection.Select(x => x.CRA_RN_NKTradeGroupCountryCode).ToArray());
				});
			}
		}

		public void TestControl()
		{
			var testItem = Factory.NewWithValidTestData<CusRefTradeGroup>();
			using (var testControl = new CusRefTradeGroupControl())
			using (var testForm = new ZForm(testItem))
			{
				testForm.Show();
				AssertEquals("CountryCodeFindBox", "CR9_RN_NKCountryCode", GetBindingMemberByName(testControl, "CountryCodeFindBox"));
				AssertEquals("TradeGroupTextBox", "CR9_TradeGroup", GetBindingMemberByName(testControl, "TradeGroupTextBox"));
				AssertEquals("DescriptionTextBox", "CR9_Description", GetBindingMemberByName(testControl, "DescriptionTextBox"));
				AssertEquals("StartDateEdit", "CR9_StartDate", GetBindingMemberByName(testControl, "StartDateEdit"));
				AssertEquals("EndDateEdit", "CR9_EndDate", GetBindingMemberByName(testControl, "EndDateEdit"));
				AssertEquals("TradeGroupCountryGrid", "TradeGroupCountries", GetBindingMemberByName(testControl, "TradeGroupCountryGrid"));
			}
		}

		string GetBindingMemberByName(CusRefTradeGroupControl parentControl, string subControlName)
		{
			var subControl = parentControl.Controls.Find(subControlName, true)[0];
			var bindingMember = parentControl.BindingSource.GetBindingMember(subControl);
			return bindingMember;
		}
	}
}
