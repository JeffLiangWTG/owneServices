using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceNumberSequenceConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestValidateCode()
		{
			Configuration.Code = ZString.Empty;
			AssertHasError(Configuration.CodeInfo, "Please enter a value.");

			var newConfiguration = Configuration.ParentCollection.AddNew();
			newConfiguration.Code = "AAA";

			Configuration.Code = "AAA";
			AssertHasError(Configuration.CodeInfo, "This code already exists.");

			Configuration.Code = "DEF";
			AssertHasError(Configuration.CodeInfo, "The specify Code cannot be 'DEF' as this is reserved for the default format (Series Prefix + Sequence Number)");
		}

		public void TestValidateDescription()
		{
			Configuration.Description = ZString.Empty;
			AssertHasError(Configuration.DescriptionInfo, "Please enter a value.");

			Configuration.Description = "description";
			AssertNoErrors(Configuration.DescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (ConfigurationCollection == null)
			{
				ConfigurationCollection = new ComplianceNumberSequenceConfigurationCollection(Factory);
				Configuration = ConfigurationCollection.AddNew();
			}
		}

		ComplianceNumberSequenceConfiguration Configuration;
		ComplianceNumberSequenceConfigurationCollection ConfigurationCollection;
	}
}
