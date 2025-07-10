using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class SDERateEntryCollection : RateEntryCollection
	{
		public SDERateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory) { }

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.SDE; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			if (entry.Destination() != null)
			{
				return RatingDataRegistry.Instance.ShippingDestinationDefaultChargeCodes.GetAsGuidArray(entry.Destination().Code);
			}
			else
			{
				return Array.Empty<Guid>();
			}
		}
	}
}

