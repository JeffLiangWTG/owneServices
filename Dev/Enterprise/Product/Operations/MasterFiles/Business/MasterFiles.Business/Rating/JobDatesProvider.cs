using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobDatesProvider<T> : IJobDatesProvider
		where T : class
	{
		protected T Parent { get; private set; }

		public JobDatesProvider(T parent)
		{
			Argument.NotNull(parent, "parent");
			Parent = parent;
		}

		#region Properties

		IList<ZDateTime> RatingDates
		{
			get
			{
				if (autoRatingDates == null)
				{
					var allRatingDates = new List<ZDateTime>
					{
						GetArrivalDateCore(), GetDepartureDateCore(),  GetEstimatedArrivalDateCore(), GetEstimatedDepartureDateCore(), GetAWBIssueDateCore(),
						GetPickupDateCore(), GetDeliveryDateCore(), GetVesselArrivalDateCore(), GetVesselDepartureDateCore(), GetHouseBillIssueDateCore(),
						GetJobOpenDateCore(), GetFirstContainerGateInDateCore(), GetLastContainerGateInDateCore(), GetCFSReceivalStartDateCore(),
						GetHBLPlaceOfReceiptArrivalDateCore(), InterimReceiptDateCore(), GetYardInDateOverrideCore(), GetYardOutDateOverrideCore(),
						GetGateInDateOverrideCore(), GetGateOutDateOverrideCore(), GetCostingAutoratingDateOverrideCore(), GetRevenueAutoratingDateOverrideCore()
					};

					autoRatingDates = new List<ZDateTime>(allRatingDates.Where(x => x.IsValid && !x.IsEmpty));
				}

				return autoRatingDates;
			}
		}
		IList<ZDateTime> autoRatingDates;

		#endregion

		protected ZDateTime GetEarliestValidDate(IEnumerable<ZDateTime> dates)
		{
			return dates.Where(date => !date.IsEmpty && date.IsValid).OrderBy(x => x).FirstOrDefault();
		}

		protected ZDateTime GetLatestValidDate(IEnumerable<ZDateTime> dates)
		{
			return dates.Where(date => !date.IsEmpty && date.IsValid).OrderByDescending(x => x).FirstOrDefault();
		}

		#region IJobDatesProvider members

		public ZDate EarliestPossibleDate
		{
			get { return RatingDates.Any() ? RatingDates.Min().Date : ZDate.Today; }
		}

		public ZDate LatestPossibleDate
		{
			get { return RatingDates.Any() ? RatingDates.Max().Date : ZDate.Today; }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZDateTime GetJobDateByType(ZString jobDateType, string direction = "")
		{
			switch (jobDateType)
			{
				case JobDateTypes.Codes.ArrivalDate:
					return GetArrivalDateCore();

				case JobDateTypes.Codes.DepartureDate:
					return GetDepartureDateCore();

				case JobDateTypes.Codes.EstimatedArrivalDate:
					return GetEstimatedArrivalDateCore();

				case JobDateTypes.Codes.EstimatedDepartureDate:
					return GetEstimatedDepartureDateCore();

				case JobDateTypes.Codes.AWBIssueDate:
					return GetAWBIssueDateCore();

				case JobDateTypes.Codes.CustomsClearanceDate:
					return string.IsNullOrEmpty(direction) ? GetCustomsClearanceDateCore() : GetCustomsClearanceDateByDirectionCore(direction);

				case JobDateTypes.Codes.PickupDate:
					return GetPickupDateCore();

				case JobDateTypes.Codes.DeliveryDate:
					return GetDeliveryDateCore();

				case JobDateTypes.Codes.VesselArrivalDate:
					return GetVesselArrivalDateCore();

				case JobDateTypes.Codes.VesselDepartureDate:
					return GetVesselDepartureDateCore();

				case JobDateTypes.Codes.HouseBillIssueDate:
					return GetHouseBillIssueDateCore();

				case JobDateTypes.Codes.JobOpenDate:
					return GetJobOpenDateCore();

				case JobDateTypes.Codes.FirstContainerGateInDate:
					return GetFirstContainerGateInDateCore();

				case JobDateTypes.Codes.LastContainerGateInDate:
					return GetLastContainerGateInDateCore();

				case JobDateTypes.Codes.CFSReceivalStartDate:
					return GetCFSReceivalStartDateCore();

				case JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate:
					return GetHBLPlaceOfReceiptArrivalDateCore();

				case JobDateTypes.Codes.InterimReceiptDate:
					return InterimReceiptDateCore();

				case JobDateTypes.Codes.YardInDate:
					return GetYardInDateOverrideCore();

				case JobDateTypes.Codes.YardOutDate:
					return GetYardOutDateOverrideCore();

				case JobDateTypes.Codes.GateInDate:
					return GetGateInDateOverrideCore();

				case JobDateTypes.Codes.GateOutDate:
					return GetGateOutDateOverrideCore();

				case JobDateTypes.Codes.CostingAutoratingDateOverride:
					return GetCostingAutoratingDateOverrideCore();

				case JobDateTypes.Codes.RevenueAutoratingDateOverride:
					return GetRevenueAutoratingDateOverrideCore();

				default:
					return ZDateTime.Empty;
			}
		}

		public virtual string TransitTime
		{
			get
			{
				var jobArrivalDate = GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
				var jobDepartureDate = GetJobDateByType(JobDateTypes.Codes.DepartureDate);
				if (jobArrivalDate.IsValid && jobDepartureDate.IsValid && !jobArrivalDate.IsEmpty && !jobDepartureDate.IsEmpty)
				{
					var result = (int)(jobArrivalDate - jobDepartureDate).TotalDays;
					return result.ToString(CultureInfo.InvariantCulture);
				}

				return string.Empty;
			}
		}

		#endregion

		protected virtual ZDateTime GetArrivalDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetDepartureDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetEstimatedArrivalDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetEstimatedDepartureDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetAWBIssueDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetCustomsClearanceDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetCustomsClearanceDateByDirectionCore(string direction)
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetPickupDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetDeliveryDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetVesselDepartureDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetVesselArrivalDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetHouseBillIssueDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetJobOpenDateCore()
		{
			return (Parent as IJobInvoicingPlugIn)?.InvoicingSupporter?.Job?.JH_A_JOP ?? ZDate.Empty;
		}

		protected virtual ZDateTime GetFirstContainerGateInDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetLastContainerGateInDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetCFSReceivalStartDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetHBLPlaceOfReceiptArrivalDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime InterimReceiptDateCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetYardInDateOverrideCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetYardOutDateOverrideCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetGateInDateOverrideCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetGateOutDateOverrideCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetCostingAutoratingDateOverrideCore()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetRevenueAutoratingDateOverrideCore()
		{
			return ZDateTime.Empty;
		}
	}
}
