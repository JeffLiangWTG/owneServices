using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportConsolidationDataObjectReader<T> : ShipmentDataObjectReader<T>
		where T : DtbTransportConsolidation
	{
		protected DtbTransportConsolidationDataObjectReader(UniversalShipment consolidationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consolidationDataObject, logger, factory)
		{
		}

		protected override bool IsImportConsolCostsAllowed(T targetBO)
		{
			return true;
		}

		#region PopulateAddresses

		protected void PopulateAddresses(DtbTransportConsolidation consolidation)
		{
			var organisationCollection = dataObject.OrganizationAddressCollection;
			if (organisationCollection != null)
			{
				foreach (var orgAddressDataObject in organisationCollection)
				{
					var jobDocAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(consolidation);
					if (jobDocAddress != null)
					{
						consolidation.DocAddresses.Add(jobDocAddress);
					}
				}
			}
		}

		#endregion
	}
}
