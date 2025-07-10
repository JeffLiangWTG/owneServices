using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class SORRateEntryCollection : RateEntryCollection
	{
		public SORRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory) { }

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.SOR; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			if (entry.Origin() != null)
			{
				return RatingDataRegistry.Instance.ShippingOriginDefaultChargeCodes.GetAsGuidArray(entry.Origin().Code);
			}
			else
			{
				return Array.Empty<Guid>();
			}
		}
	}
}

