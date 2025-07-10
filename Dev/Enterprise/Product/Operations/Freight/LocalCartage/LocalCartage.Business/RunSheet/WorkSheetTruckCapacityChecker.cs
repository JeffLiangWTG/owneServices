using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class WorkSheetTruckCapacityChecker
	{
		public WorkSheetTruckCapacityChecker(CommonWorkSheet workSheet)
		{
			WorkSheet = workSheet;
		}

		/// <summary>
		/// Checks the worksheet's truck's weight and volume capacity with the cartage legs belonging to the worksheet. Returns single string containing all errors.
		/// </summary>
		public ZString CheckTruckCapacity()
		{
			ZString result = "";

			if (WorkSheet != null)
			{
				LegsToUseForCapacityCheck = GetLegsInStartTimeOrder(new List<CommonCartageLeg>(WorkSheet.CartageLegs));
				result = CheckCapacityWithLegs();
			}

			return result;
		}

		/// <summary>
		/// Checks the worksheet's truck's weight and volume capacity with the cartage legs belonging to the worksheet 
		/// together with the additional leg passed in. Leg passed in doesn't get added to the CartageLegs collection of the worksheet. 
		/// Return only the error for the passed in leg.
		/// </summary>
		public ZString CheckTruckCapacity(CommonCartageLeg additionalLeg)
		{
			ZString result = "";

			if (additionalLeg != null && WorkSheet != null)
			{
				CapacityErrorsByLegPK.Clear();

				List<CommonCartageLeg> legs = new List<CommonCartageLeg>(WorkSheet.CartageLegs);
				legs.Add(additionalLeg);
				LegsToUseForCapacityCheck = GetLegsInStartTimeOrder(legs);

				CheckCapacityWithLegs();

				if (CapacityErrorsByLegPK.ContainsKey(additionalLeg.PK))
				{
					result = CapacityErrorsByLegPK[additionalLeg.PK];
				}
			}

			return result;
		}

		CommonCartageLeg[] GetLegsInStartTimeOrder(List<CommonCartageLeg> legs)
		{
			CommonCartageLeg[] result = legs.ToArray();
			Array.Sort(result, CompareLegsByTime);
			return result;
		}

		int CompareLegsByTime(CommonCartageLeg lhs, CommonCartageLeg rhs)
		{
			int result;

			if (lhs.StartTime == rhs.StartTime)
			{
				result = lhs.EndTime.CompareTo(rhs.EndTime);
			}
			else
			{
				result = lhs.StartTime.CompareTo(rhs.StartTime);
			}

			return result;
		}

		ZString CheckCapacityWithLegs()
		{
			ZStringBuilder result = new ZStringBuilder();

			if (WorkSheet.Truck != null && (WorkSheet.Truck.RQ_WeightCapacity != 0 || WorkSheet.Truck.RQ_CubicCapacity != 0))
			{
				CapacityErrorsByLegPK.Clear();

				foreach (CommonCartageLeg leg in LegsToUseForCapacityCheck)
				{
					result.AppendIfNotEmpty(CheckTruckCapacityExceededBy(leg));
				}

				return result.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			}

			return "";
		}

		CommonCartageLeg[] LegsToUseForCapacityCheck
		{ get; set; }

		Dictionary<ZGuid, string> CapacityErrorsByLegPK
		{
			get
			{
				if (fCapacityErrorsByLegPK == null)
				{
					fCapacityErrorsByLegPK = new Dictionary<ZGuid, string>();
				}

				return fCapacityErrorsByLegPK;
			}
		}

		Dictionary<ZGuid, string> fCapacityErrorsByLegPK;

		ZString CheckTruckCapacityExceededBy(CommonCartageLeg leg)
		{
			ZStringBuilder result = new ZStringBuilder();

			CommonCartageLeg[] legsInSameTime = GetAllLegsInSamePeriod(leg);

			bool allOfTheOverlappingLegsHavenBeenReportedBefore = true;

			foreach (CommonCartageLeg legInSameTime in legsInSameTime)
			{
				if (!CapacityErrorsByLegPK.ContainsKey(legInSameTime.PK))
				{
					allOfTheOverlappingLegsHavenBeenReportedBefore = false;
					break;
				}
			}

			if (!allOfTheOverlappingLegsHavenBeenReportedBefore)
			{
				result.AppendIfNotEmpty(CheckCapacity(leg, CommonCartageLeg.Schema.TotalWeight));
				result.AppendIfNotEmpty(CheckCapacity(leg, CommonCartageLeg.Schema.TotalVolume));
			}

			return result.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
		}

		ZString CheckCapacity(CommonCartageLeg leg, string col)
		{
			ZString result = "";

			decimal capacity = col == CommonCartageLeg.Schema.TotalWeight ? WorkSheet.Truck.RQ_WeightCapacity : WorkSheet.Truck.RQ_CubicCapacity;

			if (capacity != 0)
			{
				CommonCartageLeg[] legsInSameTime = GetAllLegsInSamePeriod(leg);

				decimal total = col == CommonCartageLeg.Schema.TotalWeight ? TotalCalculation.GetTotalWeight(legsInSameTime, CommonCartageLeg.Schema.TotalWeight, CommonCartageLeg.Schema.TotalWeightUnit, WorkSheet.Truck.RQ_WeightUnit)
					: TotalCalculation.GetTotalVolume(legsInSameTime, CommonCartageLeg.Schema.TotalVolume, CommonCartageLeg.Schema.TotalVolumeUnit, WorkSheet.Truck.RQ_CubicUnit);

				if (total > capacity)
				{
					ZDateTime startTime;
					ZDateTime endTime;
					GetTimeIntersection(legsInSameTime, out startTime, out endTime);

					ZString capacityUnit = col == CommonCartageLeg.Schema.TotalWeight ? WorkSheet.Truck.RQ_WeightUnit : WorkSheet.Truck.RQ_CubicUnit;
					decimal difference = col == CommonCartageLeg.Schema.TotalWeight ? total - WorkSheet.Truck.RQ_WeightCapacity : total - WorkSheet.Truck.RQ_CubicCapacity;

					ZStringBuilder legIDs = new ZStringBuilder();

					foreach (CommonCartageLeg offender in legsInSameTime)
					{
						legIDs.Append(offender.UniqueIDWithJobNumber);
					}

					ZString capacityName = col == CommonCartageLeg.Schema.TotalWeight ? Res.GetString("3a899ade-8462-4381-a1d0-843cea428c30", "Weight Capacity") + " " : Res.GetString("81d01853-f683-43f5-8f57-ea60d67483cb", "Cubic Capacity") + " ";

					ZString legDefinition = legsInSameTime.Length == 1 ? " " + Res.GetString("d917e54e-a78f-4e6f-902a-76eb2bc74f59", "The Port Transport Leg in this period is") + " " : " " + Res.GetString("3dd7fb71-d720-4a30-ac0f-78d58ce2bef7", "The Port Transport Legs in this period are") + " ";

					result = Res.GetString("2753803d-f832-4130-8eb0-f5d03952e5c0", "Between {0} and {1}, the vehicle's {2} of {3} {4} has been exceeded by {5}. {6} {7}", startTime, endTime, capacityName, capacity, capacityUnit, difference, legDefinition, legIDs.ToStringWithDelimiterBetweenAppends(","));

					foreach (CommonCartageLeg offender in legsInSameTime)
					{
						if (!CapacityErrorsByLegPK.ContainsKey(offender.PK))
						{
							CapacityErrorsByLegPK.Add(offender.PK, result);
						}
						else
						{
							CapacityErrorsByLegPK[offender.PK] = CapacityErrorsByLegPK[offender.PK] + System.Environment.NewLine + result;
						}
					}
				}
			}

			return result;
		}

		void GetTimeIntersection(CommonCartageLeg[] legsInSameTime, out ZDateTime startTime, out ZDateTime endTime)
		{
			startTime = ZDateTime.MinSmallDateTimeValue;
			endTime = ZDateTime.MaxSmallDateTimeValue;

			foreach (CommonCartageLeg offendingLeg in legsInSameTime)
			{
				if (offendingLeg.StartTime > startTime)
				{
					startTime = offendingLeg.StartTime;
				}
				if (offendingLeg.EndTime < endTime)
				{
					endTime = offendingLeg.EndTime;
				}
			}
		}

		CommonCartageLeg[] GetAllLegsInSamePeriod(CommonCartageLeg leg)
		{
			List<CommonCartageLeg> result = new List<CommonCartageLeg>();
			result.Add(leg);

			foreach (CommonCartageLeg otherLeg in LegsToUseForCapacityCheck)
			{
				if (otherLeg.PK != leg.PK && (otherLeg.StartTime >= leg.StartTime && otherLeg.StartTime < leg.EndTime))
				{
					result.Add(otherLeg);
				}
			}

			return result.ToArray();
		}

		CommonWorkSheet WorkSheet
		{ get; set; }
	}
}
