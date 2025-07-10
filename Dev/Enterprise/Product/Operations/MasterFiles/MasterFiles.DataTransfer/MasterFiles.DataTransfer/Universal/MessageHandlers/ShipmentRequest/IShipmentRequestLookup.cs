using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public interface IShipmentRequestLookup
	{
		IEnumerable<BusinessObject> Match(ShipmentRequest request);
	}
}
