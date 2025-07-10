using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public class ORGRateEntryCollection : RateEntryCollection
	{
		public ORGRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}

		protected override Guid[] DefaultChargeCodes(RateEntry entry)
		{
			if (!entry.TI_OriginLRC.IsEmpty)
			{
				if (entry.IsAir())
				{
					return RatingDataRegistry.Instance.AirOriginDefaultChargeCodes.GetAsGuidArray(entry.TI_OriginLRC);
				}
				else
				{
					return RatingDataRegistry.Instance.SeaOriginDefaultChargeCodes.GetAsGuidArray(entry.TI_OriginLRC);
				}
			}
			else
			{
				return Array.Empty<Guid>();
			}
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.ORG; }
		}
	}
}

