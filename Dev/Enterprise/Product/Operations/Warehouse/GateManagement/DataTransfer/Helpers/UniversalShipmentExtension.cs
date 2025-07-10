using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public static class UniversalShipmentExtension
	{
		public static ZString? GetAdditionalReferenceOrDefault(this UniversalShipment dataObject, string code)
		{
			return dataObject.AdditionalReferenceCollection?.FirstOrDefault(x => (string)x.Type?.Code == code)?.ReferenceNumber;
		}

		public static void AddAdditionalReference(this UniversalShipment dataObject, string code, string description, string referenceNumber)
		{
			if (dataObject.AdditionalReferenceCollection == null)
			{
				dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			}

			dataObject.AdditionalReferenceCollection.Add(new AdditionalReference()
			{
				Type = new EntryType()
				{
					Code = code,
					Description = description,
				},
				ReferenceNumber = referenceNumber,
			});
		}

		public static ZString? GetAdditionalInfoOrDefault(this UniversalShipment dataObject, string key)
		{
			return dataObject.AddInfoCollection?.FirstOrDefault(x => (string)x.Key == key)?.Value;
		}

		public static OrgAddress GetOrganizationAddressForShipment(this UniversalShipment xml, IXmlImportLogger logger, UniversalObjectFactory factory, DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.TransportCompanyDocumentaryAddress:
					var addressDataObject = xml.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress));

					if (addressDataObject != null)
					{
						var orgAddress = new GateManagementOrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() as OrgAddress;

						if (orgAddress != null)
						{
							return orgAddress;
						}
						else
						{
							var errorMessage = Res.GetString("dc29463c-ab01-46a3-8d01-7d81b18b8eb5", "No matching organization found for {0}.", addressDataObject.OrganizationCode);
							logger.Log(LogType.Error, errorMessage);
							throw new DataObjectReadFailureException(errorMessage);
						}
					}
					else
					{
						var errorMessage = Res.GetString("4d915fb5-724d-4e01-bcfd-8158faedd6a5", "No {0} found.", addressType);
						logger.Log(LogType.Error, errorMessage);
						throw new DataObjectReadFailureException(errorMessage);
					}
				default:
					throw new NotImplementedException();
			}
		}
	}
}
