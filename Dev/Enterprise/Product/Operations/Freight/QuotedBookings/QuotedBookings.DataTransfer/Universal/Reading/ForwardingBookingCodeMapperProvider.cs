using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public sealed class ForwardingBookingCodeMapperProvider : IUniversalCodeMapperProvider
	{
		public IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory)
		{
			if (dataObject == null || factory == null)
			{
				return null;
			}

			if (dataObject.IsCO2eResponse())
			{
				return CO2eUniversalCodeMapper.Create(dataObject, factory);
			}

			return null;
		}
	}
}
