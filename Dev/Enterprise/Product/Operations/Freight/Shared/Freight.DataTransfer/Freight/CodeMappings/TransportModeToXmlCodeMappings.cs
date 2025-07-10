using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class TransportModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		TransportModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.TransportModes.Air, nameof(Xsd.TransportMode.AIR));
			yield return new Mapping(Core.Constants.TransportModes.Sea, nameof(Xsd.TransportMode.SEA));
			yield return new Mapping(Core.Constants.TransportModes.AirSea, nameof(Xsd.TransportMode.FAS));
			yield return new Mapping(Core.Constants.TransportModes.SeaAir, nameof(Xsd.TransportMode.FSA));
			yield return new Mapping(Core.Constants.TransportModes.Road, nameof(Xsd.TransportMode.ROA));
			yield return new Mapping(Core.Constants.TransportModes.Rail, nameof(Xsd.TransportMode.RAI));
			yield return new Mapping(Core.Constants.TransportModes.Mail, nameof(Xsd.TransportMode.MAI));
			yield return new Mapping(Core.Constants.TransportModes.Courier, nameof(Xsd.TransportMode.COU));
			yield return new Mapping(Core.Constants.TransportModes.Storage, nameof(Xsd.TransportMode.STO));
			yield return new Mapping(Core.Constants.TransportModes.WarehouseHandling, nameof(Xsd.TransportMode.HND));
			yield return new Mapping(Core.Constants.TransportModes.Other, nameof(Xsd.TransportMode.OTH));
			yield return new Mapping(Core.Constants.TransportModes.Unknown, nameof(Xsd.TransportMode.UNK));
			yield return new Mapping(Core.Constants.TransportModes.BorderWaterBorne, nameof(Xsd.TransportMode.BWB));
			yield return new Mapping(Core.Constants.TransportModes.Truck, nameof(Xsd.TransportMode.TRK));
			yield return new Mapping(Core.Constants.TransportModes.Auto, nameof(Xsd.TransportMode.AUT));
			yield return new Mapping(Core.Constants.TransportModes.Pedestrian, nameof(Xsd.TransportMode.PED));
			yield return new Mapping(Core.Constants.TransportModes.PassengerHandCarried, nameof(Xsd.TransportMode.PHC));
			yield return new Mapping(Core.Constants.TransportModes.FixedTransportInstallations, nameof(Xsd.TransportMode.FIX));
			yield return new Mapping(Core.Constants.TransportModes.InlandWaterwayTransport, nameof(Xsd.TransportMode.IWT));
			yield return new Mapping(Core.Constants.TransportModes.OwnPropulsion, nameof(Xsd.TransportMode.OWN));
			yield return new Mapping(Core.Constants.TransportModes.RollOnRollOff, nameof(Xsd.TransportMode.ROR));
		}

		public static readonly TransportModeToXmlCodeMappings Instance = new TransportModeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("dd18f57e-1b06-48ed-9bd3-08fd6d1c0d19", "Transport Mode"); }
		}

		public new Xsd.TransportMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.TransportMode.SEA, errorContext, notify);
		}
	}
}
