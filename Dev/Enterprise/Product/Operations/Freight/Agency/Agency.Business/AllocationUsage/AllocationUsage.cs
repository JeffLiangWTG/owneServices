using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[System.Diagnostics.DebuggerDisplay("(t,g,r,p,t,v,a) = ({TEU},{GP_TEU},{Reefer_TEU},{PowerPoints},{Tonnes},{Volume},{Area})")]
	public sealed class AllocationUsage
	{
		#region Schema

		public static class Schema
		{
			public const string ParentPK = "ParentPK";
			public const string Total_TEU = "Total_TEU";
			public const string GP_TEU = "GP_TEU";
			public const string Reefer_TEU = "Reefer_TEU";
			public const string PowerPoints = "PowerPoints";
			public const string Tonnes = "Tonnes";
			public const string Volume = "Volume";
			public const string Area = "Area";
		}

		#endregion

		#region Constructors

		AllocationUsage(DynamicBusinessObject row)
		{
			this.ParentPK = new ZGuid(row[Schema.ParentPK]);
			this.TEU = new ZDecimal(row[Schema.Total_TEU]);
			this.GP_TEU = new ZDecimal(row[Schema.GP_TEU]);
			this.Reefer_TEU = new ZDecimal(row[Schema.Reefer_TEU]);
			this.PowerPoints = new ZInt(row[Schema.PowerPoints]);
			this.Tonnes = new ZDecimal(row[Schema.Tonnes]);
			this.Volume = new ZDecimal(row[Schema.Volume]);
			this.Area = new ZDecimal(row[Schema.Area]);
		}

		AllocationUsage(ZGuid parentPK)
		{
			this.ParentPK = parentPK;
		}

		public AllocationUsage() { }

		public AllocationUsage(decimal gp_teu, decimal reefer_teu, int powerPoints, decimal tonnes, decimal volume, decimal area)
			: this(gp_teu, reefer_teu, gp_teu + reefer_teu, powerPoints, tonnes, volume, area) { }

		public AllocationUsage(decimal gp_teu, decimal reefer_teu, decimal total_teu, int powerPoints, decimal tonnes, decimal volume, decimal area)
		{
			this.TEU = total_teu;
			this.GP_TEU = gp_teu;
			this.Reefer_TEU = reefer_teu;
			this.PowerPoints = powerPoints;
			this.Tonnes = tonnes;
			this.Volume = volume;
			this.Area = area;
		}

		#endregion

		public bool HasAspectExceedingThatFor(AllocationUsage usage)
		{
			return GP_TEU > usage.GP_TEU
				|| Reefer_TEU > usage.Reefer_TEU
				|| PowerPoints > usage.PowerPoints
				|| Tonnes > usage.Tonnes
				|| Volume > usage.Volume
				|| Area > usage.Area;
		}

		public bool IsEmpty
		{
			get { return TEU == 0 && GP_TEU == 0 && Reefer_TEU == 0 && PowerPoints == 0 && Tonnes == 0 && Volume == 0 && Area == 0; }
		}

		public ZGuid ParentPK { get; private set; }
		public ZDecimal TEU { get; private set; }
		public ZDecimal GP_TEU { get; private set; }
		public ZDecimal Reefer_TEU { get; private set; }
		public ZInt PowerPoints { get; private set; }
		public ZDecimal Tonnes { get; private set; }
		public ZDecimal Volume { get; private set; }
		public ZDecimal Area { get; private set; }

		public ZDecimal GetAspectValue(string aspectCode)
		{
			switch (aspectCode)
			{
				case AllocationAspectTypes.TEU:
					return TEU;
				case AllocationAspectTypes.PowerPoints:
					return (ZDecimal)PowerPoints;
				case AllocationAspectTypes.Tonnes:
					return Tonnes;
				case AllocationAspectTypes.Volume:
					return Volume;
				case AllocationAspectTypes.Area:
					return Area;
				default:
					throw new ArgumentOutOfRangeException(nameof(aspectCode), aspectCode, "unrecognised aspect code");
			}
		}

		public void Add(AllocationUsage other)
		{
			this.TEU += other.GP_TEU + other.Reefer_TEU;
			this.GP_TEU += other.GP_TEU;
			this.Reefer_TEU += other.Reefer_TEU;
			this.PowerPoints += other.PowerPoints;
			this.Tonnes += other.Tonnes;
			this.Volume += other.Volume;
			this.Area += other.Area;
		}

		public static AllocationUsage LoadForSailing(JobSailing sailing, ZGuid principalPK, AgencyShipment shipmentToExclude)
		{
			if (sailing == null)
			{
				throw new ArgumentNullException(nameof(sailing));
			}

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(sailing.Factory);
			const string query = @"SELECT * FROM TotalBookedBySailing(@Sailing, @Principal, @ShipmentToExclude)";

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			queryParams.Add("@Sailing", sailing.PK, JobSailingSchema.PK);
			queryParams.Add("@Principal", principalPK.IsEmpty ? null : principalPK, JobShipmentSchema.JS_OH_DeliveryAgent);
			queryParams.Add("@ShipmentToExclude", shipmentToExclude == null ? ZGuid.Empty : shipmentToExclude.PK, JobShipmentSchema.PK);
			collection.Load(query, queryParams);

			return collection.Count > 0 ? new AllocationUsage(collection[0]) : new AllocationUsage(sailing.PK);
		}
		public static AllocationUsage[] LoadForSailing(JobVoyage voyage, ZGuid principalPK)
		{
			if (voyage == null)
			{
				throw new ArgumentNullException(nameof(voyage));
			}

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(voyage.Factory);
			const string query = @"
SELECT
	Totals.*
FROM
	dbo.JobSailing
	JOIN dbo.JobVoyDestination ON JB_PK = JX_JB
	OUTER APPLY TotalBookedBySailing(JX_PK, @Principal, null) AS Totals
WHERE
	JB_JV = @Voyage
	AND Totals.ParentPK is NOT NULL
";

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			queryParams.Add("@Voyage", voyage.PK, JobVoyageSchema.PK);
			queryParams.Add("@Principal", principalPK.IsEmpty ? null : principalPK, JobShipmentSchema.JS_OH_DeliveryAgent);
			collection.Load(query, queryParams);

			AllocationUsage[] result = new AllocationUsage[collection.Count];

			for (int count = 0; count < collection.Count; count++)
			{
				result[count] = new AllocationUsage(collection[count]);
			}

			return result;
		}

		public static AllocationUsage LoadForOrigin(VoyageOrigin origin, ZGuid principalPK, AgencyShipment shipmentToExclude)
		{
			if (origin == null)
			{
				throw new ArgumentNullException(nameof(origin));
			}

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(origin.Factory);
			const string query = "SELECT * FROM TotalBookedByOrigin(@Origin, @Principal, @ShipmentToExclude)";

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			queryParams.Add("@Origin", origin.PK, JobVoyOriginSchema.PK);
			queryParams.Add("@Principal", principalPK.IsEmpty ? null : principalPK, JobShipmentSchema.JS_OH_DeliveryAgent);
			queryParams.Add("@ShipmentToExclude", shipmentToExclude == null ? ZGuid.Empty : shipmentToExclude.PK, JobShipmentSchema.PK);
			collection.Load(query, queryParams);

			return collection.Count > 0 ? new AllocationUsage(collection[0]) : new AllocationUsage(origin.PK);
		}

		public static AllocationUsage LoadForOrigin(JobSailing sailing, ZGuid principalPK, AgencyShipment shipmentToExclude)
		{
			if (sailing == null)
			{
				throw new ArgumentNullException(nameof(sailing));
			}

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(sailing.Factory);
			const string query = "SELECT * FROM MaxBookedBySailing(@Voyage, @Sailing, @Country, @ETD, @ETA, @Principal, @ShipmentToExclude)";

			ZSqlParameterCollection queryParams = new ZSqlParameterCollection();
			queryParams.Add("@Voyage", sailing.Voyage.PK, JobVoyageSchema.PK);
			queryParams.Add("@Sailing", sailing.PK, JobSailingSchema.PK);
			queryParams.Add("@Country", sailing.Origin.JA_RL_NKPortOfLoading.Left(2), JobVoyOriginSchema.JA_RL_NKPortOfLoading);
			queryParams.Add("@ETD", sailing.Origin.JA_E_DEP, JobVoyOriginSchema.JA_E_DEP);
			queryParams.Add("@ETA", sailing.Destination.JB_E_ARV, JobVoyDestinationSchema.JB_E_ARV);
			queryParams.Add("@Principal", principalPK.IsEmpty ? null : principalPK, JobShipmentSchema.JS_OH_DeliveryAgent);
			queryParams.Add("@ShipmentToExclude", shipmentToExclude == null ? ZGuid.Empty : shipmentToExclude.PK, JobShipmentSchema.PK);
			collection.Load(query, queryParams);

			return collection.Count > 0 ? new AllocationUsage(collection[0]) : new AllocationUsage();
		}

		public static AllocationUsage LoadFromShipment(AgencyShipment shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			AllocationUsage result = new AllocationUsage(shipment.Sailing == null ? ZGuid.Empty : shipment.Sailing.PK);

			if (shipment.JS_PackingMode == Constants.ContainerModes.FCL)
			{
				foreach (AgencyShipmentContainer container in shipment.ShippingContainers)
				{
					if (container.Container != null)
					{
						if (container.Container.RC_ContainerType == Constants.ContainerTypes.Refrigerated)
						{
							result.Reefer_TEU += container.JC_ContainerCount * container.Container.RC_TEU;
							result.PowerPoints += container.JC_ContainerCount;
						}
						else
						{
							result.GP_TEU += container.JC_ContainerCount * container.Container.RC_TEU;
						}
					}

					if (Constants.Weight.ContainsCode(container.JC_GrossWeightUQ))
					{
						result.Tonnes += Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, Constants.Weight.Tonnes);
					}
				}
			}
			else
			{
				if (shipment.JS_PackingMode == Constants.ContainerModes.BreakBulk ||
					shipment.JS_PackingMode == Constants.ContainerModes.RollOnRollOff)
				{
					foreach (AgencyShipmentContainer container in shipment.ShippingContainers)
					{
						if (!Constants.Length.ContainsCode(container.JC_TotalUnitOfMeasure))
						{
							result.Area = 0;
							break;
						}

						var area = container.JC_TotalLength * container.JC_TotalWidth * container.JC_ContainerCount;
						var areaUnit = Constants.Area.GetAreaUnitForLengthUnit(container.JC_TotalUnitOfMeasure);
						result.Area += Constants.Area.Convert(area, areaUnit, Constants.Area.SquareMetre);
					}
				}

				result.Volume = Constants.Volume.ContainsCode(shipment.JS_UnitOfVolume) ? Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, Constants.Volume.CubicMetres) : 0;
				result.Tonnes = Constants.Weight.ContainsCode(shipment.JS_UnitOfWeight) ? Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Tonnes) : 0;
			}

			result.TEU = result.Reefer_TEU + result.GP_TEU;
			return result;
		}

		public static AllocationUsageSet LoadRelevantToSailing(JobSailing sailing, ZGuid principalPK, AgencyShipment shipmentToExclude)
		{
			if (sailing == null)
			{
				throw new ArgumentNullException(nameof(sailing));
			}

			AllocationUsageSet result;
			SlotAllocation allocation;

			VoyageOrigin origin = sailing.Origin;

			switch (origin.VoyageCountry.J0_AllocationMethod)
			{
				case AllocationMethodList.Codes.Country:
					allocation = origin.VoyageCountry.SlotAllocations.GetAllocation(principalPK);
					result = new AllocationUsageSet(LoadForOrigin(sailing, principalPK, shipmentToExclude), allocation);
					break;

				case AllocationMethodList.Codes.Origin:
					allocation = origin.SlotAllocations.GetAllocation(principalPK);
					result = new AllocationUsageSet(LoadForOrigin(origin, principalPK, shipmentToExclude), allocation);
					break;

				case AllocationMethodList.Codes.Sailing:
					allocation = sailing.SlotAllocations.GetAllocation(principalPK);
					result = new AllocationUsageSet(LoadForSailing(sailing, principalPK, shipmentToExclude), allocation);
					break;

				default:
					result = null;
					break;
			}

			return result;
		}

		public static AllocationUsage Max(IEnumerable<AllocationUsage> usages)
		{
			AllocationUsage result = new AllocationUsage();

			foreach (AllocationUsage usage in usages)
			{
				result.TEU = Math.Max(result.TEU, usage.TEU);
				result.GP_TEU = Math.Max(result.GP_TEU, usage.GP_TEU);
				result.Reefer_TEU = Math.Max(result.Reefer_TEU, usage.Reefer_TEU);
				result.PowerPoints = Math.Max(result.PowerPoints, usage.PowerPoints);
				result.Tonnes = Math.Max(result.Tonnes, usage.Tonnes);
				result.Volume = Math.Max(result.Volume, usage.Volume);
				result.Area = Math.Max(result.Area, usage.Area);
			}

			return result;
		}

		public static AllocationUsage Sum(IEnumerable<AllocationUsage> usages)
		{
			AllocationUsage result = new AllocationUsage();

			foreach (AllocationUsage usage in usages)
			{
				result.TEU += usage.TEU;
				result.GP_TEU += usage.GP_TEU;
				result.Reefer_TEU += usage.Reefer_TEU;
				result.PowerPoints += usage.PowerPoints;
				result.Tonnes += usage.Tonnes;
				result.Volume += usage.Volume;
				result.Area += usage.Area;
			}

			return result;
		}
	}
}
