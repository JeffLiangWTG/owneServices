using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalInterchange = Enterprise.UniversalDataBuss.DataObjects.Universal.UniversalInterchange;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Helpers
{
	public class UniversalInterchangeHelper
	{
		public UniversalInterchangeHelper() { }

		public UniversalShipment universalShipment { get; set; }

		public string SenderID { get; set; } = string.Empty;

		public string RecipientID { get; set; } = string.Empty;

		UniversalInterchangeHeaderDeliveryMetadata MetaDataCollection { get; set; } = new UniversalInterchangeHeaderDeliveryMetadata();

		public string GetUniversalInterchangeXml()
		{
			string universalShipmentXml = XmlHelper.SerializeIDataObject(universalShipment);
			XDocument doc = XDocument.Parse(universalShipmentXml);
			doc.Declaration = null;
			var universalInterchange = new UniversalInterchange(MetaDataCollection, doc.ToString(), SenderID, RecipientID);
			var universalInterchangeXml = XmlHelper.Serialize(universalInterchange);
			return universalInterchangeXml;
		}

		public void PopulateCarrierBooking(DtbCarrierBookingConsignment dtbCarrierBookingConsignment)
		{
			DataWritingManager dataWritingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.CNR, dtbCarrierBookingConsignment));
			DtbCarrierBookingConsignmentDataObjectWriter dtbCarrierBookingDataObjectWriter = new DtbCarrierBookingConsignmentDataObjectWriter(dataWritingManager);
			universalShipment = dtbCarrierBookingDataObjectWriter.GetDataObject(dtbCarrierBookingConsignment);
			SetCarrierAccountMetaData(dtbCarrierBookingConsignment);
		}

		void SetCarrierAccountMetaData(DtbCarrierBookingConsignment dtbCarrierBookingConsignment)
		{
			if (dtbCarrierBookingConsignment.LTC_OAN_CarrierAccount != CargoWise.Types.ZGuid.Empty)
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Carrier Booking Factory" };
				var carrierAccount = factory.Load<OrgCarrierAccount>(dtbCarrierBookingConsignment.LTC_OAN_CarrierAccount);
				if(carrierAccount != null)
				{
					List<UniversalInterchangeHeaderDeliveryMetadataValue> metaDataList = new List<UniversalInterchangeHeaderDeliveryMetadataValue>();
					foreach (OrgCarrierAccountMetaData metaData in carrierAccount.MetaDataCollection)
					{
						metaDataList.Add(new UniversalInterchangeHeaderDeliveryMetadataValue()
						{
							Name = metaData.OAM_Name,
							Type = GetMetaDataType(metaData.OAM_Type),
							Data = OrgCarrierAccountMetaDataEncoder.Decrypt(metaData.PK.ToGuid(), metaData.OAM_BinaryValue)
						});
					}
					MetaDataCollection.ValueCollection = [.. metaDataList];
				}
			}
		}

		UniversalInterchangeHeaderDeliveryMetadataValueType GetMetaDataType(string metaDataType)
		{
			if(metaDataType == "BOO")
			{
				return UniversalInterchangeHeaderDeliveryMetadataValueType.Boolean;
			}
			return UniversalInterchangeHeaderDeliveryMetadataValueType.String;
		}
	}
}
