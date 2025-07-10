using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeDependencyConfiguration))]
	sealed class ComplianceSubTypeDependencyConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestChildSubTypeAlreadyExists()
		{
			ComplianceSubTypeDependencyConfiguration newConfig = configurations.AddNew();
			newConfig.Country = "TW";
			newConfig.ParentSubType = "TXC";
			newConfig.ChildSubType = "TDI";
			AssertHasError(newConfig.ChildSubTypeInfo, "The selected child subtype already exists in the configuration.");

			newConfig.ChildSubType = "TSX";
			AssertNoErrors(newConfig.ChildSubTypeInfo);
		}

		public void TestChildSameAsParentSubType()
		{
			ComplianceSubTypeDependencyConfiguration newConfig = configurations.AddNew();
			newConfig.Country = "TW";
			newConfig.ParentSubType = "TSX";
			newConfig.ChildSubType = "TSX";
			AssertHasError(newConfig.ChildSubTypeInfo, "Child subtype can not be the same as the parent subtype.");

			newConfig.ChildSubType = "TSD";
			AssertNoErrors(newConfig.ChildSubTypeInfo);
		}

		public void TestCheckCyclicDependencyExist()
		{
			ComplianceSubTypeDependencyConfiguration newConfig = configurations.AddNew();
			newConfig.Country = "TW";
			newConfig.ChildSubType = "TXI";
			newConfig.ParentSubType = "TXC";
			AssertNoErrors("No cyclic dependency yet", newConfig.ParentSubTypeInfo);

			newConfig = configurations.AddNew();
			newConfig.Country = "TW";
			newConfig.ChildSubType = "TXC";
			newConfig.ParentSubType = "TDI";
			AssertHasError(newConfig.ParentSubTypeInfo, "Cyclic dependency detected along the relationship chain.");

			newConfig.ParentSubType = "TXS";
			AssertNoErrors("No cyclic dependency yet", newConfig.ParentSubTypeInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ComplianceSubTypeDependencyConfiguration item = new ComplianceSubTypeDependencyConfiguration();
			item.Country = "TW";
			item.ChildSubType = "TDI";
			item.ParentSubType = "TXI";
			return item;
		}

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ComplianceSubTypeDependencyConfiguration();
			configuration.Country = "TW";
			configuration.ChildSubType = "TDI";
			configuration.ParentSubType = "TXI";
			configurations = new ComplianceSubTypeDependencyConfigurationCollection();
			configurations.Add(configuration);
		}

		ComplianceSubTypeDependencyConfiguration configuration;
		ComplianceSubTypeDependencyConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
