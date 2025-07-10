#if DEBUG

using System;

namespace Enterprise.Packing.Business.Testing
{
	public interface IPackingDataTransferTestHelper
	{
		IDisposable MockUniversalShipmentGenerationForPackage(DummyWithPacking[] packingParents, Func<PkgPackage, string> getUniversalXML);
		IDisposable MockUniversalShipmentGenerationForPackage(DummyWithPacking packingParent, string universalXML);
		IDisposable MockUniversalShipmentGenerationFailure(DummyWithPacking[] packingParents, Func<PkgPackage, Exception> getException);
		IDisposable MockUniversalShipmentGenerationFailure(DummyWithPacking packingParent, Exception ex);
	}
}

#endif
