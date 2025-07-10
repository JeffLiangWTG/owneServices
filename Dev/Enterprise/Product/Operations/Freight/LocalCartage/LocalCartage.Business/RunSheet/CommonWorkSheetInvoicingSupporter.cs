using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetInvoicingSupporter : JobInvoicingSupporter, IAutoRatingApportionmentTargetSupporter
	{
		readonly CommonCartage Cartage;

		readonly IEnumerable<CommonCartageLeg> RelatedLegs;

		public CommonWorkSheetInvoicingSupporter(CommonWorkSheet workSheet, CommonCartage cartage)
			: base(cartage)
		{
			Cartage = cartage;

			RelatedLegs = workSheet
				.CartageLegs
				.Intersect(cartage.CartageLegs)
				.OrderBy(leg => leg.JU_RunSheetSequence)
				.ToList();
		}

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return !Cartage.IsCancelled;
		}

		public override ZString ContainerMode
		{
			get
			{
				return RelatedLegs.Any() && RelatedLegs.All(leg => leg.IsContainerised)
					? Constants.ContainerModes.FCL
					: Constants.ContainerModes.LCL;
			}
		}

		public override ZDecimal ActualVolume
		{
			get
			{
				var targetUnit = ActualVolumeUnit;

				return targetUnit == ZString.Empty
					? RelatedLegs.Sum(leg => leg.TotalVolume)
					: RelatedLegs.Sum(leg => Core.Constants.Volume.Convert(leg.TotalVolume, leg.TotalVolumeUnit, targetUnit));
			}
		}

		public override ZString ActualVolumeUnit
		{
			get { return RelatedLegs.FirstOrDefault(x => !x.TotalVolumeUnit.IsEmpty)?.TotalVolumeUnit ?? ZString.Empty; }
		}

		public override ZDecimal ActualWeight
		{
			get
			{
				var targetUnit = ActualWeightUnit;

				return targetUnit == ZString.Empty
					? RelatedLegs.Sum(leg => leg.TotalWeight)
					: RelatedLegs.Sum(leg => Core.Constants.Weight.Convert(leg.TotalWeight, leg.TotalWeightUnit, targetUnit));
			}
		}

		public override ZString ActualWeightUnit
		{
			get { return RelatedLegs.FirstOrDefault(x => !x.TotalWeightUnit.IsEmpty)?.TotalWeightUnit ?? ZString.Empty; }
		}

		public override int ContainerCount
		{
			get { return RelatedLegs.Sum(leg => leg.Container?.JC_ContainerCount ?? 0); }
		}

		public override int OuterPackTotal
		{
			get { return RelatedLegs.Sum(leg => leg.TotalPackages); }
		}

		public override ZDecimal TEUCount
		{
			get { return RelatedLegs.Sum(leg => leg.Container?.JC_Calc_TEUCount ?? 0); }
		}

		public override ZDecimal ActualChargeable
		{
			get { return 0; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return RelatedLegs.FirstOrDefault()?.BookedCtgMove.EW_F3_NKPackType ?? ZString.Empty; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.LocalCartage; }
		}

		IEnumerable<string> IAutoRatingApportionmentTargetSupporter.GetDistinctContainerNumbers()
		{
			return RelatedLegs
				.Select(x => x.Container?.JC_ContainerNum.ToString())
				.Where(num => !string.IsNullOrEmpty(num))
				.Distinct();
		}
	}
}
