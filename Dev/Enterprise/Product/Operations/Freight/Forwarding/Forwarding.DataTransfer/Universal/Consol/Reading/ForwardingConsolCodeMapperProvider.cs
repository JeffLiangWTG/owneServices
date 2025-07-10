using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public sealed class ForwardingConsolCodeMapperProvider : IUniversalCodeMapperProvider
	{
		public IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory)
		{
			if (dataObject == null || factory == null)
			{
				return null;
			}

			if (dataObject.IsBookingConfirmationMessage()
				|| dataObject.IsBookingRequestMessage())
			{
				return BookingConfirmationUniversalCodeMapper.Create(dataObject, factory);
			}

			if (dataObject.IsCO2eResponse())
			{
				return CO2eUniversalCodeMapper.Create(dataObject, factory);
			}

			return null;
		}
	}
}
