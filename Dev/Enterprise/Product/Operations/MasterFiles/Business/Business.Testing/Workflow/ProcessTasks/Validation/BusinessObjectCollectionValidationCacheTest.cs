using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BusinessObjectCollectionValidationCacheTest : TestCaseWithFactory
	{
		ValidationRule<DummyBusinessObject> GetZ0_AnotherDateValidationRule(Action onValidate)
		{
			IEnumerable<IGrouping<AddValidationNotification<DummyBusinessObject>, DummyBusinessObject>> Z0_AnotherDateRule(IEnumerable<DummyBusinessObject> bizos)
			{
				foreach (var b in bizos)
				{
					onValidate();
				}
				return Enumerable.Empty<Grouping<AddValidationNotification<DummyBusinessObject>, DummyBusinessObject>>();
			}

			return new ValidationRule<DummyBusinessObject>(name: DummyBusinessObject.Schema.Z0_AnotherDate,
				validationPropertyName: DummyBusinessObject.Schema.Z0_AnotherDate,
				bizos => Z0_AnotherDateRule(bizos),
				(bizo, info) => onValidate(),
				isCachedForValidateAllOnly: true);
		}

		ValidationRule<DummyBusinessObject> GetZ0_DateValidationRule(Action onValidate)
		{
			IEnumerable<IGrouping<AddValidationNotification<DummyBusinessObject>, DummyBusinessObject>> Z0_DateRule(IEnumerable<DummyBusinessObject> bizos)
			{
				foreach (var b in bizos)
				{
					onValidate();
				}
				return Enumerable.Empty<Grouping<AddValidationNotification<DummyBusinessObject>, DummyBusinessObject>>();
			}

			return new ValidationRule<DummyBusinessObject>(name: DummyBusinessObject.Schema.Z0_Date,
				validationPropertyName: DummyBusinessObject.Schema.Z0_Date,
				bizos => Z0_DateRule(bizos),
				(bizo, info) => onValidate(),
				isCachedForValidateAllOnly: false);
		}

		public void TestValidationCaching()
		{
			int z0_AnotherDateValidations = 0;
			var z0_AnotherDateRule = GetZ0_AnotherDateValidationRule(() => z0_AnotherDateValidations++);

			int z0_DateValidations = 0;
			var z0_DateRule = GetZ0_DateValidationRule(() => z0_DateValidations++);

			var collection = new DummyBusinessObjectCollection(Factory);
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();

			var cache = new BusinessObjectCollectionValidationCache<DummyBusinessObject>(collection, z0_DateRule, z0_AnotherDateRule);

			cache.Validate(dummy1, dummy1.Z0_AnotherDateInfo);
			AssertEquals("This validation rule is cached during validate all only", 1, z0_AnotherDateValidations);
			cache.Validate(dummy1, dummy1.Z0_AnotherDateInfo);
			AssertEquals("Validation called again as cache is not active", 2, z0_AnotherDateValidations);
			AssertEquals("We haven't called validation on this property yet", 0, z0_DateValidations);

			cache.Validate(dummy1, dummy1.Z0_DateInfo);
			AssertEquals("This validation ruls is always cached so we should validate the whole collection", 2, z0_AnotherDateValidations);
			AssertEquals("Expecting no change", 2, z0_AnotherDateValidations);
			cache.Validate(dummy2, dummy2.Z0_DateInfo);
			AssertEquals("No more calls as result is cached", 2, z0_AnotherDateValidations);

			z0_AnotherDateValidations = z0_DateValidations = 0;

			cache.Invalidate(DummyBusinessObject.Schema.Z0_Date);
			using (cache.SetCacheForValidateAll())
			{
				cache.Validate(dummy1, dummy1.Z0_AnotherDateInfo);
				AssertEquals("Cache is active so should validate all", 2, z0_AnotherDateValidations);
				AssertEquals("We haven't called validation on this property yet", 0, z0_DateValidations);
				cache.Validate(dummy1, dummy1.Z0_DateInfo);
				AssertEquals("Cache is active so should validate all", 2, z0_DateValidations);
				AssertEquals("Expecting no change", 2, z0_DateValidations);
			}

			z0_AnotherDateValidations = 0;
			cache.Validate(dummy1, dummy1.Z0_AnotherDateInfo);
			AssertEquals("Cache should be reset after ValidateAll", 1, z0_AnotherDateValidations);
		}
	}
}
