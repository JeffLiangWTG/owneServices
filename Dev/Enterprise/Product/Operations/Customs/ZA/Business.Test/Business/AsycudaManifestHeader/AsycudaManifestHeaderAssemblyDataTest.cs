using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaManifestHeaderAssemblyDataTest : TestCaseWithFactory
	{
		public void TestOverrides()
		{
			var data = new AsycudaManifestHeaderAssemblyDataForTest();
			AssertEquals("BusinessObjectType", typeof(AsycudaManifestHeader), data.BusinessObjectType);
			AssertEquals("ReferenceType", Constants.ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
			AssertEquals("CollectionType", typeof(AsycudaManifestHeaderCollection), data.CollectionTypeExposed);
			AssertEquals("GetBusinessObjectCollection", typeof(AsycudaManifestHeaderCollection), data.GetBusinessObjectCollection(Factory).GetType());
		}

		class AsycudaManifestHeaderAssemblyDataForTest : AsycudaManifestHeaderAssemblyData
		{
			public Type CollectionTypeExposed => CollectionType;
		}
	}
}
