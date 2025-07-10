using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.LocalCartage.Business
{
	/// <summary>
	/// Rating adapter for a cartage leg.
	/// Legs do not have their own charges, i.e., they have no JobHeader.
	/// They are billed from either a Run Sheet (CommonWorkSheet in C#) or a Transport Job (CommonCartage).
	/// </summary>
	public class CartageLegRatingAdapter : CartageRatingAdapter
	{
		public CartageLegRatingAdapter(
			CommonCartageLeg leg,
			bool forWorkSheet = false,
			bool shouldAutorateServices = false)
			: base(leg.BookedCtgMove, leg.PickupFromDocAddress, leg.DeliverToDocAddress)
		{
			CartageLeg = leg;
			ForWorkSheet = forWorkSheet;
			MergeCharges = forWorkSheet ? MergeChargeOptions.CrossAdapter : MergeChargeOptions.WithinAdapter;
			ShouldAutorateServices = shouldAutorateServices;
		}

		bool ForWorkSheet { get; }
		public override MergeChargeOptions MergeCharges { get; }

		public override Collection<IBusiness> AutoRatedFor => new Collection<IBusiness> { CartageLeg, Pickup, Delivery };

		public override JobServicesCollection JobServices
		{
			get
			{
				if (!ShouldAutorateServices)
				{
					return new JobServicesCollection();
				}

				var result = base.JobServices;
				result.Add(CartageDemurrageJobServiceInfo);

				return result;
			}
		}

		protected override TimeInfo TotalDemurrageTime
		{
			get
			{
				var totalDemurrageTime = CartageLeg.GetTotalDemurrage();
				return new TimeInfo(totalDemurrageTime.Days, totalDemurrageTime.Hours, totalDemurrageTime.Minutes);
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				var container = ((FreightMode & FreightMode.Containerised) != 0) ? Move.Container : null;

				// The Cartage leg PK. Not added for worksheets since that would prevent merging cross adapter.
				Guid? cartageLegPk = null;

				if (!ForWorkSheet)
				{
					cartageLegPk = CartageLeg?.PK.ToGuid();
					result.SetShipmentsWithCartageLegPK(1, CartageLeg.PK);
				}
				else
				{
					result.Shipments = 1;
				}

				result.SetCartagePackage(
					cartageLegPk,
					container?.Container?.PK.ToGuid(),
					container?.JC_ContainerNum.ToString(),
					Move.TotalWeight, Move.EW_WeightUQ,
					Move.TotalVolume, Move.EW_VolumeUQ,
					Move.EW_BookedPackCount, Move.EW_F3_NKPackType);

				SetAutoRatingContainers(result);
				result.SetPickupDistance(CartageLeg.JU_Distance, CartageLeg.JU_DistanceUnit);
				return result;
			}
		}

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var container = CartageLeg.Container;
			if (container == null)
			{
				return;
			}

			measures.CreateContainerList(includeCommodity: false, includeContainerNumber: true, includeCartageLegPK: !ForWorkSheet);

			if ((FreightMode & FreightMode.Containerised) != 0)
			{
				var containerInfo = new MeasureInfo.ContainerInfo(container.JC_GrossWeight, container.JC_GrossWeightUQ, 0m, container.JC_GrossVolumeUQ, 0,
					container.JC_Calc_TEUCount, container.JC_ContainerNum);

				if (!ForWorkSheet)
				{
					measures.AddContainerWithNumberAndCartageLeg(container.JC_RC, container.JC_ContainerNum, CartageLeg.PK, containerInfo);
				}
				else
				{
					measures.AddContainerWithNumber(container.JC_RC, container.JC_ContainerNum, containerInfo);
				}
			}
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				if (CartageLeg != null)
				{
					if (!Constants.Length.ContainsCode(CartageLeg.JU_DistanceUnit))
					{
						return new AutoRatingStatusInfo(false, Res.GetString("c1176a10-61cd-47c1-8711-eb7ddff204c0", "Invalid unit of distance: '{0}'.", CartageLeg.JU_DistanceUnit));
					}
				}

				return base.StatusInformation;
			}
		}

		CommonCartageLeg CartageLeg { get; }

		public override CommonCartageLeg FirstCartageLeg => CartageLeg;

		public override CommonCartageLeg LastCartageLeg => CartageLeg;

		bool ShouldAutorateServices { get; }
		// <summary>
		// Transport Company from work sheet that associates with the cartage leg.
		// </summary>
		public override OrgHeader Carrier => CartageLeg.WorkSheet?.TransportCo;

		// <summary>
		// Creditors from work sheet that associates with the cartage leg.
		// </summary>
		public override Creditors Creditors
		{
			get
			{
				var transportProviders = new List<OrgWithSource>();
				if (CartageLeg.WorkSheet?.TransportCo != null)
				{
					transportProviders.Add(OrgWithSource.NewFrom<OrgHeader>(CartageLeg.WorkSheet.EY_OH_TransportCoInfo));
				}

				return Creditors.New(transportProviders);
			}
		}

		public override IEnumerable<ZString> ExcludedAttributesWhenMergingRateInfos
		{
			get
			{
				if (ForWorkSheet)
				{
					// Ensure leg costs are merged for worksheets, even if they have different container numbers.
					return new ZString[] { JobChargeAttribTypeList.Codes.ContainerNumber };
				}
				else
				{
					return base.ExcludedAttributesWhenMergingRateInfos;
				}
			}
		}
	}
}
