using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class PackingDataTransferTestHelper : IPackingDataTransferTestHelper
	{
		public IDisposable MockUniversalShipmentGenerationForPackage(DummyWithPacking packingParent, string universalXML)
		{
			return MockUniversalShipmentGenerationForPackage(new[] { packingParent }, p => universalXML);
		}

		public IDisposable MockUniversalShipmentGenerationForPackage(DummyWithPacking[] packingParents, Func<PkgPackage, string> getUniversalXML)
		{
			var dummyHandle = new DummyHandle(p =>
			{
				var packageUniversalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(getUniversalXML(p))))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(packageUniversalShipment, stream, new DummyLogger());
				}

				return packageUniversalShipment;
			});

			return PackageDummyParentWriterHelper.MockDummyWriter(dummyHandle, packingParents);
		}

		public IDisposable MockUniversalShipmentGenerationFailure(DummyWithPacking packingParent, Exception ex)
		{
			return MockUniversalShipmentGenerationFailure(new[] { packingParent }, p => ex);
		}

		public IDisposable MockUniversalShipmentGenerationFailure(DummyWithPacking[] packingParents, Func<PkgPackage, Exception> getException)
		{
			var mockHandle = new DummyHandle(p => throw getException(p));
			return PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, packingParents);
		}
	}
}
