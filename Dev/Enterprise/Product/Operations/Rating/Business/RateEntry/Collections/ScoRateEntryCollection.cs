using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class SCORateEntryCollection : RateEntryCollection
	{
		public SCORateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory) { }
		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.SCO; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.SEA; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			return RatingDataRegistry.Instance.ShippingContainerisedDefaultCodes.GetAsGuidArray();
		}

		protected override string DefaultFreightChargeCalculator
		{
			get { return UnitCalculator.Code; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var rateEntryChild = (RateEntry)child;

			rateEntryChild.Unit = RatingConstants.Units.CN;
			rateEntryChild.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
		}
	}
}
