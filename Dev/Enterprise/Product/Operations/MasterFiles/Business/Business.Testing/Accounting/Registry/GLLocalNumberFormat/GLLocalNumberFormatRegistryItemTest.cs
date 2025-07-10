using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLLocalNumberFormatRegistryItem))]
	sealed class GLLocalNumberFormatRegistryItemTest : StronglyTypedRegistryItemTestCase<GLLocalNumberFormatCollection>
	{
		protected override StronglyTypedRegistryItem<GLLocalNumberFormatCollection, GLLocalNumberFormatCollection> GetNewRegistryItem()
		{
			return new GLLocalNumberFormatRegistryItem("", null, null, null, RegistryOptions.Default);
		}
	}
}
