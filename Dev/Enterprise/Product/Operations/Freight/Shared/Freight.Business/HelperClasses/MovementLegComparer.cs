using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public abstract class MovementLegComparer :
		IComparer<IMovementLeg>,
		IComparer<BusinessObject>,
		IComparer
	{
		#region Public

		/// <summary>
		/// Compares two IMovementLeg's according to the dates (and only the dates).
		/// </summary>
		public static IComparer<IMovementLeg> DatesBased
		{
			[DebuggerStepThrough]
			get { return new LegDatesComparer<IMovementLeg>(); }
		}

		/// <summary>
		/// Compares two IMovementLeg objects according to the ports with fallback to the dates
		/// </summary>
		public static MovementLegComparer PortsAndDatesBased(IBusinessObjectCollection relevantLegs)
		{
			return new PortsAndDatesBasedComparer(relevantLegs);
		}

		/// <summary>
		/// Sorts an array of IMovementLeg objects according to the ports with fallback to the dates
		/// </summary>
		public static List<List<T>> SortMovementLegsByPorts<T>(T[] legs) where T : class, IMovementLeg
		{
			AddMovementLegSortDebugLogToTransports(legs);

			var dateSortedLegs = legs.OrderBy(leg => ChainDatesComparer<T>.GetSortKey(new List<T> { leg }), new DatesComparer());
			var chains = GetChains(dateSortedLegs).ToList();
			var sortedLegs = chains.SelectMany(chain => chain).ToList();

			for (var i = 0; i < legs.Length; i++)
			{
				legs[i] = sortedLegs[i];
			}

			return chains;
		}

		static void AddMovementLegSortDebugLogToTransports<T>(T[] legs) where T : class, IMovementLeg
		{
			var sb = new ZStringBuilder().AppendLine(FormattableString.Invariant($"SortMovementLegByPorts for {legs.Length} legs."));
			foreach (var transport in legs.OfType<Transport>())
			{
				sb.AppendLine(FormattableString.Invariant($"Transport ({transport.PK}): IsDeleted == {transport.IsDeleted}, IsInDatabase == {transport.IsInDatabase}"));
			}

			if (legs.OfType<Transport>().Any(transport => transport.IsDeleted))
			{
				sb.AppendLine(FormattableString.Invariant($"Deleted TransportStack Trace:\n {new StackTrace().ToString()}"));
			}

			foreach (var transport in legs.OfType<Transport>())
			{
				transport.MovementLegSortDebugLog = sb.ToString();
			}
		}

		public static T FirstOrDefaultLegForTransportMode<T>(IEnumerable<T> legs, ZString transportMode)
			where T : class, IMovementLeg
		{
			if (legs != null)
			{
				var legsArray = legs.ToArray();
				if (legsArray.Length == 1)
				{
					return legsArray[0];
				}

				if (legsArray.Length > 1)
				{
					SortMovementLegsByPorts(legsArray);
					return legsArray.FirstOrDefault(leg => leg.IsPortToPort() && LegIsOfTransportMode(transportMode, leg, true))
						   ?? legsArray.FirstOrDefault(leg => leg.IsWithinPort() && LegIsOfTransportMode(transportMode, leg, true))
						   ?? legsArray.FirstOrDefault(leg => leg.HasAtLeastOnePort() && LegIsOfTransportMode(transportMode, leg, true))
						   ?? legsArray.FirstOrDefault(leg => LegIsOfTransportMode(transportMode, leg, true))
						   ?? legsArray.FirstOrDefault();
				}
			}

			return null;
		}

		public static T LastOrDefaultLegForTransportMode<T>(IEnumerable<T> legs, ZString transportMode)
			where T : class, IMovementLeg
		{
			if (legs != null)
			{
				var legsArray = legs.ToArray();
				if (legsArray.Length == 1)
				{
					return legsArray[0];
				}

				if (legsArray.Length > 1)
				{
					SortMovementLegsByPorts(legsArray);
					return legsArray.LastOrDefault(leg => leg.IsPortToPort() && LegIsOfTransportMode(transportMode, leg, false))
						   ?? legsArray.LastOrDefault(leg => leg.IsWithinPort() && LegIsOfTransportMode(transportMode, leg, false))
						   ?? legsArray.LastOrDefault(leg => leg.HasAtLeastOnePort() && LegIsOfTransportMode(transportMode, leg, false))
						   ?? legsArray.LastOrDefault(leg => LegIsOfTransportMode(transportMode, leg, false))
						   ?? legsArray.LastOrDefault();
				}
			}

			return null;
		}

		static bool LegIsOfTransportMode<T>(ZString transportMode, T leg, bool isFirst)
			where T : class, IMovementLeg
		{
			return transportMode == leg.TransportMode ||
				   transportMode == Core.Constants.TransportModes.AirSea && leg.TransportMode == (isFirst ? Core.Constants.TransportModes.Air : Core.Constants.TransportModes.Sea) ||
				   transportMode == Core.Constants.TransportModes.SeaAir && leg.TransportMode == (isFirst ? Core.Constants.TransportModes.Sea : Core.Constants.TransportModes.Air);
		}

		#endregion

		#region IComparer<IMovementLeg> Members

		public int Compare(IMovementLeg x, IMovementLeg y)
		{
			return CompareCore(x, y);
		}

		protected abstract int CompareCore(IMovementLeg x, IMovementLeg y);

		#endregion

		#region IComparer<BusinessObject> Members

		int IComparer<BusinessObject>.Compare(BusinessObject x, BusinessObject y)
		{
			return Compare((IMovementLeg)x, (IMovementLeg)y);
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return Compare((IMovementLeg)x, (IMovementLeg)y);
		}

		#endregion

		#region Implementation

		class LegDatesComparer<T> : IComparer<T> where T : class, IMovementLeg
		{
			public int Compare(T x, T y)
			{
				var xList = new List<T> { x };
				var yList = new List<T> { y };

				return new ChainDatesComparer<T>().Compare(xList, yList);
			}
		}

		class ChainDatesComparer<T> : IComparer<List<T>> where T : class, IMovementLeg
		{
			public int Compare(List<T> chain1, List<T> chain2)
			{
				return new DatesComparer().Compare(GetSortKey(chain1), GetSortKey(chain2));
			}

			public static ZDateTime GetSortKey(List<T> chain)
			{
				ZDateTime chainEarliestDate = chain.Aggregate(ZDateTime.Empty, MinDate);
				ZDateTime chainLatestDate = chain.Aggregate(ZDateTime.Empty, MaxDate);
				return GetMiddleDate(chainEarliestDate, chainLatestDate);
			}

			static ZDateTime GetMiddleDate(ZDateTime date1, ZDateTime date2)
			{
				if (date1.IsValid && date2.IsValid)
				{
					return date1.AddTicks((date2 - date1).Ticks / 2);
				}
				else if (date1.IsValid)
				{
					return date2;
				}

				return date1;
			}

			static ZDateTime MinDate(ZDateTime prevMinDate, T leg)
			{
				var dates = new[] { prevMinDate, leg.ArrivalDate, leg.DepartureDate };
				Array.Sort(dates, new DatesComparer());
				return dates.Any(x => x.IsValid) ? dates.Where(x => x.IsValid).First() : ZDateTime.Empty;
			}

			static ZDateTime MaxDate(ZDateTime prevMaxDate, T leg)
			{
				var dates = new[] { prevMaxDate, leg.ArrivalDate, leg.DepartureDate };
				Array.Sort(dates, new DatesComparer());
				return dates.Any(x => x.IsValid) ? dates.Where(x => x.IsValid).Last() : ZDateTime.Empty;
			}
		}

		class DatesComparer : IComparer<ZDateTime>
		{
			public int Compare(ZDateTime date1, ZDateTime date2)
			{
				return date1.IsValid.Equals(date2.IsValid)
						   ? date1.CompareTo(date2)
						   : date2.IsValid.CompareTo(date1.IsValid);
			}
		}

		static IEnumerable<List<T>> GetChains<T>(IEnumerable<T> legs) where T : class, IMovementLeg
		{
			var chains = new List<List<T>>();
			var unchained = new List<T>(legs);

			while (unchained.Count > 0)
			{
				chains.Add(BuildChain(unchained));
			}

			return chains.OrderBy(ChainDatesComparer<T>.GetSortKey, new DatesComparer());
		}

		static List<T> BuildChain<T>(List<T> unchained) where T : class, IMovementLeg
		{
			List<T> chain = new List<T>();
			var validLeg = unchained.FirstOrDefault(x => x.HasAtLeastOnePort());

			if (validLeg != null)
			{
				chain.Add(validLeg);
				unchained.Remove(validLeg);

				while (ConnectToChain(chain, unchained, true) || ConnectToChain(chain, unchained, false))
				{ }
			}
			else
			{
				chain.AddRange(unchained);
				unchained.Clear();
			}

			return chain;
		}

		static bool ConnectToChain<T>(List<T> chain, List<T> unchained, bool localsOnly) where T : class, IMovementLeg
		{
			string chainDischarge = chain[chain.Count - 1].DischargeForSorting();
			string chainLoad = chain[0].LoadForSorting();

			foreach (var leg in unchained)
			{
				if (leg.HasAtLeastOnePort() && (!localsOnly || leg.LoadForSorting() == leg.DischargeForSorting()))
				{
					if (chainDischarge == leg.LoadForSorting())
					{
						chain.Add(leg);
					}
					else
					{
						continue;
					}

					unchained.Remove(leg);
					return true;
				}
			}

			for (int i = unchained.Count - 1; i >= 0; i--)
			{
				var leg = unchained[i];

				if (leg.HasAtLeastOnePort() && (!localsOnly || leg.LoadForSorting() == leg.DischargeForSorting()))
				{
					if (chainLoad == leg.DischargeForSorting())
					{
						chain.Insert(0, leg);
					}
					else
					{
						continue;
					}

					unchained.Remove(leg);
					return true;
				}
			}

			return false;
		}

		#endregion

		#region PortsAndDatesBasedComparer

		class PortsAndDatesBasedComparer : MovementLegComparer
		{
			public PortsAndDatesBasedComparer(IBusinessObjectCollection relivantLegs)
			{
				this.relivantLegs = relivantLegs;
			}

			protected override int CompareCore(IMovementLeg x, IMovementLeg y)
			{
				int xIndex;
				int yIndex;

				if (Lookup.TryGetValue(x, out xIndex) && Lookup.TryGetValue(y, out yIndex))
				{
					return xIndex - yIndex;
				}
				else
				{
					StringBuilder builder = new StringBuilder();
					builder.AppendLine((NoResString)"attempting to compare an unknown element");
					builder.AppendLine();
					AppendMovement(builder, x);
					AppendMovement(builder, y);
					builder.AppendLine();
					AppendMovements(builder, relivantLegs);

					throw new InvalidOperationException(builder.ToString());
				}
			}

			void AppendMovement(StringBuilder builder, IMovementLeg leg)
			{
				if (leg == null)
				{
					builder.Append("null");
				}
				else
				{
					builder.Append(leg.Load);
					builder.Append("(");
					builder.Append(leg.DepartureDate);
					builder.Append(")->");
					builder.Append(leg.Discharge);
					builder.Append("(");
					builder.Append(leg.ArrivalDate);
					builder.AppendLine(")");
				}
			}

			void AppendMovements(StringBuilder builder, IEnumerable legs)
			{
				foreach (IMovementLeg leg in legs)
				{
					AppendMovement(builder, leg);
				}
			}

			Dictionary<IMovementLeg, int> Lookup
			{
				get
				{
					if (lookups == null)
					{
						lookups = new CachedProperty<Dictionary<IMovementLeg, int>>(relivantLegs.Factory, delegate
						{
							IMovementLeg[] legs = new IMovementLeg[relivantLegs.Count];
							relivantLegs.CopyTo(legs, 0);
							SortMovementLegsByPorts(legs);

							Dictionary<IMovementLeg, int> result = new Dictionary<IMovementLeg, int>();
							for (int i = 0; i < legs.Length; i++)
							{
								result[legs[i]] = i;
							}

							return result;
						});
					}

					return lookups.Value;
				}
			}
			CachedProperty<Dictionary<IMovementLeg, int>> lookups;

			readonly IBusinessObjectCollection relivantLegs;
		}

		#endregion
	}
}
