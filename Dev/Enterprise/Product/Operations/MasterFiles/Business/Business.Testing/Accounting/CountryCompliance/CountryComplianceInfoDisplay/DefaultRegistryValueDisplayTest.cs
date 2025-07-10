using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(DefaultRegistryValueDisplay))]
	sealed class DefaultRegistryValueDisplayTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DefaultRegistryValueDisplay("Caption", "DefaultValue");
		}

		public void TestDefaultRegistryValueDisplay()
		{
			string caption = "Caption";
			string defaultValue = "DefaultValue";
			var defaultRegistryValueDisplay = new DefaultRegistryValueDisplay(caption, defaultValue);

			AssertEquals("defaultRegistryValueDisplay.Caption", caption, defaultRegistryValueDisplay.Caption);
			AssertEquals("defaultRegistryValueDisplay.DefaultValue", defaultValue, defaultRegistryValueDisplay.DefaultValue);
		}
	}
}
