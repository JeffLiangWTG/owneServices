using Enterprise.ZArchitecture.Core.Test;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AssemblyDataProviderAttribute))]
	sealed class AssemblyDataProviderAttributeTest : AssemblyMetaDataAttributeTestCase<AssemblyDataProviderAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.DocManagerCode = "DocManagerCode";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DocManagerCode = "DocManagerCode";
			Assert(attribute1.Equals(attribute2));

			attribute1.Country = "Country";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Country = "Country";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
