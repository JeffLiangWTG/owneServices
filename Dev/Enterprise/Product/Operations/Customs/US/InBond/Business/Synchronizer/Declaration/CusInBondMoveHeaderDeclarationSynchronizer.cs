using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using BaseCustoms = Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondMoveHeaderDeclarationSynchronizer : BaseCustoms.BusinessObjectSynchroniser
	{
		internal CusInBondMoveHeaderDeclarationSynchronizer(CusInBondMoveHeader destination)
			: base(destination, destination.Header.Declaration)
		{
		}

		protected new CusInBondMoveHeader Destination
		{
			get { return (CusInBondMoveHeader)base.Destination; }
		}

		protected new JobDeclaration Source
		{
			get { return (JobDeclaration)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				destinationPortFieldSynchroniser = new BaseCustoms.FieldSynchroniser(Destination.BM_DestinationPortCodeInfo, GetDestinationPort, GetDestinationPortRelatedInfos);
				Synchronisers.Add(destinationPortFieldSynchroniser);

				foreignDestinationFieldSynchroniser = new BaseCustoms.FieldSynchroniser(Destination.BM_ForeignDestPortKCodeInfo, GetForeignDestPort, GetForeignDestPortRelatedInfos);
				Synchronisers.Add(foreignDestinationFieldSynchroniser);

				Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
				Source.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
			}
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(destinationPortFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(foreignDestinationFieldSynchroniser);
		}

		protected override void UnHookSynchronisers()
		{
			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);

			base.UnHookSynchronisers();
		}

		#region Ports

		BaseCustoms.FieldSynchroniser destinationPortFieldSynchroniser;
		BaseCustoms.FieldSynchroniser foreignDestinationFieldSynchroniser;

		IZType GetDestinationPort()
		{
			var result = ZString.Empty;
			if (Destination.BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport || Destination.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport)
			{
				if (Destination.BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport)
				{
					result = Source.US_SchDEntry;
				}
				if (result.IsEmpty)
				{
					var mostInterestingLeg = GetMostInterestingLeg();
					if (mostInterestingLeg != null)
					{
						result = USScheduleResolver.GetScheduleCode(Schedule.D, mostInterestingLeg.JW_RL_NKDiscPort, mostInterestingLeg.JW_TransportMode, Destination.Factory);
					}
				}
			}
			return result.Left(CusInBondMoveHeader.Schema.BM_DestinationPortCodeMaxLength);
		}

		Transport GetMostInterestingLeg()
		{
			var transports = GetListOfTransports();

			Transport mostInterestingLeg = null;
			foreach (Transport transport in transports)
			{
				if (mostInterestingLeg == null && transport.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.UnitedStates)
				{
					mostInterestingLeg = transport;
				}

				if (mostInterestingLeg != null &&
					transport.JW_RL_NKLoadPort == mostInterestingLeg.JW_RL_NKDiscPort &&
					transport.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.UnitedStates &&
					GetFallBackDate(transport.JW_ATD, transport.JW_ETD) >= GetFallBackDate(mostInterestingLeg.JW_ATA, mostInterestingLeg.JW_ETA))
				{
					mostInterestingLeg = transport;
				}
			}
			return mostInterestingLeg;
		}

		IEnumerable<ZPropertyInfo> GetDestinationPortRelatedInfos()
		{
			yield return Destination.BM_InBondEntryTypeInfo;
			yield return Source.US_SchDEntryInfo;

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_TransportMode,
				Transport.Schema.JW_ETD, Transport.Schema.JW_ATD,
				Transport.Schema.JW_ETA, Transport.Schema.JW_ATA,
				Transport.Schema.JW_RL_NKLoadPort, Transport.Schema.JW_RL_NKDiscPort
				))
			{
				yield return info;
			}
		}

		IZType GetForeignDestPort()
		{
			var result = ZString.Empty;

			if (Destination.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport ||
				Destination.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport)
			{
				var transports = GetListOfTransports();
				var leg = (from Transport transport in transports
						   where transport.JW_RL_NKLoadPort.Left(2) == Core.Constants.CountryCodes.UnitedStates
							   && transport.JW_RL_NKDiscPort.Left(2) != Core.Constants.CountryCodes.UnitedStates
						   select transport).FirstOrDefault();

				if (leg != null)
				{
					result = USScheduleResolver.GetScheduleCode(Schedule.K, leg.JW_RL_NKDiscPort, leg.JW_TransportMode, Destination.Factory);
				}
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetForeignDestPortRelatedInfos()
		{
			yield return Destination.BM_InBondEntryTypeInfo;

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_TransportMode,
				Transport.Schema.JW_ETD, Transport.Schema.JW_ATD,
				Transport.Schema.JW_ETA, Transport.Schema.JW_ATA,
				Transport.Schema.JW_RL_NKLoadPort, Transport.Schema.JW_RL_NKDiscPort
				))
			{
				yield return info;
			}
		}

		List<Transport> GetListOfTransports()
		{
			return new List<Transport>(Source.Transports.OfType<Transport>().OrderBy(x => GetFallBackDate(x.JW_ATD, x.JW_ETD)));
		}

		ZDateTime GetFallBackDate(ZDateTime actual, ZDateTime estimate)
		{
			return actual.IsEmpty ? estimate : actual;
		}

		IEnumerable<ZPropertyInfo> GetTransportsInfos(params string[] propertyNames)
		{
			foreach (Transport transport in GetListOfTransports())
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

		#endregion
	}
}
