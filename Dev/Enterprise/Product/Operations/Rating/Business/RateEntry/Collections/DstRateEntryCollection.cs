using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class DSTRateEntryCollection : RateEntryCollection
	{
		public DSTRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			if (!entry.TI_DestinationLRC.IsEmpty)
			{
				if (entry.IsAir())
				{
					return RatingDataRegistry.Instance.AirDestinationDefaultChargeCodes.GetAsGuidArray(entry.TI_DestinationLRC);
				}
				else
				{
					return RatingDataRegistry.Instance.SeaDestinationDefaultChargeCodes.GetAsGuidArray(entry.TI_DestinationLRC);
				}
			}
			else
			{
				return Array.Empty<Guid>();
			}
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.DST; }
		}
	}
}

