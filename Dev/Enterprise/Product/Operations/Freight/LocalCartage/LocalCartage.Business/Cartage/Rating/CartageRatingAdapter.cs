using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.LocalCartage.Business
{
	public abstract class CartageRatingAdapter :
		RatingAdapter,
		IJobNumber,
		IAutoRatingFreightConditionsSupportable
	{
		protected CartageRatingAdapter(CommonBookedCtgMove move, JobDocAddress pickup, JobDocAddress delivery)
		{
			Move = move;
			Pickup = pickup;
			Delivery = delivery;
		}

		protected readonly CommonBookedCtgMove Move;
		protected readonly JobDocAddress Pickup;
		protected readonly JobDocAddress Delivery;

		public abstract CommonCartageLeg FirstCartageLeg { get; }

		public abstract CommonCartageLeg LastCartageLeg { get; }

		public override ZString OperationalJobCode => (Move?.Cartage?.JJ_ConsignmentID).GetValueOrDefault();

		public override AdapterType AdapterType => AdapterType.PortTransport;

		public override ZString JobID => (Move?.Cartage?.JJ_ConsignmentID).GetValueOrDefault();

		public override IJobInvoicingSupporter InvoicingSupporter => Move?.Cartage?.InvoicingSupporter;

		public override IJobDatesProvider JobDatesProvider => new CartageJobDatesProvider(this);

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				if (Move != null)
				{
					if (!Constants.Weight.ContainsCode(Move.EW_WeightUQ))
					{
						return new AutoRatingStatusInfo(false, Res.GetString("3a9732d3-07f6-48ac-ac56-81b9749ba165", "Invalid unit of weight: '{0}'.", Move.EW_WeightUQ));
					}

					if (!Constants.Volume.ContainsCode(Move.EW_VolumeUQ))
					{
						return new AutoRatingStatusInfo(false, Res.GetString("717cbd3c-fbd2-4bd2-bd37-5b0564daa1ba", "Invalid unit of volume: '{0}'.", Move.EW_VolumeUQ));
					}

					if (!Constants.Length.ContainsCode(Move.EW_DistanceUnit))
					{
						return new AutoRatingStatusInfo(false, Res.GetString("aabef9c9-2d7f-468d-9289-148458a36bb8", "Invalid unit of distance: '{0}'.", Move.EW_DistanceUnit));
					}

					if (Move.Container != null)
					{
						if (!Constants.Weight.ContainsCode(Move.Container.JC_GrossWeightUQ))
						{
							return new AutoRatingStatusInfo(false, Res.GetString("9bed7014-719e-474d-a5f2-65adc471ab2f", "Invalid unit of weight: '{0}'.", Move.Container.JC_GrossWeightUQ));
						}
					}
				}

				return base.StatusInformation;
			}
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups => chargeCodeGroups ?? (chargeCodeGroups = GetChargeCodeGroups());

		ChargeCodeGroupCollection chargeCodeGroups;

		static ChargeCodeGroupCollection GetChargeCodeGroups()
		{
			ChargeCodeGroupCollection result = new ChargeCodeGroupCollection();
			result.Add(ChargeCodeGroupList.Codes.Transport);
			result.CostChargesFilter = ChargeCodeFilter.AutorateNonConsolLevelOnly;
			return result;
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.LocalCartage;

		public override RateType RateTypeToUse => RateType.LocalTransport;

		// <summary>
		// Hidden service from demurrage time declared in cartage legs
		// </summary>
		protected JobServiceInfo CartageDemurrageJobServiceInfo
		{
			get
			{
				var duration = TotalDemurrageTime.Span;
				var enabled = duration > TimeSpan.Zero;
				var description = Res.GetString("33facaaf-6461-4438-afff-aa70b129bcaf", "Port Transport Demurrage");
				var serviceInfo = new JobServiceInfo(enabled, ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal, description, null, duration);

				if (enabled && Move != null && Move.Container != null)
				{
					serviceInfo.ContainerType = Move.Container.JC_RC;
				}

				return serviceInfo;
			}
		}

		protected abstract TimeInfo TotalDemurrageTime { get; }

		public override ILocation Origin => Move?.Cartage.Branch?.HomePort ?? GlbBranch.CurrentBranch.HomePort;

		public override IDocAddress DeliveryAddress => Delivery;

		public override ZString DeliveryCartageEquipment => CartageEquipment;

		ZString CartageEquipment
		{
			get
			{
				ZString result = ZString.Empty;

				if (!Move.EW_DropMode.IsEmpty)
				{
					result = Move.EW_DropMode;
				}
				else if (Move.Cartage != null)
				{
					result = Move.Cartage.JJ_DropMode;
				}

				return result;
			}
		}

		public override IDocAddress PickupAddress => Pickup;

		public override ZString PickupCartageEquipment => CartageEquipment;

		public override FreightMode FreightMode => Move.FreightMode;

		public override ZString ContainerMode => Move.Cartage.JJ_ContainerMode;

		public override Directions JobDirection => Move.Cartage.JobDirection;

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				ZString serviceLevel = Move.Cartage != null ? Move.Cartage.JJ_RS_NKServiceLevel : ZString.Empty;

				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(serviceLevel, ServiceLevelType.Client),
					new ServiceLevelInfo(serviceLevel, ServiceLevelType.Carrier));
			}
		}

		string IJobNumber.JobNumber
		{
			get { return Move != null && Move.Cartage != null ? ((IJobNumber)Move.Cartage).JobNumber : ""; }
		}

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new CartageRateLineConditionsSupporter(Move.Cartage)); }
		}

		RateLineConditionsSupporter conditionsSupporter;
	}
}
