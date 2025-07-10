using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class JobDeclarationTransportSupporter : BaseJobDeclarationTransportSupporter<JobDeclaration>
	{
		public JobDeclarationTransportSupporter(JobDeclaration parent) : base(parent)
		{
		}

		void UpdateIfPortMatchesCurrentCountry(Transport transport, ZPropertyInfo propertyInfo, IZType value)
		{
			var matchingTransport = Parent.Transports.Cast<Transport>()
				.Where(t => CountryCodeOfPort(t) == Parent.CountryCode)
				.OrderBy(t => t.JW_LegOrder)
				.FirstOrDefault();
			if (matchingTransport == transport)
			{
				Parent.UpdateRoutingDefaultIfAllowed(propertyInfo, value);
			}
		}

		ZString CountryCodeOfPort(Transport transport)
		{
			var port = ZString.Empty;
			if (Parent.IsImport)
			{
				port = transport.JW_RL_NKDiscPort;
			}
			if (Parent.IsExport)
			{
				port = transport.JW_RL_NKLoadPort;
			}
			return port.SubstringSafe(0, 2);
		}

		protected override void ETASetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			if (Parent.JE_RL_NKPortOfArrival == transport.JW_RL_NKDiscPort)
			{
				UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_DateOfArrivalInfo, transport.JW_ETA);
			}
		}

		protected override void ETDSetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			if (Parent.JE_RL_NKPortOfLoading == transport.JW_RL_NKLoadPort)
			{
				UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_ExportDateInfo, transport.JW_ETD);
			}
		}

		protected override void NotifyCarrierAddressChangedCore(Transport transport, ZGuid previousValue)
		{
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_OH_ShippingLineInfo, transport.CarrierPK);
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_RL_NKPortOfArrivalInfo, transport.JW_RL_NKDiscPort);
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			if (Parent.Transports.Count > 0)
			{
				UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_RL_NKOriginInfo, Parent.Transports[0].JW_RL_NKLoadPort);
			}
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_RL_NKPortOfLoadingInfo, transport.JW_RL_NKLoadPort);
		}

		protected override void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_TransportModeInfo, transport.JW_TransportMode);
		}

		protected override void NotifyVesselChangedCore(Transport transport, ZString previousValue)
		{
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_VesselNameInfo, Parent.IsSea ? transport.JW_Vessel : ZString.Empty);
		}

		protected override void NotifyVoyageFlightChangedCore(Transport transport, ZString previousValue)
		{
			UpdateIfPortMatchesCurrentCountry(transport, Parent.JE_VoyageFlightNoInfo, transport.JW_VoyageFlight);
		}
	}
}
