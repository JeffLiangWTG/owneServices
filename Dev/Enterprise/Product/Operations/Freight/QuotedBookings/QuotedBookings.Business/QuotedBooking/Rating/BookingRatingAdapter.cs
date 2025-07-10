using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.QuotedBookings.Business
{
	class BookingRatingAdapter : ShipmentRatingAdapter<ForwardingShipment>
	{
		public BookingRatingAdapter(QuotedBooking quotedBooking)
			: base(quotedBooking.Booking)
		{
			this.quotedBooking = quotedBooking;
		}

		protected override JobHeader ParentJob => quotedBooking.Job;

		public override RateablePartList GetPackages(CommonConsol consol = null, bool fallbackToShipmentMeasures = true)
		{
			var packages = new RateablePartList
			{
				HasContainerType = true,
				HasCommodity = true,
				WeightUnit = WeightUnit,
				VolumeUnit = VolumeUnit
			};

			var parts = parent.OuterPackLines.Select(packLine =>
			{
				var part = GetPackagePartFromPackLine(packLine, consol);
				part.ContainerTypePk = NullableHelper.ToNullable(packLine.JL_RC_ContainerType);

				return part;
			}).ToArray();

			packages.AddParts(parts);
			return packages;
		}

		public override RateablePartList GetUnits(CommonConsol consol)
		{
			var units = new RateablePartList
			{
				HasContainerType = true,
				HasPackageType = true
			};

			var parts = parent.OuterPackLines.Select(packLine =>
			{
				var part = GetUnitPartFromPackLine(packLine, consol);
				part.ContainerTypePk = NullableHelper.ToNullable(packLine.JL_RC_ContainerType);

				return part;
			}).ToArray();

			units.AddParts(parts);
			return units;
		}

		protected override void SetContainersCore(RateableMeasureSet measures)
		{
			var containers = quotedBooking.QuotedBookingContainers
				.Cast<ForwardingContainer>()
				.GroupBy(c => new { c.JC_RC, c.JC_RH_NKContainerCommodityCode })
				.ToList();

			foreach (var container in containers)
			{
				var packs = Parent.OuterPackLines
					.Cast<ForwardingPackLine>()
					.Where(pl => pl.JL_RC_ContainerType == container.Key.JC_RC && pl.JL_RH_NKCommodityCode == container.Key.JC_RH_NKContainerCommodityCode)
					.ToList();

				// We need to convert weight and volume of all packs to the same unit.
				// It doesn't matter which one as it will be converted to KG/M3 later anyway (this is how autorating works with containers),
				// so, we convert to KG/M3.
				var weight = packs.Sum(pl => FreightRatingHelper.Convert(pl.JL_ActualWeight, pl.JL_ActualWeightUQ, Constants.Weight.Kilograms));
				var volume = packs.Sum(pl => FreightRatingHelper.Convert(pl.JL_ActualVolume, pl.JL_ActualVolumeUQ, Constants.Volume.CubicMetres));

				var containerInfo = new MeasureInfo.ContainerInfo(
					weight: weight, weightUnit: Constants.Weight.Kilograms,
					volume: volume, volumeUnit: Constants.Volume.CubicMetres,
					containerCount: container.Sum(c => c.JC_ContainerCount),
					teu: container.Sum(c => c.JC_Calc_TEUCount));

				measures.AddContainerGroup(
					container.Key.JC_RC,
					container.Key.JC_RH_NKContainerCommodityCode,
					new[] { containerInfo });
			}

			// for mixed-mode shipments (FCL and LCL), the LCL information needs to be passed in to the
			// container collection with weight/volume information (container type and count are irrelevant)
			if (quotedBooking.ContainerMode == Constants.ContainerModes.LCL)
			{
				var commodity = quotedBooking.Quote != null ? quotedBooking.Quote.CurrentOneOffQuote.TT_RH_NKCommodity : ZString.Empty;
				measures.AddLCL(commodity, quotedBooking.Weight, quotedBooking.WeightUnit, quotedBooking.Volume, quotedBooking.VolumeUnit, quotedBooking.Booking.JS_OuterPacks);
			}
		}

		public override Creditors Creditors
		{
			get
			{
				var shippingCreditors = base.Creditors;
				var bookingCreditors = new Creditors();
				foreach (var chargeCodeGroup in ChargeCodeGroups)
				{
					if (shippingCreditors[chargeCodeGroup].Count > 0)
					{
						var orgSource = OrgWithSource.NewFrom<OrgHeader>(quotedBooking.CreditorInfo);
						var orgPrioritizedList = new OrgPrioritizedList { { 1, orgSource } };
						bookingCreditors.Add(chargeCodeGroup, orgPrioritizedList);
						foreach (var shipmentOrg in shippingCreditors[chargeCodeGroup])
						{
							bookingCreditors[chargeCodeGroup].Merge(new OrgPrioritizedList { { 2, shipmentOrg } });
						}
					}
				}
				return bookingCreditors;
			}
		}

		readonly QuotedBooking quotedBooking;
	}
}
