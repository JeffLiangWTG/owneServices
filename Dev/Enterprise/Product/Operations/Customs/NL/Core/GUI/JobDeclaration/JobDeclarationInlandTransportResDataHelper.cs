using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Res = Enterprise.Customs.NL.GUI.Res;

namespace Enterprise.Customs.NL.Business.Declaration;

public static class JobDeclarationInlandTransportResDataHelper
{
	public static ResourceStringData GetInlandTransactionIDCaption(BaseJobDeclaration declaration) => GetInlandTransactionRes(declaration.JE_TransportModeInland, declaration.JE_TransportMeans);

	public static ResourceStringData InlandModeOfTransportResString => Res.GetData("C417ADD6-35E4-452F-89F6-47B9FBA1865A", "Inland M.O.T.", "[UCC 7/5] Inland M.O.T.", "[UCC 7/5] Inland Mode of Transport");
	public static ResourceStringData InlandTransportCodeResString => Res.GetData("C955DE41-CAAA-403E-96B7-AA8AF34EED22", "Code", "Code", "[19 06 061 000] type of identification of the transport");
	public static ResourceStringData InlandTransportNationalityCodeFindBoxResString => Res.GetData("D53D278F-F598-4694-AEA7-A920E8545D50", "Nationality", "Nationality", "[19 06 062 000] Nationality inland transport");

	static ResourceStringData ResDataRoad => Res.GetData("613B95CE-0E7E-48CA-9757-84B164ED7028", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Vehicle Registration Number");
	static ResourceStringData ResDataAirIATAFlightNumber => Res.GetData("8C2F8CDB-DABD-4577-B9E4-9A93C55F7FE6", englishCaption: "Flight No.", englishFullDescription: "[19 05 017 000] Flight No.");
	static ResourceStringData ResDataAirAircraftNumber => Res.GetData("59A0E699-3735-43F4-A74F-F007AC55F419", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Aircraft Registration Number");
	static ResourceStringData ResDataInlandWaterwaysEUVesselCode => Res.GetData("25484A28-6269-41A2-976C-5211B5AC7068", englishCaption: "ENI Code", englishFullDescription: "[19 05 017 000] European Vessel Identification Number");
	static ResourceStringData ResDataInlandWaterwaysInlandWaterwaysVesselName => ResDataVesselName;
	static ResourceStringData ResDataRailWagonNumber => Res.GetData("DC8FADAD-69E5-4E95-A8DF-178EA5A69282", englishCaption: "Wagon No.", englishFullDescription: "[19 05 017 000] Wagon Number");
	static ResourceStringData ResDataRailTrainNumber => Res.GetData("F634BCBE-5E59-4BA0-B176-98E8B402D5E8", englishCaption: "Train No.", englishFullDescription: "[19 05 017 000] Train Number");
	static ResourceStringData ResDataSeaIMOShipNumber => Res.GetData("70E6465E-A639-453A-A3C3-B673A951734B", englishCaption: "Lloyds No.", englishFullDescription: "[19 05 017 000] Lloyds Number");
	static ResourceStringData ResDataSeaSeaGoingShipName => ResDataVesselName;

	static ResourceStringData ResDataVesselName => Res.GetData("5A858356-07F2-45C6-AF3E-5EAAD200579E", englishCaption: "Vessel Name", englishFullDescription: "[19 05 017 000] Vessel Name");
	static ResourceStringData ResDataTransportId => Res.GetData("2A16059A-D6C7-470F-A2AD-AF053F715E4B", englishCaption: "Transport ID", englishFullDescription: "[19 06 017 000] Identification number of the transport");
	public static ResourceStringData VoyageResString => Res.GetData("17D565AF-E32F-4553-B761-5F1471CF8C96", "Voyage", "[UCC 7/7] Voyage");
	public static ResourceStringData MasterBillResString => Res.GetData("39B87309-5D8A-488E-ACDB-2F0F9CC9C02A", "Master Bill", "[UCC 7/7] Master Bill");
	public static ResourceStringData OceanBillResString => Res.GetData("5ACB7D69-EA46-4CB2-A4DD-85FB67540F0C", "Ocean Bill", "[UCC 7/7] Ocean Bill");
	public static ResourceStringData TransportIdResString => Res.GetData("F25C62A5-B3DB-4945-B11C-56C4037140A8", "Transport ID", "[UCC 7/7] Transport ID");

	static ResourceStringData GetInlandTransactionRes(string transportMode, string transportMeans)
	{
		ResourceStringData result = null;

		switch (transportMode)
		{
			case TransportTypeList.Codes.Road:
				result = ResDataRoad;
				break;
			case TransportTypeList.Codes.Air:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.IataFlightNumber:
						result = ResDataAirIATAFlightNumber;
						break;
					case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
						result = ResDataAirAircraftNumber;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.InlandWaterwayTransport:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
						result = ResDataInlandWaterwaysInlandWaterwaysVesselName;
						break;
					case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
						result = ResDataInlandWaterwaysEUVesselCode;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.Rail:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.WagonNumber:
						result = ResDataRailWagonNumber;
						break;
					case TransportMeansList.Codes.TrainNumber:
						result = ResDataRailTrainNumber;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.Sea:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.ImoShipIdentificationNumber:
						result = ResDataSeaIMOShipNumber;
						break;
					case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
						result = ResDataSeaSeaGoingShipName;
						break;
					default:
						break;
				}
				break;
		}

		return result ?? ResDataTransportId;
	}
}
