using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	internal class InvoiceTransportWrapper
	{
		readonly IEnumerable<Transport> transports;
		readonly bool hasTransportLegs;
		readonly BusinessObjectFactory factory;

		public InvoiceTransportWrapper(BusinessObjectFactory factory, IEnumerable<Transport> transports)
		{
			this.factory = factory;
			this.transports = transports;
			hasTransportLegs = this.transports.Any();
		}

		bool IsSeaTransport => factory.GetValue(ref isSeaTransportCached, () => transports.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea && x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel) && !transports.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Air && x.JW_TransportType == Core.Constants.TransportPlanningType.Flight1));
		CachedProperty<bool> isSeaTransportCached;

		bool IsAirTransport => factory.GetValue(ref isAirTransportCached, () => transports.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Air && x.JW_TransportType == Core.Constants.TransportPlanningType.Flight1) && !transports.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea && x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel));
		CachedProperty<bool> isAirTransportCached;

		public ZString Transportation
		{
			get
			{
				var result = ZString.Empty;
				if (hasTransportLegs)
				{
					if (IsAirTransport)
					{
						result = Enterprise.DocumentWrappers.GenericWrappers.TransportModeList.Descriptions.Airfreight.ToString().ToUpper();
					}
					else if (IsSeaTransport)
					{
						result = transports.FirstOrDefault(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea && x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel).JW_VesselForBinding;
					}
				}
				return result;
			}
		}

		public ZString PortOfOriginName
		{
			get
			{
				var result = ZString.Empty;
				if (hasTransportLegs)
				{
					var transport = transports.FirstOrDefault(x => x.JW_TransportType == Core.Constants.TransportPlanningType.Flight1 || x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
					if (transport != null)
					{
						result = GetPortNameWithCountry(transport.JW_RL_NKLoadPortForBinding);
					}
				}
				return result;
			}
		}

		public ZString FinalDestinationName
		{
			get
			{
				var result = ZString.Empty;
				if (hasTransportLegs)
				{
					var typesToLookFor = System.Array.Empty<string>();
					if (IsAirTransport)
					{
						typesToLookFor = [Core.Constants.TransportPlanningType.Other, Core.Constants.TransportPlanningType.Flight3, Core.Constants.TransportPlanningType.Flight2, Core.Constants.TransportPlanningType.Flight1];
					}
					else if (IsSeaTransport)
					{
						typesToLookFor = [Core.Constants.TransportPlanningType.Other, Core.Constants.TransportPlanningType.MainVessel];
					}

					foreach (var type in typesToLookFor)
					{
						var theLastTransport = transports.FirstOrDefault(x => x.JW_TransportType == type);
						if (theLastTransport != null)
						{
							result = GetPortNameWithCountry(theLastTransport.JW_RL_NKDiscPortForBinding);
							break;
						}
					}
				}
				return result;
			}
		}

		ZString GetPortNameWithCountry(ZString portCode)
		{
			var result = ZString.Empty;
			var refUNLOCO = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, portCode));
			if (refUNLOCO != null)
			{
				result = string.Format(CultureInfo.InvariantCulture, "{0} - {1}", refUNLOCO.Country?.RN_Desc ?? ZString.Empty, refUNLOCO.RL_NameWithDiacriticals);
			}
			return result;
		}
	}
}
