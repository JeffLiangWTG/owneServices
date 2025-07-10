using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class ConsolTransportModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ConsolTransportModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.TransportModes.Air, nameof(Xsd.ConsolTransportMode.AIR));
			yield return new Mapping(Core.Constants.TransportModes.Rail, nameof(Xsd.ConsolTransportMode.RAI));
			yield return new Mapping(Core.Constants.TransportModes.Road, nameof(Xsd.ConsolTransportMode.ROA));
			yield return new Mapping(Core.Constants.TransportModes.Sea, nameof(Xsd.ConsolTransportMode.SEA));
			yield return new Mapping(Core.Constants.TransportModes.RollOnRollOff, nameof(Xsd.ConsolTransportMode.ROR));
			yield return new Mapping(Core.Constants.TransportModes.Unknown, nameof(Xsd.ConsolTransportMode.UNK));
		}

		public static readonly ConsolTransportModeToXmlCodeMappings Instance = new ConsolTransportModeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("dbcad410-1793-4200-a794-9e20d5c633f9", "Consol Transport Mode"); }
		}

		public new Xsd.ConsolTransportMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ConsolTransportMode.SEA, errorContext, notify);
		}
	}
}
