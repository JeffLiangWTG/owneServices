using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public abstract class BaseHVLVRelatedJobCommandTest : TestCaseWithFactory
	{
		public void TestRequirementAttributes()
		{
			var command = GetCommandForTest();

			CombineAssertions(() =>
			{
				AssertRequirementAttribute<ApplicableLoginCountryAttribute>(command,
					() => ExpectedApplicableLoginCountries != null,
					attribute => AssertContainsExactElementsInAnyOrder("Login countries", ExpectedApplicableLoginCountries, attribute.CountryCodes));

				AssertRequirementAttribute<ApplicableLoginCountryAttribute>(command,
					() => ExpectedApplicableLoginCountries != null,
					attribute => AssertContainsExactElementsInAnyOrder("Allow login to different country registry", ExpectedAllowLoginToDifferentCountryRegistry, attribute.AllowLoginToDifferentCountryRegistry ?? string.Empty));

				AssertRequirementAttribute<ShipmentDestinationCountryAttribute>(command,
					() => !string.IsNullOrEmpty(ExpectedShipmentDestinationCountry),
					attribute => AssertEquals("Shipment destination", ExpectedShipmentDestinationCountry, attribute.CountryCode));

				AssertRequirementAttribute<ShipmentTransportModeAttribute>(command,
					() => ExpectedShipmentTransportModes != null,
					attribute => AssertContainsExactElementsInAnyOrder("Transport modes", ExpectedShipmentTransportModes, attribute.TransportModes));

				AssertRequirementAttribute<ShipmentDirectionAttribute>(command,
					() => ExpectedShipmentDirections != null,
					attribute => AssertContainsExactElementsInAnyOrder("Directions", ExpectedShipmentDirections, attribute.Directions));

				AssertRequirementAttribute<RequiredRegistryItemAttribute>(command,
					() => !string.IsNullOrEmpty(ExpectedRegistryItemSetName),
					attribute => AssertContainsExactElementsInAnyOrder("Registry Item Set Name", ExpectedRegistryItemSetName, attribute.RegistryItemSetName));

				AssertRequirementAttribute<RequiredRegistryItemAttribute>(command,
					() => !string.IsNullOrEmpty(ExpectedRequiredRegistryItemName),
					attribute => AssertContainsExactElementsInAnyOrder("Required Registry Item Name", ExpectedRequiredRegistryItemName, attribute.RequiredRegistryItemName));

				AssertRequirementAttribute<RequiredFeatureControlCodeAttribute>(command,
					() => !string.IsNullOrEmpty(ExpectedRequiredFeatureControlCode),
					attribute => AssertContainsExactElementsInAnyOrder("Required Registry Item Name", ExpectedRequiredFeatureControlCode, attribute.FeatureControlCode));
			});
		}

		void AssertRequirementAttribute<TAttribute>(BaseHVLVRelatedJobCommand command, Func<bool> shouldHaveAttribute, Action<TAttribute> assertElements) where TAttribute : Attribute
		{
			var attribute = Attribute.GetCustomAttribute(command.GetType(), typeof(TAttribute)) as TAttribute;
			if (shouldHaveAttribute())
			{
				AssertNotNull(string.Format("Should have {0}", typeof(TAttribute).Name), attribute);
				assertElements?.Invoke(attribute);
			}
			else
			{
				AssertNull(string.Format("Should not have {0}", typeof(TAttribute).Name), attribute);
			}
		}

		protected virtual string ExpectedAllowLoginToDifferentCountryRegistry => string.Empty;

		protected virtual string[] ExpectedApplicableLoginCountries => null;

		protected virtual string ExpectedShipmentDestinationCountry => string.Empty;

		protected virtual string[] ExpectedShipmentTransportModes => null;

		protected virtual string ExpectedRegistryItemSetName => string.Empty;

		protected virtual string ExpectedRequiredRegistryItemName => string.Empty;

		protected virtual Directions[] ExpectedShipmentDirections => null;

		protected virtual string ExpectedRequiredFeatureControlCode => string.Empty;

		public void TestGetRelatedJobConverter()
		{
			var command = GetCommandForTest();
			AssertType(ExpectedRelatedJobConverterType, command.Converter);
		}

		protected abstract Type ExpectedRelatedJobConverterType { get; }

		public void TestShouldValidateWaybill()
		{
			var command = GetCommandForTest();
			AssertEquals(ExpectedShouldValidateWaybill, command.ShouldValidateWaybill);
		}

		protected abstract bool ExpectedShouldValidateWaybill { get; }

		protected abstract bool ExpectedShouldTrackPrimaryFieldChanges { get; }

		public void TestNeedPreScreening()
		{
			var command = GetCommandForTest();
			AssertEquals(ExpectedNeedPreScreening, command.NeedPreScreening);
		}

		protected abstract bool ExpectedNeedPreScreening { get; }

		public virtual void TestUsageCodeAndCategory()
		{
			var command = GetCommandForTest();
			AssertUsageCodeAndCategory("Expected", command, ExpectedUsageCode, ExpectedUsageCategory);
		}

		protected virtual string ExpectedUsageCode => string.Empty;

		protected virtual string ExpectedUsageCategory => string.Empty;

		protected static void AssertUsageCodeAndCategory(string message, BaseHVLVRelatedJobCommand command, string expectedUsageCode, string expectedUsageCategory)
		{
			AssertNotNullOrEmpty("Please set ExpectedUsageCode or override TestUsageCodeAndCategory()", expectedUsageCode);
			AssertNotNullOrEmpty("Please set ExpectedUsageCategory or override TestUsageCodeAndCategory()", expectedUsageCategory);

			var actualUsageCode = command.UsageCode;
			AssertEquals($"{message} usage code:", expectedUsageCode, actualUsageCode);
			AssertEquals($"{message} usage category for {actualUsageCode}:", expectedUsageCategory, UsageCategories.LookupByUsageCode.GetValueSafe(actualUsageCode));
		}

		public void TestRelatedCustomsJobs_SingleGenPivotDbHit()
		{
			var command = GetCommandForTest();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenPivotSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsForAllFactories(expectedDbHits, ignoreUnspecified: true))
			{
				_ = command.ActiveRelatedCustomsJobs.FirstOrDefault();
			}
		}

		protected abstract BaseHVLVRelatedJobCommand GetCommandForTest();

		protected abstract Type ExpectedCustomsJobsType { get; }

		public void TestControllerIsRegisteredForCustomsJobTypeOrItsBaseType()
		{
			var command = GetCommandForTest();
			var customsJobTypeInfo = command.GetType().GetProperty("RelatedCustomsJobType", BindingFlags.NonPublic | BindingFlags.Instance);
			var customsJobTypeFromCommand = customsJobTypeInfo.GetValue(command) as Type;

			Assert("Pre-condition: Expected customs job type matches the definition of command", customsJobTypeFromCommand.IsAssignableFrom(ExpectedCustomsJobsType));

			AssertNotNull(ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(ExpectedCustomsJobsType));
		}

		protected virtual string ExpectedRelatedJobName => string.Empty;

		public virtual void TestGetRelatedJobName()
		{
			var command = GetCommandForTest();
			AssertEquals(ExpectedRelatedJobName, command.RelatedJobName);
		}
	}
}
