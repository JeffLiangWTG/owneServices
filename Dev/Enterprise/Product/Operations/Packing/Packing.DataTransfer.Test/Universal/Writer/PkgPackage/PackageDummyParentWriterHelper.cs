using System;
using CargoWise.Application;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	static class PackageDummyParentWriterHelper
	{
		public static IDisposable MockDummyWriter(DummyHandle mockHandle, DummyWithPacking packingParent)
		{
			return MockDummyWriter(mockHandle, new[] { packingParent });
		}

		public static IDisposable MockDummyWriter(DummyHandle mockHandle, DummyWithPacking[] packingParents)
		{
			var mockedPackageExporterDictionary = new KeyTypeDictionaryObject { { nameof(ParentJobType.Dummy), mockHandle } };
			Array.ForEach(packingParents, p => p.ParentJobType = ParentJobType.Dummy); // set Parent Type so we get the Mocked Writer.

			return ObjectFactory.Substitute("UniversalPackingExporters", mockedPackageExporterDictionary);
		}
	}
}
