using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FormCustomisableElementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePlacement_UniquePlacement()
		{
			FormCustomisationSettings settings = new FormCustomisationSettings(Factory.New<ProcessTaskTemplate>());
			settings.DisplayTabs.Add((NoResString)"Tab 1", "Tab1", true);
			settings.DisplayTabs.Add((NoResString)"Tab 2", "Tab2", true);

			FormCustomisableElement element1 = settings.DisplayFields.Add((NoResString)"Element 1", "1", false, (NoResString)"", "Tab1", TabPlacement.Placements.BottomLeft, 0);
			FormCustomisableElement element2 = settings.DisplayFields.Add((NoResString)"Element 2", "2", false, (NoResString)"", "Tab1", TabPlacement.Placements.BottomRight, 0);
			AssertNoErrors(element1.PlacementLocalizedInfo);
			AssertNoErrors(element2.PlacementLocalizedInfo);

			element2.Placement = TabPlacement.Placements.BottomLeft;
			AssertHasErrors(element1.PlacementLocalizedInfo);
			AssertHasErrors(element2.PlacementLocalizedInfo);

			element1.Placement = TabPlacement.Placements.BottomRight;
			AssertNoErrors("Same tab, same position - error", element1.PlacementLocalizedInfo);
			AssertNoErrors("Same tab, same position - error", element2.PlacementLocalizedInfo);

			element2.DisplayTabCode = "Tab2";
			element2.Placement = TabPlacement.Placements.BottomRight;
			AssertNoErrors("Differnet tab so no conflict", element1.PlacementLocalizedInfo);
			AssertNoErrors("Differnet tab so no conflict", element2.PlacementLocalizedInfo);
		}

		public void TestValidateDisplayTab()
		{
			FormCustomisationSettings settings = new FormCustomisationSettings(Factory.New<ProcessTaskTemplate>());
			settings.DisplayTabs.Add((NoResString)"Tab 1", string.Empty, true, (NoResString)"Tab1");
			settings.DisplayTabs.Add((NoResString)"Tab 2", string.Empty, true, (NoResString)"Tab2");
			settings.DisplayTabs.Add((NoResString)"中文", string.Empty, true, (NoResString)"Tab2");

			FormCustomisableElement element = settings.DisplayFields.Add((NoResString)"Blah", "Blah", false, (NoResString)string.Empty, "Tab 1", TabPlacement.Placements.BottomLeft);
			AssertNoErrors(element.DisplayTabLocalizedInfo);

			element.DisplayTab = "Tab 9";
			AssertHasErrors(element.DisplayTabLocalizedInfo);

			element.DisplayTab = "Tab 2";
			AssertNoErrors(element.DisplayTabLocalizedInfo);

			element.DisplayTab = "中文";
			AssertNoErrors(element.DisplayTabLocalizedInfo);
		}

		public void TestValidateRowNumber()
		{
			FormCustomisationSettings settings = new FormCustomisationSettings(Factory.New<ProcessTaskTemplate>());
			settings.DisplayTabs.Add((NoResString)"Tab 1", string.Empty, true, (NoResString)"Tab1");

			FormCustomisableElement element1 = settings.DisplayFields.Add((NoResString)"Element 1", "1", false, (NoResString)"Group1", "Tab1", TabPlacement.Placements.BottomLeft, 1);
			FormCustomisableElement element2 = settings.DisplayFields.Add((NoResString)"Element 2", "2", false, (NoResString)"Group1", "Tab1", TabPlacement.Placements.BottomRight, 2);
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element2.RowNumber = 1;
			AssertHasErrors(element1.RowNumberInfo);
			AssertHasErrors(element2.RowNumberInfo);

			element1.RowNumber = 3;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element1.RowNumber = 0;
			element2.RowNumber = 0;
			AssertHasErrors(element1.RowNumberInfo);
			AssertHasErrors(element2.RowNumberInfo);

			element1.IsInTemplate = true;
			element2.IsInTemplate = false;
			element1.RowNumber = 0;
			element2.RowNumber = 0;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element2.IsInTemplate = true;
			element2.Visible = false;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element2.Visible = true;
			element1.Visible = false;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element1.Visible = true;
			element2.Visible = true;
			element1.RowNumber = 1;
			element2.RowNumber = 1;
			AssertHasErrors(element1.RowNumberInfo);
			AssertHasErrors(element2.RowNumberInfo);

			element2.IsAvailableFunction = () => false;
			element1.RowNumber = 1;
			element2.RowNumber = 1;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);

			element1.RowNumber = -1;
			element2.RowNumber = 0;
			AssertHasErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);
		}

		public void TestWithoutValidateRowNumberUniqueForSystemTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_IsSystem = true;
			var settings = new FormCustomisationSettings(template);
			settings.DisplayTabs.Add((NoResString)"Tab 1", string.Empty, true, (NoResString)"Tab1");

			var element1 = settings.DisplayFields.Add((NoResString)"Element 1", "1", false, (NoResString)"Group1", "Tab1", TabPlacement.Placements.BottomLeft, 1);
			var element2 = settings.DisplayFields.Add((NoResString)"Element 2", "2", false, (NoResString)"Group1", "Tab1", TabPlacement.Placements.BottomRight, 2);

			element1.Visible = true;
			element2.Visible = true;
			element1.RowNumber = 1;
			element2.RowNumber = 1;
			AssertNoErrors(element1.RowNumberInfo);
			AssertNoErrors(element2.RowNumberInfo);
		}

		public void TestValidateIsInTemplate()
		{
			FormCustomisationSettings settings = new FormCustomisationSettings(Factory.New<ProcessTaskTemplate>());
			FormCustomisableElement element = settings.DisplayFields.Add((NoResString)"Element 1", "1", false, (NoResString)"", "Tab1", TabPlacement.Placements.BottomLeft);
			AssertNoWarnings(element.IsInTemplateInfo);

			element.Hydrate(new FormCustomisationSettingsStorageField());
			element.Visible = true;
			element.IsInTemplate = false;
			AssertHasWarning(element.IsInTemplateInfo, "This element is not valid for this workflow type and its visibility setting will be disregarded");

			element.IsInTemplate = true;
			AssertNoWarnings(element.IsInTemplateInfo);
		}

		public void TestValidateElementDescriptionAndElementGroup()
		{
			FormCustomisationSettings settings = new FormCustomisationSettings(Factory.New<ProcessTaskTemplate>());
			settings.DisplayTabs.Add((NoResString)"Tab 1", string.Empty, true, (NoResString)"Tab1");

			FormCustomisableElement element = settings.DisplayFields.Add((NoResString)"Blah", "Blah", false, (NoResString)"", "Tab 1", TabPlacement.Placements.BottomLeft);
			AssertNoErrors(element.DisplayTabInfo);

			element.ElementDescription = "中文说明";
			element.ElementGroup = "中文分组";
			AssertNoErrors(element.ElementDescriptionInfo);
			AssertNoErrors(element.ElementGroupInfo);
		}
	}
}
