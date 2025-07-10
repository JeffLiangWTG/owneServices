namespace Enterprise.Rating.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public class RatingIATASupportedLocationCollection : RatingLocationCollection, ICodeDescriptionPairList
	{
		public RatingIATASupportedLocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public RatingIATASupportedLocationCollection(BusinessObjectFactory factory, ZoneTypeList requiredZoneTypes)
			: base(factory, requiredZoneTypes)
		{ }

		public RatingIATASupportedLocationCollection(BusinessObjectFactory factory, bool allowZones)
			: base(factory, allowZones)
		{ }

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider => new IATASupportedLocationFindBoxListProvider(this);

		class IATASupportedLocationFindBoxListProvider : LocationFindBoxListProvider
		{
			public IATASupportedLocationFindBoxListProvider(BusinessObjectCollection list)
				: base(list)
			{
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				string iataCode = null;

				if (LocationHelper.GetLocationType(code) == LocationHelper.LocationType.IATACityCode)
				{
					iataCode = IATACityCode.GetValidOrDefault(List.Factory, code)?.Code;
				}

				if (iataCode != null)
				{
					return (iataCode, true);
				}
				return base.NearestMatchCore(code, explicitAutoComplete);
			}

			public override string DescriptionFromCode(string code)
			{
				string description = null;

				if (LocationHelper.GetLocationType(code) == LocationHelper.LocationType.IATACityCode)
				{
					description = IATACityCode.GetValidOrDefault(List.Factory, code)?.Description;
				}

				return description ?? base.DescriptionFromCode(code);
			}
		}

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code) =>
			LocationHelper.GetLocationFromString(new ZString(code), Factory) != null;

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code) =>
			LocationHelper.GetLocationFromString(code, Factory)?.Description ?? string.Empty;

		#endregion
	}
}

