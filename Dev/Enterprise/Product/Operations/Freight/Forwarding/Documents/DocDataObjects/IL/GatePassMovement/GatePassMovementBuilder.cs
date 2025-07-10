using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class GatePassMovementBuilder
	{
		public GatePassMovementBuilder(IGatePassMovementProvider gatePassMovementProvider)
		{
			this.gatePassMovementProvider = gatePassMovementProvider;
		}

		public GatePassMovementDocDataObject Build()
		{
			var factory = gatePassMovementProvider.Factory;

			var dataObject = new GatePassMovementDocDataObject(gatePassMovementProvider.SourceType, gatePassMovementProvider.SourceID, factory);
			dataObject.GatePassMovementNumber = gatePassMovementProvider.MessageReferenceNumber;

			BuildProcessType(dataObject);

			BuildOriginSite(factory, dataObject);

			BuildDestinationSite(factory, dataObject);

			BuildCargoType(factory, dataObject);

			BuildTransportMethod(factory, dataObject);

			BuildCargoIdentifierType(factory, dataObject);

			BuildCargoIdentifierKey1(dataObject);

			BuildCargoIdentifierKey2(dataObject);

			BuildCargoIdentifierKey3(dataObject);

			dataObject.ValidateAll();

			return dataObject;
		}

		void BuildProcessType(GatePassMovementDocDataObject dataObject)
		{
			dataObject.ProcessType = gatePassMovementProvider.ProcessType;
		}

		void BuildOriginSite(BusinessObjectFactory factory, GatePassMovementDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
			dataObject.OriginSite = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.OriginSite).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("8A8FAF42-49B1-46C7-94A6-7845DD869C10", "Origin Site must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.OriginSite.Code), Res.GetString("79902A20-7640-4C30-AEE8-5677D3CAD484", "Origin Site should be selected from the list"));

			dataObject.OriginSite.Code = gatePassMovementProvider.OriginSiteCode;

			((CodeDescription)dataObject.OriginSite).ValidateAll();
		}

		void BuildDestinationSite(BusinessObjectFactory factory, GatePassMovementDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
			dataObject.DestinationSite = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.DestinationSite).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("672DFEDB-F822-4A3B-A40F-DEBCA54006E5", "Destination Site must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.DestinationSite.Code), Res.GetString("CAC3A9EC-E7B9-47D4-8AAC-AA8E203E1F35", "Destination Site should be selected from the list"));

			dataObject.DestinationSite.Code = gatePassMovementProvider.DestinationSiteCode;

			((CodeDescription)dataObject.DestinationSite).ValidateAll();
		}

		void BuildCargoType(BusinessObjectFactory factory, GatePassMovementDocDataObject dataObject)
		{
			var cargoTypeCodeList = gatePassMovementProvider.CargoTypeCodeCollection;
			dataObject.CargoType = new CodeDescription(cargoTypeCodeList);
			var codeInfo = ((CodeDescription)dataObject.CargoType).CodeInfo;

			codeInfo.AddMessageError(() => !cargoTypeCodeList.ContainsCode(dataObject.CargoType.Code), Res.GetString("9C695011-DF5C-4CAD-9F4E-08B8440D08CB", "Cargo Type should be selected from the list"));

			dataObject.CargoType.Code = gatePassMovementProvider.CargoTypeCode;

			((CodeDescription)dataObject.CargoType).ValidateAll();
		}

		void BuildTransportMethod(BusinessObjectFactory factory, GatePassMovementDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, ZDateTime.Today);
			dataObject.TransportMethod = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.TransportMethod).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("622528BE-66AD-4A2F-8EDD-23462AA3227C", "Transport Method must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.TransportMethod.Code), Res.GetString("3B0A19CB-B650-4976-A18A-5799ED28D0C8", "Transport Method should be selected from the list"));

			((CodeDescription)dataObject.TransportMethod).ValidateAll();
		}

		void BuildCargoIdentifierType(BusinessObjectFactory factory, GatePassMovementDocDataObject dataObject)
		{
			var codeDescriptionPairList = RefCusCodeListTypes.GetCachedList(factory, Constants.CountryCodes.Israel, Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, ZDateTime.Today);
			dataObject.CargoIdentifierType = new CodeDescription(codeDescriptionPairList);
			var codeInfo = ((CodeDescription)dataObject.CargoIdentifierType).CodeInfo;

			codeInfo.AddMessageErrorIfEmpty(Res.GetString("050B07DB-7601-43F9-8A2D-2C036C9A13CF", "Cargo Identifier Type must be provided"));
			codeInfo.AddMessageError(() => !codeDescriptionPairList.ContainsCode(dataObject.CargoIdentifierType.Code), Res.GetString("4D1547A8-B1CF-4CA8-97BA-8AB5C3A14B1C", "Cargo Identifier Type should be selected from the list"));

			dataObject.CargoIdentifierType.Code = gatePassMovementProvider.CargoIdentifierTypeCode;
			((CodeDescription)dataObject.CargoIdentifierType).ValidateAll();
		}

		void BuildCargoIdentifierKey1(GatePassMovementDocDataObject dataObject)
		{
			dataObject.CargoIdentifierKey1Info
				.AddMessageErrorIfEmpty(Res.GetString("FD333ABC-3BA3-447F-BDD0-353A7313D83C", "Cargo Identifier Key 1 must be provided"));

			dataObject.CargoIdentifierKey1 = gatePassMovementProvider.CargoIdentifierKey1;
		}

		void BuildCargoIdentifierKey2(GatePassMovementDocDataObject dataObject)
		{
			dataObject.CargoIdentifierKey2 = gatePassMovementProvider.CargoIdentifierKey2;

			if (gatePassMovementProvider.TransportMode != Constants.TransportModes.Road)
			{
				dataObject.CargoIdentifierKey2Info
					.AddMessageErrorIfEmpty(Res.GetString("17413296-71CE-40E2-98BC-A3C1B89F48CF", "Cargo Identifier Key 2 must be provided"));
			}
		}

		void BuildCargoIdentifierKey3(GatePassMovementDocDataObject dataObject)
		{
			dataObject.CargoIdentifierKey3 = gatePassMovementProvider.CargoIdentifierKey3;
			dataObject.CargoIdentifierKey3IsVisible = gatePassMovementProvider.CargoIdentifierKey3IsVisible;
		}

		readonly IGatePassMovementProvider gatePassMovementProvider;
	}
}
