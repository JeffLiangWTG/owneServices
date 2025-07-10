using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsShipmentDataObjectReaderProvider : ICustomsShipmentDataObjectReaderProvider
	{
		#region ICustomsShipmentDataObjectReaderProvider Members

		public ITopLevelDataObjectReader GetReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment)
		{
			IUniversalCustomsDataObjectProvider provider = null;
			var applicationCode = universalShipment.MessagingApplicationCode?.Code.GetValueOrDefault() ?? ZString.Empty;
			if (!applicationCode.IsEmpty)
			{
				provider = factory.BOFactory.GetApplicationSpecificUniversalCustomsDataObjectProvider(applicationCode);
			}

			if (provider == null)
			{
				provider = factory.BOFactory.GetUniversalCustomsDataObjectProvider(universalShipment.GetTargetCountryCode());
			}

			ITopLevelDataObjectReader reader = null;
			if (provider != null)
			{
				reader = provider.GetNewJobDeclarationDataObjectReader(universalShipment, logger, factory, forwardingShipment);
			}
			return reader ?? new JobDeclarationDataObjectReader(universalShipment, logger, factory, forwardingShipment);
		}

		#endregion
	}
}
