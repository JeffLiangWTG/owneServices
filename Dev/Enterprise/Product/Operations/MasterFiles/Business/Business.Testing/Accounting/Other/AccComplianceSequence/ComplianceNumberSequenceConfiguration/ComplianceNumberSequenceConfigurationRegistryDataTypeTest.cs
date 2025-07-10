using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceConfigurationRegistryDataType))]
	sealed class ComplianceNumberSequenceConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceNumberSequenceConfigurationRegistryDataType>
	{
		public void TestValidationForElements()
		{
			var dataType = new ComplianceNumberSequenceConfigurationRegistryDataType();
			var registryItem = new ComplianceNumberSequenceConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.MustOverrideDefaultValue);

			var collection = new ComplianceNumberSequenceConfigurationCollection(new BusinessObjectFactory());
			var configuration = collection.AddNew();
			configuration.Code = "AAA";
			configuration.Description = "Test for AAA";
			configuration.Elements[ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber].Include = false;
			AssertExceptionThrown(typeof(RegistryValidationException), "Configuration Code AAA : There should be at least one element is included.", () => dataType.Validate(registryItem, collection, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region Implementation

		protected override ComplianceNumberSequenceConfigurationRegistryDataType GetNewDataType()
		{
			return new ComplianceNumberSequenceConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceNumberSequenceConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ComplianceNumberSequenceConfigurationCollection(new BusinessObjectFactory());
			var configuration = collection.AddNew();
			configuration.Code = "AAA";
			configuration.Description = "Test for AAA";
			var elements = configuration.Elements;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include = true;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCode = "XX";
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Order = 1;

			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement2].Include = true;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement2].DigitCode = "8";
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement2].Order = 2;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}

		#endregion
	}
}
