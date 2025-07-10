using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AESEncryptionKey128RegistryItem))]
	sealed class AESEncryptionKey128RegistryItemTest : StronglyTypedRegistryItemTestCase<String>
	{
		protected override StronglyTypedRegistryItem<String, String> GetNewRegistryItem()
		{
			return new AESEncryptionKey128RegistryItem(String.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
