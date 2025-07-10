using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceConfigurationRegistryDataType))]
	sealed class InvoiceRemittanceConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceRemittanceConfigurationRegistryDataType>
	{
		public void TestValidationForElements()
		{
			var dataType = new InvoiceRemittanceConfigurationRegistryDataType();
			var registryItem = new InvoiceRemittanceConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.MustOverrideDefaultValue);

			var collection = new InvoiceRemittanceConfigurationCollection(new BusinessObjectFactory());
			var configuration = collection.AddNew();
			configuration.Code = "AAA";
			configuration.Description = "Test for AAA";
			configuration.DebtorLocation = "ALL";
			AssertExceptionThrown(typeof(RegistryValidationException), "Configuration Code AAA : There should be at least one element is included.", () => dataType.Validate(registryItem, collection, Guid.Empty, Guid.Empty, Guid.Empty));

			var elements = configuration.Elements;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1].Include = true;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1].DigitCode = "1";
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1].Order = 5;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1].CheckDigitAlgorithm = CheckDigitAlgorithm.Algorithm731;
			AssertExceptionThrown(typeof(RegistryValidationException), string.Format("Configuration Code AAA : Element {0} is included but not used in Check Digit.", InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1), () => dataType.Validate(registryItem, collection, Guid.Empty, Guid.Empty, Guid.Empty));

			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].Include = true;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].DigitCode = "8";
			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].Order = 7;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].CheckDigit = InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1;
			AssertExceptionThrown(typeof(RegistryValidationException), string.Format("Configuration Code AAA : Element {0} should be after the elements which use it.", InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1), () => dataType.Validate(registryItem, collection, Guid.Empty, Guid.Empty, Guid.Empty));

			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].Order = 3;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.DebtorClientNumber].Include = true;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.DebtorClientNumber].DigitCode = "8";
			elements[InvoiceRemittanceCustomisationElement.ElementNames.DebtorClientNumber].Order = 4;
			AssertExceptionThrown(typeof(RegistryValidationException), string.Format("Configuration Code AAA : Element {0} order list should not contain elements without check digit.", InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1), () => dataType.Validate(registryItem, collection, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region Implementation

		protected override InvoiceRemittanceConfigurationRegistryDataType GetNewDataType()
		{
			return new InvoiceRemittanceConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InvoiceRemittanceConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new InvoiceRemittanceConfigurationCollection(new BusinessObjectFactory());
			var configuration = collection.AddNew();
			configuration.Code = "AAA";
			configuration.Description = "Test for AAA";
			configuration.DebtorLocation = "ALL";
			var elements = configuration.Elements;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1].Include = true;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1].DigitCode = "XX";
			elements[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1].Order = 1;

			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].Include = true;
			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].DigitCode = "8";
			elements[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference].Order = 2;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}

		#endregion
	}
}
