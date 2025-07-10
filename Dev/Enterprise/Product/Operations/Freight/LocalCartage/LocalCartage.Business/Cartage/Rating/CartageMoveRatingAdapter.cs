using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageMoveRatingAdapter : CartageRatingAdapter
	{
		public CartageMoveRatingAdapter(CommonBookedCtgMove move, JobDocAddress pickup, JobDocAddress delivery, bool autorateServicesOnly = false)
			: base(move, pickup, delivery)
		{
			this.autorateServicesOnly = autorateServicesOnly;
		}

		readonly bool autorateServicesOnly;

		public override ZBool IsServicesOnly => autorateServicesOnly;

		public override MergeChargeOptions MergeCharges => MergeChargeOptions.WithinAdapter;

		public override Collection<IBusiness> AutoRatedFor => new Collection<IBusiness> { Move, Pickup, Delivery };

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = base.JobServices;
				result.Add(CartageDemurrageJobServiceInfo);

				var services = Move.Container != null ? ContainerServices : MoveServices;
				result.AddRange(services);

				return result;
			}
		}

		// <summary>
		// Sum of all demurrage time (if defined) in cartage legs for demurrage service information.
		// </summary>
		protected override TimeInfo TotalDemurrageTime
		{
			get
			{
				var totalDemurrageTime = new TimeSpan();
				Array.ForEach(IncludedLegs, l => totalDemurrageTime += l.GetTotalDemurrage());
				return new TimeInfo(totalDemurrageTime.Days, totalDemurrageTime.Hours, totalDemurrageTime.Minutes);
			}
		}

		// <summary>
		// Services from a Loose Move
		// </summary>
		IEnumerable<JobServiceInfo> MoveServices => GetServiceInfosFromJobServices(Move.Services, ChargeCodeGroupList.Codes.Transport);

		// <summary>
		// Services from the container attached to a containerized Move
		// </summary>
		IEnumerable<JobServiceInfo> ContainerServices
		{
			get
			{
				var services = GetServiceInfosFromJobServices(Move.Container.Services, ChargeCodeGroupList.Codes.Transport);

				foreach (var service in services)
				{
					service.ContainerType = Move.Container.JC_RC;
				}

				return services;
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				// Though CartageMove measures have nothing to do with CartageLeg, CartageLegPK is being used to separate charges.
				result.SetCartagePackage(CartageLegToAllocateRev?.PK.ToGuid(),
					null, null,
					Move.TotalWeight, Move.EW_WeightUQ,
					Move.TotalVolume, Move.EW_VolumeUQ,
					(decimal)Move.EW_BookedPackCount, Move.EW_F3_NKPackType);

				SetAutoRatingContainers(result);

				if (CartageLegToAllocateRev != null)
				{
					result.SetShipmentsWithCartageLegPK(1, CartageLegToAllocateRev.PK);
				}

				result.SetPickupDistance(Move.EW_Distance, Move.EW_DistanceUnit);

				return result;
			}
		}

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var container = Move.Container;
			if (container == null)
			{
				return;
			}

			measures.CreateContainerList(includeCommodity: false, includeContainerNumber: true, includeCartageLegPK: true);
			if ((FreightMode & FreightMode.Containerised) != 0)
			{
				var containerInfo = new MeasureInfo.ContainerInfo(container.JC_GrossWeight, container.JC_GrossWeightUQ, 0m, container.JC_GrossVolumeUQ,
					packages: Move.EW_BookedPackCount,
					container.JC_Calc_TEUCount,
					container.JC_ContainerNum);

				measures.AddContainerWithNumberAndCartageLeg(container.JC_RC, container.JC_ContainerNum, CartageLegToAllocateRev?.PK ?? ZGuid.Empty, containerInfo);
			}
		}

		public override CommonCartageLeg FirstCartageLeg => IncludedLegs.FirstOrDefault();

		public override CommonCartageLeg LastCartageLeg => IncludedLegs.LastOrDefault();

		CommonCartageLeg CartageLegToAllocateRev => Move.Cartage.IsExportOrOrigin ? FirstCartageLeg : LastCartageLeg;

		CommonCartageLeg[] IncludedLegs
		{
			get
			{
				return includedLegs ?? (
					includedLegs = Move.CartageLegs
						.Where(x => x.JU_AdditionalService.IsEmpty)
						.OrderBy(x => x.JU_DisplayOrder)
						.ToArray()
				);
			}
		}
		CommonCartageLeg[] includedLegs;

		public override AutoRatingStatusInfo StatusInformation =>
			IncludedLegs.Length > 0
				? base.StatusInformation
				: new AutoRatingStatusInfo(false, Res.GetString("bb4ebdd8-14d0-4394-8359-44854bdf3989", "Booking ({0}) doesn't contain any included Port Transport Legs. Cannot continue. Ensure all Legs are not marked additional.", Move.Identifier));

		// <summary>
		// Transport Company from work sheet that associates the first cartage leg.
		// Second Leg is ignored (Very odd if they were different, would be a feature request if required)
		// </summary>
		public override OrgHeader Carrier => FirstCartageLeg?.WorkSheet?.TransportCo;

		// <summary>
		// Creditors from work sheets that associates the first and the last cartage legs.
		// </summary>
		public override Creditors Creditors
		{
			get
			{
				var transportProviders = new List<OrgWithSource>();

				var firstCarrier = FirstCartageLeg?.WorkSheet?.TransportCo;
				var lastCarrier = LastCartageLeg?.WorkSheet?.TransportCo;

				if (firstCarrier != null)
				{
					transportProviders.Add(OrgWithSource.NewFrom<OrgHeader>(FirstCartageLeg.WorkSheet.EY_OH_TransportCoInfo));
				}

				if (lastCarrier != null && lastCarrier.PK != firstCarrier?.PK)
				{
					transportProviders.Add(OrgWithSource.NewFrom<OrgHeader>(LastCartageLeg.WorkSheet.EY_OH_TransportCoInfo));
				}

				return Creditors.New(transportProviders);
			}
		}
	}
}
