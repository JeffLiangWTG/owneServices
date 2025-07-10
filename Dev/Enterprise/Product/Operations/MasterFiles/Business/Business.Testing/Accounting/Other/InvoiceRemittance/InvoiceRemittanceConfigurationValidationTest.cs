using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.InvoiceRemittanceCustomisationElement;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceRemittanceConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestValidateCode()
		{
			Configuration.Code = ZString.Empty;
			AssertHasError(Configuration.CodeInfo, "Please enter a value.");

			var newConfiguration = Configuration.ParentCollection.AddNew();
			newConfiguration.Code = "AAA";

			Configuration.Code = "AAA";
			AssertHasError(Configuration.CodeInfo, "This code already exists.");
		}

		public void TestValidateDescription()
		{
			Configuration.Description = ZString.Empty;
			AssertHasError(Configuration.DescriptionInfo, "Please enter a value.");

			Configuration.Description = "description";
			AssertNoErrors(Configuration.DescriptionInfo);
		}

		public void TestValidateMaxPossibleLength()
		{
			Configuration.Elements[ElementNames.CustomCode1].Include = true;
			Configuration.Elements[ElementNames.CustomCode1].DigitCode = "1234";

			Configuration.MaxPossibleLength = 3;
			AssertHasError(Configuration.MaxPossibleLengthInfo, "This calculated reference number length is greater than Max Possible Length.");

			Configuration.MaxPossibleLength = 121;
			AssertHasError(Configuration.MaxPossibleLengthInfo, "Max Possible Length cannot greater than 120.");
		}

		public void TestValidateDebtorLocation()
		{
			Configuration.DebtorLocation = ZString.Empty;
			AssertHasError(Configuration.DebtorLocationInfo, "Please enter a value.");

			Configuration.DebtorLocation = "XXX";
			AssertHasError(Configuration.DebtorLocationInfo, "Enter a valid selection.");

			Configuration.DebtorLocation = "ALL";
			AssertNoErrors(Configuration.DebtorLocationInfo);

			var newConfiguration = Configuration.ParentCollection.AddNew();
			newConfiguration.DebtorLocation = "ALL";
			AssertHasError(newConfiguration.DebtorLocationInfo, "This Debtor Location already exists.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (ConfigurationCollection == null)
			{
				ConfigurationCollection = new InvoiceRemittanceConfigurationCollection(Factory);
				Configuration = ConfigurationCollection.AddNew();
			}
		}

		InvoiceRemittanceConfiguration Configuration;
		InvoiceRemittanceConfigurationCollection ConfigurationCollection;
	}
}
