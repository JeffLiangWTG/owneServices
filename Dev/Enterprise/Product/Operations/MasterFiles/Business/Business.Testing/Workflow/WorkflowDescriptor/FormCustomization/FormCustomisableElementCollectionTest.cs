using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FormCustomisableElementCollection))]
	sealed class FormCustomisableElementCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FormCustomisableElementCollection>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199", Justification = "Testing Multilingual behavior")]
		public void TestAddSetsProperties()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("5E48313F-205C-4CDA-BDA2-A749DDCE82CC", new ResourceStringData("5E48313F-205C-4CDA-BDA2-A749DDCE82CC", "描述"));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
					FormCustomisableElement element = collection.Add(ResString.GetMultilingualString("5E48313F-205C-4CDA-BDA2-A749DDCE82CC", "Description"), "Name", true, (NoResString)"Group", "Tab1", "Nowhere", 4, true);
					AssertEquals("Description", element.ElementDescription);
					AssertEquals("描述", element.ElementDescriptionMultilingual);
					AssertEquals("Name", element.ElementName);
					AssertEquals("Group", element.ElementGroup);
					AssertEquals(true, element.CanContainOtherElements);
					AssertEquals("Tab1", element.DisplayTabCode);
					AssertEquals("Nowhere", element.Placement);
					AssertEquals(4, element.RowNumber);
					AssertEquals(true, element.Visible);
					AssertEquals(collection, element.ParentElements);

					FormCustomisableElement newElement = collection.AddNew();
					AssertEquals(collection, newElement.ParentElements);
				}
			}
		}

		public void TestAddSetsPropertiesForNonVisibleElements()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			FormCustomisableElement notVisibleElement = collection.Add((NoResString)"Pasha Bulka", "Name", true, (NoResString)"Group1", "Tab1", "Anywhere", 99, false);
			AssertEquals("Pasha Bulka", notVisibleElement.ElementDescription);
			AssertEquals("Name", notVisibleElement.ElementName);
			AssertEquals("Group1", notVisibleElement.ElementGroup);
			AssertEquals(true, notVisibleElement.CanContainOtherElements);
			AssertEquals("Tab1", notVisibleElement.DisplayTabCode);
			AssertEquals(false, notVisibleElement.Visible);
			AssertEquals("Placement should be reset to default when element is not visisble", string.Empty, notVisibleElement.Placement);
			AssertEquals("Row number should be reset to default when element is not visisble", 0, notVisibleElement.RowNumber);
			AssertEquals(collection, notVisibleElement.ParentElements);

			FormCustomisableElement newElement = collection.AddNew();
			AssertEquals(collection, newElement.ParentElements);
		}

		public void TestIsElementVisible()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			FormCustomisableElement elem1 = collection.Add((NoResString)"Zasha1", "Name1");
			FormCustomisableElement elem2 = collection.Add((NoResString)"Zasha2", "Name2", false, (NoResString)"Group");
			elem2.DisplayTabCode = "XXX";
			elem2.Placement = elem2.PlacementList[0].Code;

			elem1.Visible = false;
			AssertEquals(false, collection.IsElementVisible("Name1"));

			elem1.Visible = true;
			AssertEquals(true, collection.IsElementVisible("Name1"));

			elem1.IsInTemplate = false;
			AssertEquals(false, collection.IsElementVisible("Name1"));

			elem1.IsInTemplate = true;
			elem1.IsAvailableFunction = () => false;
			AssertEquals(false, collection.IsElementVisible("Name1"));

			AssertEquals(true, collection.IsElementVisible("Group"));

			elem2.Visible = false;
			AssertEquals(false, collection.IsElementVisible("Group"));

			AssertNull(collection.IsElementVisible("Unknown"));
		}

		public void TestGetTabPlacement()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			FormCustomisableElement element = collection.Add((NoResString)"Zasha", "Name", true, (NoResString)"Group1", "Tab1", "Nowhere", 4);
			TabPlacement placement = collection.GetTabPlacement("Name");
			AssertEquals("Tab1", placement.TabPageName);
			AssertEquals("Nowhere", placement.Placement);
			AssertEquals(4, placement.RowNumber);

			AssertEquals(new TabPlacement(), collection.GetTabPlacement("Unknown"));
		}

		public void TestGetTabPlacementByGroupReturnsFirstNonEmptyElement()
		{
			FormCustomisableElementCollection collection =
				new FormCustomisableElementCollection
				{
					{ (NoResString)"Description1", "Name1", true, (NoResString)"Group1", "", "", 0 },
					{ (NoResString)"Description2", "Name2", true, (NoResString)"Group1", "Tab1", "Nowhere", 4 }
				};

			TabPlacement placement = collection.GetTabPlacement("Group1");
			AssertEquals("Tab1", placement.TabPageName);
			AssertEquals("Nowhere", placement.Placement);
			AssertEquals(4, placement.RowNumber);
		}

		public void TestAllowNew()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestHydrateTabs()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();

			FormCustomisationSettingsStorageTabCollection tabs = null;
			AssertNoExceptionThrown(() => collection.Hydrate(tabs));

			tabs = new FormCustomisationSettingsStorageTabCollection();
			tabs.AddNew().Name = "AAA";
			tabs.AddNew().Name = "BBB";
			tabs.AddNew().Name = "CCC";

			collection.Hydrate(tabs);
			AssertEquals(3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, collection.Cast<FormCustomisableElement>().Select(elem => (string)elem.ElementName));
		}

		public void TestDeHydrateTabs()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			collection.AddNew().ElementName = "AAA";
			collection.AddNew().ElementName = "BBB";
			collection.AddNew().ElementName = "CCC";

			FormCustomisationSettingsStorageTabCollection tabs = null;
			AssertNoExceptionThrown(() => collection.DeHydrate(tabs));
			tabs = new FormCustomisationSettingsStorageTabCollection();

			collection.DeHydrate(tabs);
			AssertEquals(3, tabs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, tabs.Cast<FormCustomisationSettingsStorageTab>().Select(tab => (string)tab.Name));
		}

		public void TestHydrateFields()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();

			FormCustomisationSettingsStorageFieldCollection fields = null;
			AssertNoExceptionThrown(() => collection.Hydrate(fields));

			fields = new FormCustomisationSettingsStorageFieldCollection();
			fields.AddNew().Name = "AAA";
			fields.AddNew().Name = "BBB";
			fields.AddNew().Name = "CCC";

			collection.Hydrate(fields);
			AssertEquals(3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, collection.Cast<FormCustomisableElement>().Select(elem => (string)elem.ElementName));
		}

		public void TestDeHydrateFields()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			collection.AddNew().ElementName = "AAA";
			collection.AddNew().ElementName = "BBB";
			collection.AddNew().ElementName = "CCC";

			FormCustomisationSettingsStorageFieldCollection fields = null;
			AssertNoExceptionThrown(() => collection.DeHydrate(fields));
			fields = new FormCustomisationSettingsStorageFieldCollection();

			collection.DeHydrate(fields);
			AssertEquals(3, fields.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, fields.Cast<FormCustomisationSettingsStorageField>().Select(tab => (string)tab.Name));
		}

		#region Implementation

		protected override FormCustomisableElementCollection GetCollectionToTest()
		{
			return new FormCustomisableElementCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FormCustomisableElement();
		}

		#endregion
	}
}
