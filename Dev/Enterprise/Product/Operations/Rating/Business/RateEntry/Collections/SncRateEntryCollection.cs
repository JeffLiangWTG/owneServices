using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class SNCRateEntryCollection : RateEntryCollection
	{
		public SNCRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory) { }

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.SNC; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.LCL; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			return RatingDataRegistry.Instance.ShippingNonContainerisedtDefaultCodes.GetAsGuidArray();
		}

		protected override string DefaultFreightChargeCalculator
		{
			get { return MinimumOrPerUnitCalculator.Code; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var rateEntryChild = (RateEntry)child;

			rateEntryChild.Unit = Env.Registry.FreightVolumeUnit;
			rateEntryChild.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
		}
	}
}
