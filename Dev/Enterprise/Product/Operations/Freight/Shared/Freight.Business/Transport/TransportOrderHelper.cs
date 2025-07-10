using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class TransportOrderHelper : IEnumerable<Transport>
	{
		public TransportOrderHelper(TransportCollection collection)
		{
			list = (Transport[])collection.ToArray(typeof(Transport));
			Array.Sort(list, new LegOrderComparer());
		}

		public TransportOrderHelper(RoutingCollection collection)
		{
			list = (Transport[])collection.ToArray(typeof(Transport));
			chains = MovementLegComparer.SortMovementLegsByPorts(list);
		}

		public TransportOrderHelper(Transport[] transports)
		{
			list = transports;
			chains = MovementLegComparer.SortMovementLegsByPorts(list);
		}

		public Transport this[int index] => list[index];

		public int Count => list.Length;

		public Transport FirstLeg
		{
			get { return list.Length == 0 ? null : list[0]; }
		}

		public Transport FirstLegMatching(Predicate<Transport> predicate)
		{
			Argument.NotNull(predicate, nameof(predicate));

			foreach (var transport in list)
			{
				if (predicate(transport))
				{
					return transport;
				}
			}

			return null;
		}

		// this should be removed in faviour of FirstLegMatching
		public Transport FirstLegWithTransportMode(ZString transportMode)
		{
			return FirstLegMatching((t) => t.JW_TransportMode == transportMode);
		}

		public Transport LastLeg
		{
			get { return list.Length == 0 ? null : list[list.Length - 1]; }
		}

		// this monstrosity should be removed in faviour of LastLegMatching
		public Transport LastLegWithTransportMode(ZString transportMode, RefVessel vessel, ZString voyageFlight)
		{
			return LastLegMatching(delegate(Transport transport)
			{
				if (transport.JW_TransportMode != transportMode)
				{
					return false;
				}
				else if (transport.IsSea)
				{
					return vessel == null || transport.Vessel == vessel;
				}
				else if (transport.IsAir)
				{
					return voyageFlight.IsEmpty || voyageFlight == transport.JW_VoyageFlight;
				}
				else
				{
					return true;
				}
			});
		}

		public Transport LastLegMatching(Predicate<Transport> predicate)
		{
			Argument.NotNull(predicate, nameof(predicate));

			for (var index = list.Length - 1; index >= 0; index--)
			{
				var transport = list[index];

				if (predicate(transport))
				{
					return transport;
				}
			}

			return null;
		}

		public Transport ImportLeg
		{
			get
			{
				foreach (Transport transport in list)
				{
					if (ImportExportHelper.IsImport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort))
					{
						return transport;
					}
				}

				return null;
			}
		}

		public Transport ExportLeg
		{
			get
			{
				for (int index = list.Length - 1; index >= 0; index--)
				{
					Transport transport = list[index];

					if (ImportExportHelper.IsExport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort))
					{
						return transport;
					}
				}

				return null;
			}
		}

		public Transport FirstImportTransportByLoadAndDischarge()
		{
			var homePort = GlbBranch.CurrentBranch.HomePort;

			for (int i = 0; i <= list.Length - 1; i++)
			{
				if (!ImportExportHelper.IsLocal(list[i].JW_RL_NKLoadPort, homePort) && ImportExportHelper.IsLocal(list[i].JW_RL_NKDiscPort, homePort))
				{
					return list[i];
				}

				if (!ImportExportHelper.IsLocal(list[i].JW_RL_NKLoadPort, homePort) && list[i].JW_RL_NKDiscPort.IsEmpty)
				{
					if (i == list.Length - 1)
					{
						return list[i];
					}
					else if (ImportExportHelper.IsLocal(list[i + 1].JW_RL_NKLoadPort, homePort))
					{
						return list[i];
					}
				}
			}

			return null;
		}

		public List<Transport> GetChain(Transport transport) => transport == null || chains == null ? null : chains.FirstOrDefault(chain => chain.Contains(transport));

		#region Implementation

		class LegOrderComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				Transport lHS = (Transport)x;
				Transport rHS = (Transport)y;

				return lHS.JW_LegOrder - rHS.JW_LegOrder;
			}
		}

		readonly Transport[] list;
		readonly List<List<Transport>> chains;

		#endregion

		#region IEnumerable<Transport> Members

		public IEnumerator<Transport> GetEnumerator()
		{
			return ((IEnumerable<Transport>)this.list).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
