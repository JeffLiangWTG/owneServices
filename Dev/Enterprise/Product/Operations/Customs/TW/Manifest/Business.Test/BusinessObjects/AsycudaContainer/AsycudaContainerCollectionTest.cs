using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerCollection))]
	sealed class AsycudaContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaContainerCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Containers;
		}
	}
}
