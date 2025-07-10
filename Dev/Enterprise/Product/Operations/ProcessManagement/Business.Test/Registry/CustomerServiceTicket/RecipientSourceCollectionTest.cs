using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(RecipientSourceCollection))]
	class RecipientSourceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RecipientSourceCollection>
	{
		public void TestAddNew_ViaIBindingListInterface_ShouldValidateCorrectlyAndSetSequence()
		{
			var collection = GetCollectionToTest();
			var bindingList = (IBindingList)collection;
			var item1 = (RecipientSource)bindingList.AddNew();
			var item2 = (RecipientSource)bindingList.AddNew();

			AssertEquals(1, item1.FallbackSequence);
			AssertEquals(2, item2.FallbackSequence);

			item1.SourceType = RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup;
			item2.SourceType = RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup;

			// Validation should work for rows added through this interface because it's how the GUI creates rows before they are added to the collection when the row is 'committed' in the grid.
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(item2.SourceTypeInfo, isExpectingError: true);

			item2.SourceType = RecipientSourceTypeList.Codes.LastCompletedTaskResource;
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(item2.SourceTypeInfo, isExpectingError: false);
		}

		public void TestAddNew_ShouldMakeSequenceLargerThanMax_ExceptWhenThatWillOverflow()
		{
			var collection = GetCollectionToTest();
			var item1 = collection.AddNew();

			item1.FallbackSequence = 68;

			var item2 = collection.AddNew();
			AssertEquals(69, item2.FallbackSequence);

			item2.FallbackSequence = int.MaxValue;

			var item3 = collection.AddNew();
			AssertEquals(int.MaxValue, item2.FallbackSequence);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RecipientSourceCollection GetCollectionToTest()
		{
			return new RecipientSourceFallbackHeader().SourceCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RecipientSource();
		}
	}
}
