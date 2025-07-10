using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IRatingRoute<out T>
		where T : IRoutingSupport
	{
		T Parent { get; }
		ILocation Origin { get; }
		ILocation Destination { get; }
		ILocation GetVia(CostSell costOrSell);

		/// <summary>
		/// Refers to Consol > 1st Load
		/// </summary>
		ILocation GetFirstLoad(CostSell costOrSell);
		/// <summary>
		/// Refers to Consol > Last Discharge
		/// </summary>
		ILocation GetLastDischarge(CostSell costOrSell);
		/// <summary>
		/// Refers to First Load Port of the First Route Set with Transport Mode same as Transport Mode on Consol
		/// </summary>
		ILocation GetFirstRouteSetLoad(CostSell costOrSell);
		/// <summary>
		/// Refers to Last Discharge Port of the Last Route Set with Transport Mode same as Transport Mode on Last Consol
		/// </summary>
		ILocation GetLastRouteSetDischarge(CostSell costOrSell);
		OrgHeader Carrier { get; }
		Creditors Creditors { get; }
		ZString TransportMode { get; }
		IJobDatesProvider JobDatesProvider { get; }
		ZInt RouteSetNumber { get; }
		ZBool SupportsManualRateSelection { get; set; }
	}

	public abstract class RatingRoute<T> : IRatingRoute<T>
		where T : IRoutingSupport
	{
		protected RatingRoute(T parent)
		{
			this.parent = parent;
		}

		public T Parent
		{
			get { return parent; }
		}

		protected readonly T parent;

		public abstract ILocation Origin { get; }
		public abstract ILocation Destination { get; }
		public abstract ILocation GetVia(CostSell costOrSell);
		public abstract ILocation GetFirstLoad(CostSell costOrSell);
		public abstract ILocation GetLastDischarge(CostSell costOrSell);
		public abstract ILocation GetFirstRouteSetLoad(CostSell costOrSell);
		public abstract ILocation GetLastRouteSetDischarge(CostSell costOrSell);
		public abstract OrgHeader Carrier { get; }
		public abstract Creditors Creditors { get; set; }
		public abstract ZString TransportMode { get; }
		public abstract IJobDatesProvider JobDatesProvider { get; }
		public virtual ZInt RouteSetNumber
		{
			get { return 1; }
		}

		public abstract ZBool SupportsManualRateSelection { get; set; }
	}
}
