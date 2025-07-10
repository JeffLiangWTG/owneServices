using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FormCustomisableElement))]
	sealed class FormCustomisableElementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			AssertEquals(true, element.Visible);
			AssertEquals(true, element.IsInTemplate);
			AssertEquals(true, element.IsAvailable);
		}

		public void TestIsAvailableFunction()
		{
			var element = new FormCustomisableElement();
			int i = 0;
			element.IsAvailableFunction = () => i > 0;
			Assert(!element.IsAvailable);
			i++;
			Assert(element.IsAvailable);
		}

		public void TestElementNameDescReadOnly()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			AssertEquals(true, element.ElementNameInfo.ReadOnly);
			AssertEquals(true, element.ElementDescriptionInfo.ReadOnly);
			AssertEquals(true, element.ElementDescriptionMultilingualInfo.ReadOnly);
		}

		public void TestElementGroup()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 1", "Tab1");
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 2", "Tab2");

			FormCustomisableElement element1 = template.FormCustomisationSettings.DisplayFields.AddNew();
			FormCustomisableElement element2 = template.FormCustomisationSettings.DisplayFields.AddNew();
			FormCustomisableElement element3 = template.FormCustomisationSettings.DisplayFields.AddNew();

			element1.ElementGroup = "Group1";
			element2.ElementGroup = "Group1";
			element3.ElementGroup = "Group1";

			element1.Placement = "Place";
			AssertEquals("Place", element2.Placement);
			AssertEquals("Place", element3.Placement);

			element2.DisplayTabCode = "Tab1";
			AssertEquals("Tab1", element1.DisplayTabCode);
			AssertEquals("Tab1", element3.DisplayTabCode);

			element2.Visible = false;
			element3.Placement = "Place2";
			AssertEquals("", element2.Placement);
			AssertEquals("Place2", element1.Placement);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199", Justification = "Testing Multilingual behavior")]
		public void TestMultilingualProperties()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("63EFE275-82B8-43A0-B491-9BBB92B6F441", new ResourceStringData("63EFE275-82B8-43A0-B491-9BBB92B6F441", "说明1"));
				mockChs.Put("7539BCE6-787A-46B3-99AB-43A217762C8D", new ResourceStringData("7539BCE6-787A-46B3-99AB-43A217762C8D", "页面1"));
				mockChs.Put("987B8017-BCA8-4164-9510-B2FD7B5EFD8A", new ResourceStringData("987B8017-BCA8-4164-9510-B2FD7B5EFD8A", "页面2"));
				mockChs.Put("261f8e5b-52b6-4987-bb31-8300d879d261", new ResourceStringData("261f8e5b-52b6-4987-bb31-8300d879d261", "底部靠左"));
				mockChs.Put("21ab6981-88f2-43f4-9bbb-3f323f8a944b", new ResourceStringData("21ab6981-88f2-43f4-9bbb-3f323f8a944b", "顶部居中"));

				ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
				template.FormCustomisationSettings.DisplayTabs.Add(ResString.GetMultilingualString("7539BCE6-787A-46B3-99AB-43A217762C8D", "Tab 1"), "Tab1", canContainOtherElements: true);
				template.FormCustomisationSettings.DisplayTabs.Add(ResString.GetMultilingualString("987B8017-BCA8-4164-9510-B2FD7B5EFD8A", "Tab 2"), "Tab2", canContainOtherElements: true);

				FormCustomisableElement element = template.FormCustomisationSettings.DisplayFields.AddNew();
				element.ElementDescriptionMultilingual = ResString.GetMultilingualString("63EFE275-82B8-43A0-B491-9BBB92B6F441", "Desc1");
				element.ElementDescription = "Desc1";
				element.ElementGroupMultilingual = ResString.GetMultilingualString("63EFE275-82B8-43A0-B491-9BBB92B6F441", "Desc1");
				element.ElementGroup = "Desc1";
				element.Placement = TabPlacement.Placements.BottomLeft;
				element.DisplayTab = "Tab 1";

				AssertEquals("Desc1", element.ElementDescriptionMultilingual);
				AssertEquals("Desc1", element.ElementGroupMultilingual);

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("页面1", element.DisplayTabLocalized);
					AssertEquals("底部靠左", element.PlacementLocalized);

					AssertEquals("说明1", element.ElementDescriptionMultilingual);
					AssertEquals("Desc1", element.ElementDescription);

					AssertEquals("说明1", element.ElementGroupMultilingual);
					AssertEquals("Desc1", element.ElementGroup);

					element.DisplayTabLocalized = "页面2";
					AssertEquals("Tab 2", element.DisplayTab);
					element.DisplayTabLocalized = "Invalid Value";
					AssertEquals("Invalid Value", element.DisplayTab);

					element.PlacementLocalized = "顶部居中";
					element.Placement = TabPlacement.Placements.TopMiddle;
					element.PlacementLocalized = "Somewhere Invalid";
					element.Placement = "Somewhere Invalid";
				}
			}
		}

		public void TestVisible()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			element.RowNumber = 3;
			element.Placement = "X";
			element.DisplayTab = "Y";
			element.ElementGroup = "Group1";
			AssertEquals(false, element.PlacementInfo.ReadOnly);
			AssertEquals(false, element.RowNumberInfo.ReadOnly);
			AssertEquals(false, element.DisplayTabInfo.ReadOnly);

			element.Visible = false;
			AssertEquals(0, element.RowNumber);
			AssertEquals("", element.Placement);
			AssertEquals("", element.DisplayTab);
			AssertEquals(true, element.PlacementInfo.ReadOnly);
			AssertEquals(true, element.RowNumberInfo.ReadOnly);
			AssertEquals(true, element.DisplayTabInfo.ReadOnly);
		}

		public void TestDisplayTab()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 1", "1");
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 2", "2");

			FormCustomisableElement element = template.FormCustomisationSettings.DisplayFields.AddNew();

			element.DisplayTabCode = "1";
			AssertEquals("Tab 1", element.DisplayTab);

			element.DisplayTabCode = "2";
			AssertEquals("Tab 2", element.DisplayTab);

			element.DisplayTab = "Tab 1";
			AssertEquals("1", element.DisplayTabCode);
		}

		public void TestDisplayTabList()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 1", "Tab1");
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 2", "Tab2").CanContainOtherElements = true;
			template.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 3", "Tab3").CanContainOtherElements = true;

			FormCustomisableElement element = template.FormCustomisationSettings.DisplayFields.AddNew();
			AssertEquals(2, element.DisplayTabList.Count);
			AssertEquals("Tab 2", element.DisplayTabList[0].Code);
			AssertEquals("Tab 3", element.DisplayTabList[1].Code);
		}

		public void TestHydrateTab()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			AssertEquals("prerequisite", ZString.Empty, element.ElementDescription);
			AssertEquals("prerequisite", ZString.Empty, element.ElementName);
			AssertEquals("prerequisite", true, element.Visible);
			AssertEquals("prerequisite", false, element.IsPersistedInDatabase);

			FormCustomisationSettingsStorageTab tab = new FormCustomisationSettingsStorageTab { Description = "DESCRIPTION", Name = "NAME", Visible = false };

			element.Hydrate(tab);

			AssertEquals("DESCRIPTION", element.ElementDescription);
			AssertEquals("NAME", element.ElementName);
			AssertEquals(false, element.Visible);
			AssertEquals(true, element.IsPersistedInDatabase);
		}

		public void TestDeHydrateTab()
		{
			FormCustomisationSettingsStorageTab tab = new FormCustomisationSettingsStorageTab();
			AssertEquals("prerequisite", ZString.Empty, tab.Description);
			AssertEquals("prerequisite", ZString.Empty, tab.Name);
			AssertEquals("prerequisite", false, tab.Visible);

			FormCustomisableElement element = new FormCustomisableElement { ElementDescription = "DESCRIPTION", ElementName = "NAME", Visible = true };

			element.DeHydrate(tab);

			AssertEquals("DESCRIPTION", tab.Description);
			AssertEquals("NAME", tab.Name);
			AssertEquals(true, tab.Visible);
		}

		public void TestHydrateField()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			AssertEquals("prerequisite", ZString.Empty, element.ElementDescription);
			AssertEquals("prerequisite", ZString.Empty, element.ElementName);
			AssertEquals("prerequisite", ZString.Empty, element.ElementGroup);
			AssertEquals("prerequisite", ZString.Empty, element.DisplayTab);
			AssertEquals("prerequisite", ZString.Empty, element.Placement);
			AssertEquals("prerequisite", 0, element.RowNumber);
			AssertEquals("prerequisite", true, element.Visible);
			AssertEquals("prerequisite", false, element.IsPersistedInDatabase);

			FormCustomisationSettingsStorageField field = new FormCustomisationSettingsStorageField
			{
				Description = "DESCRIPTION",
				Name = "NAME",
				Group = "GROUP",
				Visible = false,
				TabName = "TAB",
				Placement = "DUPA",
				Position = 69
			};

			element.Hydrate(field);

			AssertEquals("DESCRIPTION", element.ElementDescription);
			AssertEquals("NAME", element.ElementName);
			AssertEquals("GROUP", element.ElementGroup);
			AssertEquals(false, element.Visible);
			AssertEquals("TAB", element.DisplayTab);
			AssertEquals("DUPA", element.Placement);
			AssertEquals(69, element.RowNumber);
			AssertEquals(true, element.IsPersistedInDatabase);
		}

		public void TestDeHydrateField()
		{
			FormCustomisationSettingsStorageField field = new FormCustomisationSettingsStorageField();
			AssertEquals("prerequisite", ZString.Empty, field.Description);
			AssertEquals("prerequisite", ZString.Empty, field.Name);
			AssertEquals("prerequisite", ZString.Empty, field.Group);
			AssertEquals("prerequisite", false, field.Visible);
			AssertEquals("prerequisite", ZString.Empty, field.TabName);
			AssertEquals("prerequisite", ZString.Empty, field.Placement);
			AssertEquals("prerequisite", 0, field.Position);

			FormCustomisableElement element = new FormCustomisableElement
			{
				ElementDescription = "DESCRIPTION",
				ElementName = "NAME",
				ElementGroup = "GROUP",
				Visible = true,
				DisplayTab = "TAB",
				Placement = "DUPA",
				RowNumber = 69
			};

			element.DeHydrate(field);

			AssertEquals("DESCRIPTION", field.Description);
			AssertEquals("NAME", field.Name);
			AssertEquals("GROUP", field.Group);
			AssertEquals(true, field.Visible);
			AssertEquals("TAB", field.TabName);
			AssertEquals("DUPA", field.Placement);
			AssertEquals(69, field.Position);
		}

		public void TestReadOnly()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			AssertEquals("prerequisite", false, element.IsPersistedInDatabase);
			AssertEquals("prerequisite", true, element.IsInTemplate);
			AssertEquals(false, element.ReadOnly);

			element.Hydrate(new FormCustomisationSettingsStorageField());
			AssertEquals("prerequisite", true, element.IsPersistedInDatabase);
			AssertEquals(false, element.ReadOnly);

			element.IsInTemplate = false;
			AssertEquals(true, element.ReadOnly);
		}

		public void TestCopyValues()
		{
			FormCustomisableElement element1 = new FormCustomisableElement
			{
				ElementName = "NAME",
				ElementDescription = "DESCRIPTION",
				ElementGroup = "GROUP",
				CanContainOtherElements = true,
				DisplayTabCode = "TAB",
				Placement = "DUPA",
				RowNumber = 69,
				Visible = true
			};

			FormCustomisableElement element2 = new FormCustomisableElement();
			AssertEquals("prerequisite", false, element2.HasChanges);
			AssertEquals("prerequisite", ZString.Empty, element2.ElementName);
			AssertEquals("prerequisite", ZString.Empty, element2.ElementDescription);
			AssertEquals("prerequisite", ZString.Empty, element2.ElementGroup);
			AssertEquals("prerequisite", false, element2.CanContainOtherElements);
			AssertEquals("prerequisite", ZString.Empty, element2.DisplayTabCode);
			AssertEquals("prerequisite", ZString.Empty, element2.Placement);
			AssertEquals("prerequisite", true, element2.Visible);
			AssertEquals("prerequisite", 0, element2.RowNumber);

			element2.CopyValues(element1);

			AssertEquals(false, element2.HasChanges);
			AssertEquals("NAME", element2.ElementName);
			AssertEquals("DESCRIPTION", element2.ElementDescription);
			AssertEquals("GROUP", element2.ElementGroup);
			AssertEquals(true, element2.CanContainOtherElements);
			AssertEquals("TAB", element2.DisplayTabCode);
			AssertEquals("DUPA", element2.Placement);
			AssertEquals(true, element2.Visible);
			AssertEquals(69, element2.RowNumber);
		}

		public void TestCopyValuesForNonVisibleElement()
		{
			FormCustomisableElement originalElement = new FormCustomisableElement
			{
				ElementName = "ANOTHER NAME",
				ElementDescription = "ANOTHER DESCRIPTION",
				ElementGroup = "ANOTHER GROUP",
				CanContainOtherElements = false,
				DisplayTabCode = "ANOTHER TAB",
				Placement = "Anywhere",
				RowNumber = 99,
				Visible = false
			};

			FormCustomisableElement copiedElement = new FormCustomisableElement();
			copiedElement.CopyValues(originalElement);

			AssertEquals(false, copiedElement.HasChanges);
			AssertEquals("ANOTHER NAME", copiedElement.ElementName);
			AssertEquals("ANOTHER DESCRIPTION", copiedElement.ElementDescription);
			AssertEquals("ANOTHER GROUP", copiedElement.ElementGroup);
			AssertEquals(false, copiedElement.CanContainOtherElements);
			AssertEquals("ANOTHER TAB", copiedElement.DisplayTabCode);
			AssertEquals(false, copiedElement.Visible);
			AssertEquals("Placement should be reset to default when element is not visisble", "", copiedElement.Placement);
			AssertEquals("Row number should be reset to default when element is not visisble", 0, copiedElement.RowNumber);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
			return template.FormCustomisationSettings.DisplayFields.AddNew();
		}

		#endregion
	}
}
