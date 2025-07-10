using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderSynchroniser : BusinessObjectSynchroniser
	{
		public CusInBondHeaderSynchroniser(CusInBondHeader destination)
			: base(destination, destination.Consol)
		{
		}

		protected new CusInBondHeader Destination
		{
			get { return (CusInBondHeader)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Hook and Unhook FieldSynchronisers

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			importTransportModeSynchroniser = new FieldSynchroniser(Destination.BH_ImportTransportModeInfo, GetImportTransportMode, GetImportTransportModeInfos);
			Synchronisers.Add(importTransportModeSynchroniser);
			importConveyanceNameSynchroniser = new FieldSynchroniser(Destination.BH_ImportConveyanceNameInfo, GetImportConveyanceName, GetImportConveyanceNameInfo);
			Synchronisers.Add(importConveyanceNameSynchroniser);
			voyageNumberSynchroniser = new FieldSynchroniser(Destination.BH_VoyageNumberInfo, GetVoyageNumber, GetVoyageNumberInfo);
			Synchronisers.Add(voyageNumberSynchroniser);
			portUnladingSynchroniser = new FieldSynchroniser(Destination.BH_RL_NKPortUnladingInfo, GetPortUnlading, GetPortUnladingInfo);
			Synchronisers.Add(portUnladingSynchroniser);
			etaSynchroniser = new FieldSynchroniser(Destination.BH_ETAInfo, GetETA, GetETAInfo);
			Synchronisers.Add(etaSynchroniser);

			var billsSynchroniser = new CusInBondBillCollectionSynchroniser(Destination);
			Synchronisers.Add(billsSynchroniser);

			if (Destination.IsNVOCCHeader)
			{
				Synchronisers.Add(new OceanBillSynchroniser(Destination.OceanBill, Source));
			}

			Source.Containers.CountChanged -= new CollectionCountChangedEventHandler(Containers_CountChanged);
			Source.Containers.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			Source.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
			foreach (CommonContainer container in Source.Containers)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
				container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
			}

			Synchronisers.Add(new FieldSynchroniser(Destination.BH_CarrierSCACInfo, () => ((ConsolDataCalculator)ConsolDataCalculator).OrgProxySCAC.ToUpper(), GetInfosAffectingOrgProxySCAC));
			importLoadPortSynchroniser = new FieldSynchroniser(Destination.BH_RL_NKImportLoadPortInfo, GetMostInterestingTransportLoadPort, GetMostInterestingTransportLoadPortInfos);
			firstExportDateSynchroniser = new FieldSynchroniser(Destination.BH_FirstExportDateInfo, GetMostInterestingTransportETD, GetMostInterestingTransportETDInfos);
			Synchronisers.Add(importLoadPortSynchroniser);
			Synchronisers.Add(firstExportDateSynchroniser);
		}

		FieldSynchroniser importTransportModeSynchroniser;
		FieldSynchroniser importConveyanceNameSynchroniser;
		FieldSynchroniser voyageNumberSynchroniser;
		FieldSynchroniser portUnladingSynchroniser;
		FieldSynchroniser etaSynchroniser;

		IZType GetVoyageNumber()
		{
			var voyage = GetLegForVoyageAndVessel()?.JW_VoyageFlight ?? ZString.Empty;
			if (voyage.IsEmpty)
			{
				var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				return leg != null ? leg.JW_VoyageFlight.ToUpper().Left(Destination.VoyageNumberMaxLength) : ZString.Empty;
			}
			return voyage.Left(Destination.VoyageNumberMaxLength);
		}

		IEnumerable<ZPropertyInfo> GetVoyageNumberInfo()
		{
			yield return Source.JK_DatePortOfFirstArrivalInfo;
			yield return Source.JK_RL_NKPortOfFirstArrivalInfo;

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_ETD, Transport.Schema.JW_ETA))
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_VoyageFlight))
			{
				yield return info;
			}
		}

		IZType GetImportConveyanceName()
		{
			var vessel = GetLegForVoyageAndVessel()?.JW_Vessel ?? ZString.Empty;
			if (vessel.IsEmpty)
			{
				var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				return leg != null ? leg.JW_Vessel.ToUpper() : ZString.Empty;
			}
			return vessel;
		}

		IEnumerable<ZPropertyInfo> GetImportConveyanceNameInfo()
		{
			yield return Source.JK_DatePortOfFirstArrivalInfo;
			yield return Source.JK_RL_NKPortOfFirstArrivalInfo;

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_ETD, Transport.Schema.JW_ETA))
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_Vessel))
			{
				yield return info;
			}
		}

		Transport GetLegForVoyageAndVessel()
		{
			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Source.JK_RL_NKPortOfFirstArrival.Left(2)) == Core.Constants.CountryCodes.UnitedStates)
			{
				var transports = Source.Transports.OfType<Transport>().ToList();
				var legs = transports.Where(x => x.TransportMode == Core.Constants.TransportModes.Sea
				&& Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x.JW_RL_NKDiscPort.Left(2)) != Core.Constants.CountryCodes.UnitedStates);
				if (legs.Count() > 1)
				{
					var firstArrivalDate = Source.JK_DatePortOfFirstArrival;
					return legs.Where(x => x.JW_ETD <= firstArrivalDate && x.JW_ETA >= firstArrivalDate).OrderBy(o => o.JW_LegOrder).FirstOrDefault();
				}
			}
			return null;
		}

		IZType GetPortUnlading()
		{
			var portUnlading = GetLegForPortUnladingAndETA()?.JW_RL_NKDiscPort ?? ZString.Empty;
			if (portUnlading.IsEmpty)
			{
				var port = ConsolDataCalculator.FirstCountryPortOfDischarge;
				return port != null ? port.RL_Code.ToUpper() : ZString.Empty;
			}
			return portUnlading;
		}

		IEnumerable<ZPropertyInfo> GetPortUnladingInfo()
		{
			return ConsolDataCalculator.GetInfosAffectingFirstCountryPortOfDischarge();
		}

		IZType GetETA()
		{
			var eta = GetLegForPortUnladingAndETA()?.JW_ETA ?? ZDateTime.Empty;
			return eta.IsEmpty ? ConsolDataCalculator.FirstCountryDischargeDate : eta;
		}

		IEnumerable<ZPropertyInfo> GetETAInfo()
		{
			return ConsolDataCalculator.GetInfosAffectingFirstCountryDischargeDate();
		}

		Transport GetLegForPortUnladingAndETA()
		{
			return Source.Transports.OfType<Transport>().ToList().Where(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea
			&& Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x.JW_RL_NKDiscPort.Left(2)) == Core.Constants.CountryCodes.UnitedStates).OrderBy(o => o.JW_LegOrder).FirstOrDefault();
		}

		IZType GetMostInterestingTransportLoadPort()
		{
			var transport = GetMostInterestingTransportForBinding();
			return transport?.JW_RL_NKLoadPort ?? ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetMostInterestingTransportLoadPortInfos()
		{
			return ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_LegOrder, Transport.Schema.JW_TransportMode, Transport.Schema.JW_RL_NKLoadPort, Transport.Schema.JW_RL_NKDiscPort);
		}

		IZType GetMostInterestingTransportETD()
		{
			var transport = GetMostInterestingTransportForBinding();
			return transport?.JW_ETD ?? ZDateTime.Empty;
		}

		IEnumerable<ZPropertyInfo> GetMostInterestingTransportETDInfos()
		{
			return ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_LegOrder, Transport.Schema.JW_TransportMode, Transport.Schema.JW_RL_NKLoadPort, Transport.Schema.JW_RL_NKDiscPort, Transport.Schema.JW_ETD);
		}

		protected override void UnHookSynchronisers()
		{
			Source.Transports.CountChanged -= Transports_CountChanged;
			importLoadPortSynchroniser = null;
			firstExportDateSynchroniser = null;

			foreach (CommonContainer container in Source.Containers)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
			}
			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			Source.Containers.CountChanged -= new CollectionCountChangedEventHandler(Containers_CountChanged);
			importTransportModeSynchroniser = null;
			importConveyanceNameSynchroniser = null;
			voyageNumberSynchroniser = null;
			portUnladingSynchroniser = null;
			etaSynchroniser = null;

			base.UnHookSynchronisers();
		}

		FieldSynchroniser importLoadPortSynchroniser;
		FieldSynchroniser firstExportDateSynchroniser;

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(importConveyanceNameSynchroniser);
			UpdateInfoEventsAndReSynchronise(voyageNumberSynchroniser);
			UpdateInfoEventsAndReSynchronise(portUnladingSynchroniser);
			UpdateInfoEventsAndReSynchronise(importLoadPortSynchroniser);
			UpdateInfoEventsAndReSynchronise(firstExportDateSynchroniser);
		}

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var container = e.BizObject as CommonContainer;
			if (container != null)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
				if (e.ItemAdded)
				{
					container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
				}
			}
			UpdateInfoEventsAndReSynchronise(importTransportModeSynchroniser);
		}

		void JC_ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(importTransportModeSynchroniser);
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingOrgProxySCAC()
		{
			yield return Destination.BH_ParentIDInfo;

			foreach (var info in ((ConsolDataCalculator)ConsolDataCalculator).GetInfosAffectingOrgProxySCAC())
			{
				yield return info;
			}
		}

		IZType GetImportTransportMode()
		{
			var result = ZString.Empty;
			if (Source.IsSea)
			{
				result = Source.Containers.Count == 0 || Source.Containers.FirstOrDefault(x => ((CommonContainer)x).JC_ContainerMode != Core.Constants.ContainerModes.BreakBulk) == null ? TransportTypeList.Codes.VesselNonContainer : TransportTypeList.Codes.VesselContainer;
			}
			else if (Source.IsRail)
			{
				result = TransportTypeList.Codes.Rail;
			}
			return result;
		}

		ZPropertyInfo[] GetImportTransportModeInfos()
		{
			return new ZPropertyInfo[] { Source.JK_TransportModeInfo };
		}

		#endregion

		#region Implementation

		Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new ConsolDataCalculator(Source, Destination)); }
		}
		ConsolDataCalculator consolDataCalculator;

		protected override void DisposeCore()
		{
			base.DisposeCore();
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
		}

		Transport GetMostInterestingTransportForBinding()
		{
			var orderHelper = new TransportOrderHelper(Source.Transports);
			return orderHelper.LastLegMatching(MatchTransportModeAndFromForeignToUS) ?? orderHelper.LastLeg;
		}

		Predicate<Transport> MatchTransportModeAndFromForeignToUS => transport =>
		{
			var matchTransportMode = transport.JW_TransportMode == Source.JK_TransportMode;
			var foreignToUs = transport.JW_RL_NKLoadPort.Left(2) != Core.Constants.CountryCodes.UnitedStates && transport.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.UnitedStates;
			return matchTransportMode && foreignToUs;
		};

		#endregion
	}
}
