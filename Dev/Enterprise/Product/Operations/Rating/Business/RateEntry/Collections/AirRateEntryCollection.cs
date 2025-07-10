using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class AIRRateEntryCollection : RateEntryCollection
	{
		public AIRRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.AIR; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.LSE; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			return RatingDataRegistry.Instance.AIRFreightDefaultCodes.GetAsGuidArray();
		}

		protected override string DefaultFreightChargeCalculator
		{
			get { return CombinedCalculator.Code; }
		}

		protected override void AddFreightLineItems(RateLine freightRateLine)
		{
			freightRateLine.RateLineItems.AddAIRFreightLineItems();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var rateEntryChild = (RateEntry)child;

			rateEntryChild.Unit = Env.Registry.FreightWeightUnit;
		}
	}
}

