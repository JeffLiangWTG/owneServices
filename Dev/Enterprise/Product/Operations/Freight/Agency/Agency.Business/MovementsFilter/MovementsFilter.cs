using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class MovementsFilter : AutoMovementsFilter
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MovementsFilter(BusinessObjectFactory factory, ICollectionRelationship baseRelationship)
			: base(factory)
		{
			Reset();
			relationship = new MovementsFilterRelationship(baseRelationship);
		}

		#region Properties

		[List("Lookups.MovementTypes")]
		public override ZString MovementType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.MovementType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.MovementType = value; }
		}

		[List("Lookups.Vessels")]
		public override ZString Vessel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Vessel; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Vessel = value; }
		}

		public override ZDateTime FromDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.FromDate; }
			set
			{
				base.FromDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateToDate();
				}
			}
		}

		public override ZDateTime ToDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ToDate; }
			set
			{
				base.ToDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFromDate();
				}
			}
		}

		#endregion

		#region Related Business Objects

		public ContainerMovementCollection Movements
		{
			get
			{
				if (movements == null)
				{
					relationship.Load(Factory, GenerateFilter());
					movements = new ContainerMovementCollection(Factory, true, relationship);
				}
				return movements;
			}
		}
		ContainerMovementCollection movements;

		#endregion

		#region Operations

		public void Find()
		{
			relationship.Load(Factory, GenerateFilter());
		}

		public void Reset()
		{
			int days = AgencyRegistry.Instance.MovementArchiveDays.Value;

			MovementType = ZString.Empty;
			FromDate = days == 0 ? ZDateTime.Empty : ZDateTime.Today.AddDays(-days);
			ToDate = ZDateTime.Empty;
			DepotPK_ZAddress.OrgPK = ZGuid.Empty;
			Vessel = ZString.Empty;
			VoyageNo = ZString.Empty;
		}

		public void SetToShow(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			AdjustFiltersToIncludeMovementType(movement.E9_MovementType);
			AdjustFiltersToIncludeDateTime(movement.E9_MovementDate);
			AdjustFiltersToIncludeDepot(movement.Depot);
			AdjustFiltersToIncludeVoyage(movement.Voyage);
		}

		#endregion

		#region Strategies

		public MovementsFilterLookups Lookups
		{
			get { return lookups ?? (lookups = new MovementsFilterLookups(this)); }
		}
		MovementsFilterLookups lookups;

		#endregion

		#region Implementation

		ZQuery GenerateFilter()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(GenerateMovementTypeFilter());
			filter.AddToFilter(GenerateMovementDateFilter());
			filter.AddToFilter(GenerateDepotFilter());
			filter.AddToFilter(GenerateVoyageVesselFilter());
			return filter;
		}
		ZQuery GenerateMovementTypeFilter()
		{
			if (!MovementType.IsEmpty)
			{
				return new ZQuery(JobContainerMoveSchema.E9_MovementType, MovementType);
			}
			else
			{
				return new ZQuery();
			}
		}
		ZQuery GenerateMovementDateFilter()
		{
			ZQuery dateRangeFilter = new ZQuery();

			if (FromDate.IsValidSmallDateTime)
			{
				dateRangeFilter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, FromDate);
			}

			if (ToDate.IsValidSmallDateTime)
			{
				dateRangeFilter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ToDate);
			}

			if (dateRangeFilter.IsEmpty)
			{
				return dateRangeFilter;
			}
			else
			{
				ZQuery result = new ZQuery();
				result.DefaultJoinCondition = JoinCondition.Or;
				result.AddToFilter(dateRangeFilter);
				result.AddToFilter(JobContainerMoveSchema.E9_MovementDate, ZDateTime.Empty);
				return result;
			}
		}
		ZQuery GenerateDepotFilter()
		{
			if (DepotPK.IsValid)
			{
				return new ZQuery(JobContainerMoveSchema.E9_OA_Depot, DepotPK);
			}
			else if (DepotPK_ZAddress.OrgPK.IsValid)
			{
				OrgAddress[] addresses = Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, DepotPK_ZAddress.OrgPK));
				return new ZQuery(JobContainerMoveSchema.E9_OA_Depot, Array.ConvertAll(addresses, (a) => a.PK));
			}
			else
			{
				return new ZQuery();
			}
		}
		ZQuery GenerateVoyageVesselFilter()
		{
			if (Vessel.IsEmpty && VoyageNo.IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				ZQuery voyageFilter = new ZQuery();

				if (!Vessel.IsEmpty)
				{
					voyageFilter.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, Vessel);
				}

				if (!VoyageNo.IsEmpty)
				{
					voyageFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, VoyageNo);
				}

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JobVoyage[] voyages = newFactory.Load<JobVoyage>(voyageFilter);

				return new ZQuery(JobContainerMoveSchema.E9_JV, Array.ConvertAll(voyages, (v) => v.PK));
			}
		}

		void AdjustFiltersToIncludeMovementType(ZString movementType)
		{
			if (!MovementType.IsEmpty && MovementType != movementType)
			{
				MovementType = movementType;
			}
		}
		void AdjustFiltersToIncludeDateTime(ZDateTime dateTime)
		{
			if (!dateTime.IsEmpty)
			{
				ZDateTime now = ZDateTime.Now;

				if (!FromDate.IsEmpty && FromDate > dateTime)
				{
					ZDateTime newToDate;
					TimeSpan diff = (dateTime - FromDate);

					if (ToDate.IsEmpty)
					{
						int days = AgencyRegistry.Instance.MovementArchiveDays.Value;
						newToDate = (days == 0 ? now : FromDate.AddDays(days)).Add(diff).AddHours(-1);
					}
					else
					{
						newToDate = ToDate.Add(diff).AddHours(-1);
					}

					if (newToDate > now)
					{
						newToDate = ZDateTime.Empty;
					}
					else if (newToDate < dateTime)
					{
						newToDate = dateTime.AddHours(1);
					}

					FromDate = FromDate.Add(diff).AddHours(-1);
					ToDate = newToDate;
				}
				else if (!ToDate.IsEmpty && ToDate < dateTime)
				{
					ZDateTime newFromDate;
					TimeSpan diff = (dateTime - ToDate);

					if (FromDate.IsEmpty)
					{
						int days = AgencyRegistry.Instance.MovementArchiveDays.Value;
						newFromDate = ToDate.AddDays(days == 0 ? -10 : -days).Add(diff).AddHours(1);
					}
					else
					{
						newFromDate = FromDate.Add(diff).AddHours(1);
					}

					if (newFromDate > dateTime)
					{
						newFromDate = dateTime.AddHours(1);
					}

					FromDate = newFromDate;
					ToDate = ToDate.Add(diff).AddHours(1);
				}
			}
		}
		void AdjustFiltersToIncludeDepot(OrgAddress depot)
		{
			if (depot == null)
			{
				DepotPK_ZAddress.OrgPK = ZGuid.Empty;
			}
			else if (!DepotPK.IsEmpty)
			{
				if (DepotPK != depot.PK)
				{
					DepotPK = depot.PK;
				}
			}
			else if (!DepotPK_ZAddress.OrgPK.IsEmpty && depot.OA_OH != DepotPK_ZAddress.OrgPK)
			{
				DepotPK_ZAddress.OrgPK = depot.OA_OH;
				DepotPK = ZGuid.Empty;
			}
		}
		void AdjustFiltersToIncludeVoyage(JobVoyage voyage)
		{
			if (voyage == null)
			{
				Vessel = "";
				VoyageNo = "";
			}
			else
			{
				if (!Vessel.IsEmpty && Vessel != voyage.JV_RV_NKVessel)
				{
					Vessel = voyage.JV_RV_NKVessel;
				}

				if (!VoyageNo.IsEmpty && VoyageNo != voyage.JV_RV_NKVessel)
				{
					VoyageNo = voyage.JV_VoyageFlight;
				}
			}
		}

		readonly MovementsFilterRelationship relationship;

		#endregion
	}
}


