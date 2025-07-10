using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eManifest.Integration
{
	public interface IHVLVTransportBookingLoader
	{
		IEnumerable<IDataObject> GetInstructions(ZGuid bookingPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks, IDataWritingManager outboundSessionTracker);
		IEnumerable<IDataObject> GetPackingLines(ZGuid packingParentPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks);
		IEnumerable<IDataObject> GetShipments(ZGuid bookingPK, BusinessObjectFactory factory, Dictionary<ZGuid, ZInt> packingLineLinks, IDataWritingManager outboundSessionTracker);
		BusinessObject[] GetEventParents(IXmlEventValueObject xmlEvent, ZGuid bookingPK, BusinessObjectFactory factory, IXmlImportLogger inboundSessionTracker);
	}
}
