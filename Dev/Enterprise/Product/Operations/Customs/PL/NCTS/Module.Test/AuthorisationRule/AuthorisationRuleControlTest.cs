using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.PL.NCTS.Module.Testing;

sealed class AuthorisationRuleControlTest : ZFilterStripControlTest
{
	public void TestRuleCodeColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle(nameof(CusAuthorisationRule.CPR_RuleCode));
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 114, column.Width);
			});
		}
	}

	public void TestValueColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle(nameof(CusAuthorisationRule.CPR_ValueFrom));
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 150, column.Width);
			});
		}
	}

	public void TestDescriptionColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle(nameof(CusAuthorisationRule.CPR_Description));
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 150, column.Width);
			});
		}
	}

	public void TestAuthorisationTypeColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle("AuthorisationHeader+CPH_Type");
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 150, column.Width);
			});
		}
	}

	public void TestAuthorisationNumberColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle("AuthorisationHeader+CPH_Number");
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 150, column.Width);
			});
		}
	}

	public void TestAuthorisationHolderColumn()
	{
		using (var form = new ZForm())
		using (var filterControl = GetNewFilterControl())
		{
			form.Controls.Add(filterControl);
			form.Show();

			var column = filterControl.Grid.GetColumnStyle("AuthorisationHeader+CPH_OH_PermitHolder");
			CombineAssertions(() =>
			{
				AssertType<ZGuidFindBoxColumnStyleInfo>("Column type is correct", column);
				AssertEquals("Column is visible", true, column.IsVisible);
				AssertEquals("Width", 150, column.Width);
			});
		}
	}

	AuthorisationRuleControl GetNewFilterControl()
	{
		var collection = new Business.CusAuthorisationRuleCollection(Factory);
		var filterBizO = new AuthorisationRuleFilterBusinessObject();
		return new AuthorisationRuleControl(collection, filterBizO);
	}
}
