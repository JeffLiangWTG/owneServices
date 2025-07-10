using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetRatingAdapter : RatingAdapter<CommonWorkSheet>
	{
		readonly CommonWorkSheet WorkSheet;

		readonly CommonCartageLeg FirstLeg;

		public CommonWorkSheetRatingAdapter(CommonWorkSheet workSheet)
			: base(workSheet)
		{
			WorkSheet = workSheet;

			FirstLeg = workSheet
				.CartageLegs
				.OrderBy(leg => leg.JU_RunSheetSequence)
				.FirstOrDefault();
		}

		public override AdapterType AdapterType => AdapterType.RunSheet;

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				return WorkSheet.HasMixedContainerMode
					? new AutoRatingStatusInfo(false, WorkSheet.MixedContainerModeErrorMessage)
					: new AutoRatingStatusInfo(true);
			}
		}

		public override ILocation Origin
		{
			get
			{
				return FirstLeg == null
					? null
					: FirstLeg.Cartage.Branch != null ? FirstLeg.Cartage.Branch.HomePort : GlbBranch.CurrentBranch.HomePort;
			}
		}

		public override OrgHeader Carrier
		{
			get { return WorkSheet.TransportCo; }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(OrgWithSource.NewFrom<OrgHeader>(WorkSheet.EY_OH_TransportCoInfo)); }
		}

		public override FreightMode FreightMode
		{
			get
			{
				var freightModes = this
					.WorkSheet
					.CartageLegs
					.Select(leg => leg.BookedCtgMove.FreightMode)
					.Distinct()
					.ToArray();

				return freightModes.Length == 1 ? freightModes.Single() : FreightMode.UKN;
			}
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.LocalTransport; }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups => chargeCodeGroups ?? (chargeCodeGroups = GetChargeCodeGroups());

		ChargeCodeGroupCollection chargeCodeGroups;

		static ChargeCodeGroupCollection GetChargeCodeGroups()
		{
			var result = new ChargeCodeGroupCollection { ChargeCodeGroupList.Codes.Transport };
			result.SellChargesFilter = ChargeCodeFilter.AutorateNothing;
			// Business decision: Autorate cost a run sheet should NOT care if a charge code is a consol level one or not.
			result.CostChargesFilter = ChargeCodeFilter.AutorateAll;

			return result;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.LocalCartage; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var measures = new RateableMeasureSet(AdapterType);
				SetCountainerCountMeasure(measures, WorkSheet.CartageLegs);
				measures.Time = WorkSheet.EY_EndTime == ZDateTime.Empty || WorkSheet.EY_StartTime == ZDateTime.Empty
					? TimeInfo.Empty
					: new TimeInfo(WorkSheet.EY_EndTime - WorkSheet.EY_StartTime);

				return measures;
			}
		}

		void SetCountainerCountMeasure(RateableMeasureSet measures, IEnumerable<CommonCartageLeg> legs)
		{
			measures.CreateContainerList(includeCommodity: false, includeContainerNumber: true);

			if ((((IAutoRating)this).FreightMode & FreightMode.Containerised) == 0)
			{
				return;
			}

			legs
				.Where(leg => leg.BookedCtgMove.Container != null)
				.ForEach(leg =>
				{
					var container = leg.BookedCtgMove.Container;

					var containerInfo = new MeasureInfo.ContainerInfo(
						container.JC_GrossWeight,
						container.JC_GrossWeightUQ,
						0m,
						container.JC_GrossVolumeUQ,
						0,
						container.JC_Calc_TEUCount,
						container.JC_ContainerNum);
					measures.AddContainerWithNumber(container.JC_RC, container.JC_ContainerNum, containerInfo);
				});
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return jobDatesProvider ?? (jobDatesProvider = new CommonWorkSheetDateProvider(WorkSheet)); }
		}

		IJobDatesProvider jobDatesProvider;

		public override IEnumerable<ZString> ExcludedAttributesWhenMergingRateInfos
		{
			get
			{
				// Costings for run sheets can be merged from calculation results from different legs.
				// Different ContainerNumber attributes prevent us from merging them. Each apportioned charge from a cost charge must has
				// its own ContainerNumber attribute for revenue charge merging.
				// We pass ContainerNumber along the way to AutoRateInfos but use this list to ignore it when merging cost charges.
				return excludedAttributesWhenMergingRateInfos
					?? (excludedAttributesWhenMergingRateInfos = new ZString[] { JobChargeAttribTypeList.Codes.ContainerNumber });
			}
		}
		IEnumerable<ZString> excludedAttributesWhenMergingRateInfos;

		class CommonWorkSheetDateProvider : JobDatesProvider<CommonWorkSheet>
		{
			public CommonWorkSheetDateProvider(CommonWorkSheet workSheet)
				: base(workSheet)
			{
			}

			protected override ZDateTime GetArrivalDateCore()
			{
				return Parent.EY_StartTime;
			}

			protected override ZDateTime GetDepartureDateCore()
			{
				return Parent.EY_EndTime;
			}
		}
	}
}
