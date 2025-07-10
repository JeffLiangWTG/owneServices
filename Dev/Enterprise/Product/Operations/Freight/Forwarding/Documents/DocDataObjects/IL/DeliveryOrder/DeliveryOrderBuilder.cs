using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class DeliveryOrderBuilder
	{
		public DeliveryOrderBuilder(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		public DeliveryOrderDocDataObject Build()
		{
			var context = new CommonContext(shipment.Factory);
			var isNotAir = shipment.TransportMode != Constants.TransportModes.Air;
			var isNotRoad = shipment.TransportMode != Constants.TransportModes.Road;

			var dataObject = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			dataObject.DeliveryOrderNumber = shipment.JS_DLO;

			dataObject.ManifestNumberInfo.AddMessageErrorIfEmpty(Res.GetString("CC1B45D1-E1CD-403F-951A-0EF008B6D394", "Manifest Number must be provided"));

			dataObject.DealNumberInfo.AddMessageErrorIfEmpty(Res.GetString("82D4E965-A009-4580-A6D9-0D2AB8FF489F", "Deal Number must be provided"));

			dataObject.ForwarderVatInfo.AddMessageErrorIfEmpty(Res.GetString("58802970-CE82-49FA-AD4D-E9C1AF650F72", "Forwarder Vat must be provided"));

			dataObject.CustomsBrokerVatInfo.AddMessageErrorIfEmpty(Res.GetString("F32C1D60-992A-4985-9C36-B7054D253008", "Customs Broker Vat must be provided"));

			var consol = shipment.LocalConsol;
			if (consol != null)
			{
				var receivingForwarder = consol.ReceivingForwarderAddress;
				if (receivingForwarder != null)
				{
					dataObject.Forwarder = AddressBuilder.Create(context, receivingForwarder);
					dataObject.ForwarderVat = receivingForwarder.Header.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Constants.CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode);
				}

				dataObject.ManifestNumber = shipment.TransportMode.ToString() switch
				{
					Constants.TransportModes.Road => shipment.Numbers.Cast<CusEntryNumber>().FirstOrDefault(s => s.CE_EntryType == IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber)?.CE_EntryNum ?? ZString.Empty,
					Constants.TransportModes.Sea => consol.Transports.ArrivalTransport?.JW_ArrivalPortRouteId ?? ZString.Empty,
					_ => ZString.Empty
				};
			}

			var importBroker = shipment.ImportBroker;
			if (importBroker != null)
			{
				dataObject.CustomsBroker = AddressBuilder.Create(context, importBroker.MainAddress);
				dataObject.CustomsBrokerVat = importBroker.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Constants.CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode);
			}

			BuildDeliverySite(dataObject);

			BuildCargoIdentifierType(dataObject);

			BuildReceiverType(dataObject);

			dataObject.DealNumber = shipment.TransportMode.ToString() switch
			{
				Constants.TransportModes.Sea => shipment.Numbers.Cast<CusEntryNumber>().FirstOrDefault(s => s.CE_EntryType == IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber)?.CE_EntryNum ?? ZString.Empty,
				_ => ZString.Empty
			};

			dataObject.ValidateAll();

			return dataObject;
		}

		void BuildDeliverySite(DeliveryOrderDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(this.shipment.Factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
			dataObject.DeliverySite = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.DeliverySite).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("A106C87E-A9F5-47D3-A076-C22DC41054DB", "Delivery Site must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.DeliverySite.Code), Res.GetString("CA143CC4-E7D6-4247-98E2-56B6D0D0C450", "Delivery Site should be selected from the list"));

			if (shipment.ImportReleaseDepot is OrgAddress importReleaseDepot && importReleaseDepot.Header is OrgHeader orgHeaderImportReleaseDepot)
			{
				dataObject.DeliverySite.Code = orgHeaderImportReleaseDepot.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Constants.CountryCodes.Israel);
			}

			if (dataObject.DeliverySite.Code.IsEmpty)
			{
				dataObject.DeliverySite.Code = shipment.JS_RL_NKDestination;
			}

			((CodeDescription)dataObject.DeliverySite).ValidateAll();
		}

		void BuildCargoIdentifierType(DeliveryOrderDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(this.shipment.Factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, ZDateTime.Today);
			dataObject.CargoIdentifierType = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.CargoIdentifierType).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("51C79FE1-50DC-4FF3-A945-B8C1C134C50E", "Cargo Identifier Type must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.CargoIdentifierType.Code), Res.GetString("94D41262-05CB-4E32-B4FB-C9E33B1648AC", "Cargo Identifier Type should be selected from the list"));

			dataObject.CargoIdentifierType.Code = shipment.TransportMode.ToString() switch
			{
				Constants.TransportModes.Sea => DocDataConstants.ILCargoIdentifierType.SeaDealImport,
				Constants.TransportModes.Road => DocDataConstants.ILCargoIdentifierType.LandDealImport,
				_ => ZString.Empty
			};
			((CodeDescription)dataObject.CargoIdentifierType).ValidateAll();
		}

		void BuildReceiverType(DeliveryOrderDocDataObject dataObject)
		{
			var codeDescriptionPairList = new ReceiverTypes();
			dataObject.ReceiverType = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.ReceiverType).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("273E5B08-9EB6-412F-B43A-79BE718B36A9", "Receiver Type must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.ReceiverType.Code), Res.GetString("E38B4ADC-B0B8-4E9F-93D8-9BE36139E191", "Receiver Type should be selected from the list"));

			((CodeDescription)dataObject.ReceiverType).ValidateAll();
		}
	
		readonly ForwardingShipment shipment;
	}
}
