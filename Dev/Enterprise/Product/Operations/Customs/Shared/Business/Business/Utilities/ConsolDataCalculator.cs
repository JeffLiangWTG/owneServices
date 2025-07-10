using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public abstract class ConsolDataCalculator : IDisposable
	{
		protected ConsolDataCalculator(ForwardingConsol consol, ZString countryCode)
		{
			this.consol = consol;
			this.countryCode = countryCode;
			HookToEventsAffectingTransportsOrder();
		}
		protected readonly ForwardingConsol consol;
		readonly ZString countryCode;

		#region Hook events

		void HookToEventsAffectingTransportsOrder()
		{
			foreach (Transport transport in consol.Transports)
			{
				HookTransport(transport);
			}
			consol.Transports.CountChanged += Transports_CountChanged;
		}

		void UnHookEventsAffectingTransportsOrder()
		{
			foreach (Transport transport in consol.Transports)
			{
				UnHookTransport(transport);
			}
			consol.Transports.CountChanged -= Transports_CountChanged;
		}

		void UnHookTransport(Transport transport)
		{
			transport.JW_LegOrderInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
			transport.JW_TransportModeInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
		}

		void HookTransport(Transport transport)
		{
			UnHookTransport(transport);
			transport.JW_LegOrderInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
			transport.JW_TransportModeInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
			transport.JW_RL_NKLoadPortInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookTransport((Transport)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookTransport((Transport)e.BizObject);
			}
			TransportsOrderAffecting_ValueChanged(sender, e);
		}

		protected void TransportsOrderAffecting_ValueChanged(object sender, EventArgs e)
		{
			shouldCalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode = true;
			shouldCalculateLastCountryDepartureTransportOrLastTransportWithTransportMode = true;
		}

		#endregion

		public IEnumerable<ZPropertyInfo> GetInfosAffectingFirstCountryPortOfDischarge()
		{
			yield return consol.JK_DatePortOfFirstArrivalInfo;
			yield return consol.JK_RL_NKPortOfFirstArrivalInfo;

			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_ETA, Transport.Schema.JW_ATA))
			{
				yield return info;
			}
		}

		public virtual IEnumerable<ZPropertyInfo> GetInfosAffectingTransportsOrder()
			=> GetTransportsInfos(Transport.Schema.JW_LegOrder, Transport.Schema.JW_TransportMode, Transport.Schema.JW_RL_NKLoadPort, Transport.Schema.JW_RL_NKDiscPort);

		public IEnumerable<ZPropertyInfo> GetTransportsInfos(params string[] propertyNames)
		{
			foreach (Transport transport in consol.Transports)
			{
				foreach (var propertyName in propertyNames)
				{
					if (transport.ZPropertyInfoHash.ContainsKey(propertyName))
					{
						yield return transport.ZPropertyInfoHash[propertyName];
					}
				}
			}
		}

		#region FirstCountryPortOfDischarge

		public RefUNLOCO FirstCountryPortOfDischarge
		{
			get
			{
				RefUNLOCO result = null;
				var transport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				var firstCountryDischargeDate = ZDateTime.Empty;
				if (transport != null)
				{
					result = transport.DiscPort;
					firstCountryDischargeDate = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
				}
				if (result == null ||
					!IsPortInTheCountry(result) ||
					IsConsolPortOfFirstArrivalMoreValid(firstCountryDischargeDate))
				{
					if (ShouldClobberFirstCountryPortOfDischargeWithConsolsPortOfFirstArrival(consol.PortOfFirstArrival))
					{
						result = consol.PortOfFirstArrival;
					}
				}
				return result;
			}
		}

		protected virtual bool ShouldClobberFirstCountryPortOfDischargeWithConsolsPortOfFirstArrival(RefUNLOCO port)
		{
			return true;
		}

		bool IsConsolPortOfFirstArrivalMoreValid(ZDateTime date)
		{
			return (date > consol.JK_DatePortOfFirstArrival && IsPortInTheCountry(consol.PortOfFirstArrival));
		}

		#endregion

		#region FirstCountryBoundTransportOrFirstTransportWithTransportMode

		public Transport FirstCountryBoundTransportOrFirstTransportWithTransportMode
		{
			get { return CalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode(); }
		}

		public Transport FirstCountryBoundTransport
		{
			get
			{
				CalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode();
				return firstCountryBoundTransport;
			}
		}

		Transport CalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode()
		{
			if (shouldCalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode)
			{
				shouldCalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode = false;
				firstCountryBoundTransportOrFirstTransportWithTransportMode = null;
				var transportMode = GetConsolTransportMode();
				foreach (Transport transport in CountryDischargeTransportsInLegOrder)
				{
					if (transport.JW_TransportMode == transportMode)
					{
						firstCountryBoundTransportOrFirstTransportWithTransportMode = transport;
						break;
					}
				}
				firstCountryBoundTransport = firstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (firstCountryBoundTransportOrFirstTransportWithTransportMode == null)
				{
					firstCountryBoundTransportOrFirstTransportWithTransportMode = consol.Transports.FirstTransportWithTransportMode(transportMode);
				}
			}
			return firstCountryBoundTransportOrFirstTransportWithTransportMode;
		}
		Transport firstCountryBoundTransportOrFirstTransportWithTransportMode;
		Transport firstCountryBoundTransport;

		bool shouldCalculateFirstCountryBoundTransportOrFirstTransportWithTransportMode = true;

		#endregion

		#region LastCountryDepartureTransportOrLastTransportWithTransportMode

		public Transport LastCountryDepartureTransportOrLastTransportWithTransportMode
		{
			get { return CalculateLastCountryDepartureTransportOrLastTransportWithTransportMode(); }
		}

		public Transport LastCountryDepartureTransport
		{
			get
			{
				CalculateLastCountryDepartureTransportOrLastTransportWithTransportMode();
				return lastCountryDepartureTransport;
			}
		}

		Transport CalculateLastCountryDepartureTransportOrLastTransportWithTransportMode()
		{
			if (shouldCalculateLastCountryDepartureTransportOrLastTransportWithTransportMode)
			{
				shouldCalculateLastCountryDepartureTransportOrLastTransportWithTransportMode = false;
				lastCountryDepartureTransportOrLastTransportWithTransportMode = null;
				var transportMode = GetConsolTransportMode();
				foreach (var transport in CountryLoadTransportsInLegOrder)
				{
					if (transport.JW_TransportMode == transportMode)
					{
						lastCountryDepartureTransportOrLastTransportWithTransportMode = transport;
						break;
					}
				}
				lastCountryDepartureTransport = lastCountryDepartureTransportOrLastTransportWithTransportMode;
				if (lastCountryDepartureTransportOrLastTransportWithTransportMode == null)
				{
					lastCountryDepartureTransportOrLastTransportWithTransportMode = consol.Transports.LastTransportWithTransportMode(transportMode);
				}
			}
			return lastCountryDepartureTransportOrLastTransportWithTransportMode;
		}
		Transport lastCountryDepartureTransportOrLastTransportWithTransportMode;
		Transport lastCountryDepartureTransport;

		bool shouldCalculateLastCountryDepartureTransportOrLastTransportWithTransportMode = true;

		#endregion

		protected abstract ZString GetConsolTransportMode();

		IEnumerable<Transport> CountryDischargeTransportsInLegOrder
			=> TransportsInLegOrder.Where(transport => IsPortInTheCountry(transport.DiscPort) && !IsPortInTheCountry(transport.LoadPort));

		protected IEnumerable<Transport> TransportsInLegOrder => consol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder);

		IEnumerable<Transport> CountryLoadTransportsInLegOrder
			=> TransportsInDescendingLegOrder.Where(transport => IsPortInTheCountry(transport.LoadPort) && !IsPortInTheCountry(transport.DiscPort));

		protected IEnumerable<Transport> TransportsInDescendingLegOrder => consol.Transports.Cast<Transport>().OrderByDescending(x => x.JW_LegOrder);

		#region FirstCountryDischargeDate

		public ZDateTime FirstCountryDischargeDate
		{
			get
			{
				return GetFirstCountryDischargeDate(transport => transport.JW_ETA, transport => transport.JW_ATA);
			}
		}

		protected ZDateTime GetFirstCountryDischargeDate(Func<Transport, ZDateTime> getETA, Func<Transport, ZDateTime> getATA)
		{
			var result = ZDateTime.Empty;
			var transport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			RefUNLOCO firstCountryPortOfDischarge = null;
			if (transport != null)
			{
				firstCountryPortOfDischarge = transport.DiscPort;
				var ata = getATA(transport);
				result = ata.IsEmpty ? getETA(transport) : ata;
			}
			if (firstCountryPortOfDischarge == null ||
				!IsPortInTheCountry(firstCountryPortOfDischarge) ||
				IsConsolPortOfFirstArrivalMoreValid(result))
			{
				if (ShouldClobberFirstCountryDischargeDateWithConsolsDatePortOfFirstArrival(consol.JK_DatePortOfFirstArrival))
				{
					result = consol.JK_DatePortOfFirstArrival;
				}
			}
			return result;
		}

		protected virtual bool ShouldClobberFirstCountryDischargeDateWithConsolsDatePortOfFirstArrival(ZDateTime zDateTime)
		{
			return true;
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingFirstCountryDischargeDate()
		{
			yield return consol.JK_DatePortOfFirstArrivalInfo;
			yield return consol.JK_RL_NKPortOfFirstArrivalInfo;
			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_ETA, Transport.Schema.JW_ATA))
			{
				yield return info;
			}
		}

		#endregion

		protected bool IsPortInTheCountry(RefUNLOCO port)
		{
			return port != null && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(port.RL_RN_NKCountryCode) == countryCode;
		}

		protected BusinessObjectFactory Factory
		{
			get { return consol.Factory; }
		}

		#region IDisposable

		public virtual void Dispose()
		{
			UnHookEventsAffectingTransportsOrder();
		}

		#endregion
	}
}
