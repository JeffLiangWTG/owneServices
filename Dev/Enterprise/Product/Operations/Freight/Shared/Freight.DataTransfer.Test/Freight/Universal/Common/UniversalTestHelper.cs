using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public static class UniversalTestHelper
	{
		public static JobVoyage CreateSeaVoyage(BusinessObjectFactory factory, ZString vesselName, ZString voyageNumber, ZGuid? carrierPK = null)
		{
			var vessel = RefVessel.LookupVesselByName(vesselName, factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = factory.New<RefVessel>();
				vessel.RV_Name = vesselName;
			}

			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = voyageNumber;
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			if (carrierPK.HasValue && !carrierPK.Value.IsEmpty)
			{
				voyage.JV_OH_Line = carrierPK.Value;
			}

			return voyage;
		}

		public static JobVoyage CreateAirVoyage(BusinessObjectFactory factory, ZString flightNumber, bool isCargoOnly = true)
		{
			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNumber;
			voyage.JV_IsCargoOnly = isCargoOnly;

			return voyage;
		}

		public static JobVoyage CreateRailVoyage(BusinessObjectFactory factory, ZString journey, ZString journeyNumber)
		{
			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			voyage.JV_RV_NKVessel = journey;
			voyage.JV_VoyageFlight = journeyNumber;

			return voyage;
		}

		public static JobVoyage CreateRoadVoyage(BusinessObjectFactory factory, ZString truckReference)
		{
			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			voyage.JV_VoyageFlight = truckReference;

			return voyage;
		}

		public static JobSailing AddSailing(JobVoyage voyage, ZString loadPort, ZString dischargePort)
		{
			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);
			if (sailing == null)
			{
				if (voyage.Origins.Cast<VoyageOrigin>().All(origin => origin.JA_RL_NKPortOfLoading != loadPort))
				{
					var origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = loadPort;
				}

				if (voyage.Destinations.Cast<VoyageDestination>().All(destination => destination.JB_RL_NKPortOfDischarge != dischargePort))
				{
					var destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = dischargePort;
				}

				sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);
			}

			return sailing;
		}

		public static JobSailing CreateSailingWithVoyage(BusinessObjectFactory factory, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNumber)
		{
			var voyage = CreateSeaVoyage(factory, vesselName, voyageNumber);
			var sailing = AddSailing(voyage, loadPort, dischargePort);

			return sailing;
		}

		public static string GetXml(IDataObject dataObject)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(dataObject, stream);

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static Classification CreateClassificationDataObject(ZString code, ZString typeCode, ZString typeDescription, ZString countryCode, ZString countryName)
		{
			return new Classification
			{
				Code = code,
				Country = new Country { Code = countryCode, Name = countryName },
				Type = new CodeDescriptionPair { Code = typeCode, Description = typeDescription }
			};
		}
	}
}
