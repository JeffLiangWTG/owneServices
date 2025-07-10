using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Testing
{
	internal class RegistryItemImplWithDefaultDelegateTest : TestCase
	{
		public void TestGetDelegate()
		{
			int value = 10;
			RegistryItemImplWithDefaultDelegate<int> impl = new RegistryItemImplWithDefaultDelegate<int>("", null, null, null, new IntRegistryDataType(), RegistryStorageFlags.System, RegistryOptions.Default, (c, b, d) => value);
			AssertEquals(10, impl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
			value = 15;
			AssertEquals(15, impl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
