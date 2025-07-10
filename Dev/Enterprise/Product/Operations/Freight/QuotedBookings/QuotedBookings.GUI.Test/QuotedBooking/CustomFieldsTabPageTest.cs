using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class CustomFieldsTabPageTest : TestCaseWithFactory
	{
		public void TestSetVisibilityOverrideRule()
		{
			using (CustomFieldsTabPage tabPage = new CustomFieldsTabPage())
			{
				bool? shouldTabBeVisible = true;
				Func<bool?> visiblilityRule = () => shouldTabBeVisible;
				tabPage.SetVisibilityRule(visiblilityRule);
				ITabVisibilityOverride tabVisibilityOverride = tabPage;
				AssertEquals("tab should be visible", true, tabVisibilityOverride.IsTabVisible);
				shouldTabBeVisible = false;
				AssertEquals("tab should no be visible", false, tabVisibilityOverride.IsTabVisible);
				shouldTabBeVisible = null;
				AssertEquals("we don't know if tab should be visible or not", null, tabVisibilityOverride.IsTabVisible);
			}
		}

		public void TestNothingSetupMessageLabelText()
		{
			using (CustomFieldsTabPage tabPage = new CustomFieldsTabPage())
			using (ProcessTemplateCustomFieldsControl customFieldsControl = (ProcessTemplateCustomFieldsControl)tabPage.Controls.Find("processTemplateCustomFieldsControl", true)[0])
			{
				AssertEquals("To make use of this tab, please setup quoted booking custom fields in Workflow Manager.", customFieldsControl.NothingSetupMessageLabelText);
			}
		}
	}
}
