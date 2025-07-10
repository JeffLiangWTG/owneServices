using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FormCustomisableElementCollectionView))]
	sealed class FormCustomisableElementCollectionViewTest : BusinessObjectCollectionViewTestCase<FormCustomisableElementCollectionView>
	{
		protected override FormCustomisableElementCollectionView GetCollectionToTest()
		{
			return new FormCustomisableElementCollectionView(new FormCustomisableElementCollection());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			FormCustomisableElement element = new FormCustomisableElement();
			return element;
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject biz1 = GetNewElementToAddToTheCollection();
			BusinessObject biz2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(biz1, biz2);

			Factory.Save();

			Collection.Remove(biz1);

			AssertContainsExactElementsInAnyOrder(new[] { biz2 }, Collection);
			Assert("element 1 was only removed, but not deleted", !biz1.IsDeleted);
		}

		public override void TestTypedAddNew()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			AssertNotNull(collection.AddNew());
		}

		public void TestIsThisPartOfTheCollection()
		{
			FormCustomisableElementCollection collection = new FormCustomisableElementCollection();
			FormCustomisableElementCollectionView view = new FormCustomisableElementCollectionView(collection);

			AssertEquals(0, view.Count);

			FormCustomisableElement elem1 = collection.Add((NoResString)"desc 1", "name 1", false);

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1 }, view);

			FormCustomisableElement elem2 = collection.Add((NoResString)"desc 2", "name 2", false);
			elem2.IsInTemplate = false;

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1 }, view);

			FormCustomisableElement elem3 = collection.Add((NoResString)"desc 3", "name 3", false);
			elem3.IsAvailableFunction = () => false;

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1 }, view);

			FormCustomisableElement elem4 = collection.AddNew();
			elem4.Hydrate(new FormCustomisationSettingsStorageField());
			Assert("prerequisite", elem4.IsPersistedInDatabase);

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1, elem4 }, view);

			elem4.IsInTemplate = false;

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1 }, view);

			elem4.IsAvailableFunction = () => false;

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1 }, view);

			elem2.IsInTemplate = true;
			elem3.IsAvailableFunction = () => true;
			elem4.IsInTemplate = true;
			elem4.IsAvailableFunction = () => true;

			view.Rebuild();
			AssertContainsExactElementsInAnyOrder(new[] { elem1, elem2, elem3, elem4 }, view);
		}
	}
}
