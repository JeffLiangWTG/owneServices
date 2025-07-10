using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class ContainerMovementRelationship : CollectionRelationship
	{
		public ContainerMovementRelationship(AgencyShipmentContainer container)
			: this(container, null) { }

		public ContainerMovementRelationship(AgencyShipmentContainer container, ZQuery additionalFilter)
			: base(typeof(ContainerMovement), additionalFilter)
		{
			if (object.ReferenceEquals(container, null))
			{
				throw new ArgumentNullException(nameof(container));
			}

			this.container = container;

			UpdateFilters();
		}

		public void UpdateFilters()
		{
			if (container.IsDeleted)
			{
				SetFilters(null, ZGuid.Empty);
			}
			else
			{
				SetFilters(GetVoyages(container), GetStock(container));
			}
		}

		public override int GetHashCode()
		{
			return (AdditionalRelationshipFilter == null ? 0 : AdditionalRelationshipFilter.GetHashCode()) ^ container.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			ContainerMovementRelationship other = (ContainerMovementRelationship)obj;

			return other != null
				&& container == other.container
				&& AdditionalRelationshipFilter == other.AdditionalRelationshipFilter;
		}

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				if (stockPK.IsEmpty || voyagePKs == null || voyagePKs.Length == 0)
				{
					return ZQuery.NoResultQuery;
				}
				else
				{
					ZQuery result = new ZQuery();
					result.AddToFilter(JobContainerMoveSchema.E9_JV, voyagePKs);
					result.AddToFilter(JobContainerMoveSchema.E9_R6, stockPK);
					return result;
				}
			}
		}

		static ZGuid[] GetVoyages(AgencyShipmentContainer container)
		{
			AgencyShipment shipment;

			if ((shipment = container.Booking) != null)
			{
				JobSailing sailing;
				JobVoyage voyage;
				List<ZGuid> result = new List<ZGuid>();

				foreach (Transport transport in shipment.TransportsIncludingRelated)
				{
					if (transport.JW_TransportMode != Constants.TransportModes.Sea)
					{
						continue;
					}

					if (!transport.JW_IsLinked)
					{
						continue;
					}

					if ((sailing = transport.Sailing) == null)
					{
						continue;
					}

					if ((voyage = sailing.Voyage) == null)
					{
						continue;
					}

					result.Add(voyage.PK);
				}

				return result.Count == 0 ? null : result.ToArray();
			}
			else
			{
				return null;
			}
		}

		static ZGuid GetStock(AgencyShipmentContainer container)
		{
			RefContainerStock stock = RefContainerStock.Load(container.Factory, container.JC_ContainerNum);
			return stock == null ? ZGuid.Empty : stock.PK;
		}

		void SetFilters(ZGuid[] voyages, ZGuid stock)
		{
			if (voyages != null || this.voyagePKs != null || stock != stockPK)
			{
				voyagePKs = voyages;
				stockPK = stock;
				OnRelationshipFilterChanged(EventArgs.Empty);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly AgencyShipmentContainer container;

		ZGuid[] voyagePKs;
		ZGuid stockPK;
	}
}


