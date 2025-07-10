using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateTransportProviderLookups : AutoRateTransportProviderLookups
	{
		public RateTransportProviderLookups(AutoRateTransportProvider parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly AutoRateTransportProvider Parent;

		#region Related Parties

		public new OrgHeaderCollection RelatedParties
		{
			get { return Factory.GetCachedValue("RelatedParties_Generic", delegate { return new OrgHeaderCollection(Factory); }); }
		}

		#endregion

		#region ZoneTypes

		public CodeDescriptionPairList ZoneTypes
		{
			get { return Factory.GetCachedValue("TransportZoneSets_ZoneTypes", GetZonesPairList); }
		}

		static CodeDescriptionPairList GetZonesPairList()
		{
			var zoneTypes = new CodeDescriptionPairList();

			zoneTypes.AddPair(RatingConstants.RatingZoneTypes.All, RatingConstants.RatingZoneTypes.Descriptions.All);
			zoneTypes.AddPair(RatingConstants.RatingZoneTypes.Rating, RatingConstants.RatingZoneTypes.Descriptions.Rating);
			zoneTypes.AddPair(RatingConstants.RatingZoneTypes.Operations, RatingConstants.RatingZoneTypes.Descriptions.Operations);
			zoneTypes.AddPair(RatingConstants.RatingZoneTypes.Reporting, RatingConstants.RatingZoneTypes.Descriptions.Reporting);

			return zoneTypes;
		}

		#endregion

		#region ZoneModes

		public CodeDescriptionPairList ZoneModes
		{
			get { return Factory.GetCachedValue("TransportZoneSets_ZoneModes" + Parent.TP_ZoneType, () => GetZoneModesPairList(Parent.TP_ZoneType)); }
		}

		static CodeDescriptionPairList GetZoneModesPairList(string zoneType)
		{
			var zoneModeList = new CodeDescriptionPairList();
			if (zoneType == RatingConstants.RatingZoneTypes.Rating)
			{
				zoneModeList = new CodeDescriptionPairList(OLookUpEditType.RateModes);
			}
			else
			{
				zoneModeList.AddPair(Core.Constants.RateMode.ALL, ResString.GetMultilingualString("0DC4319D-78F4-487A-B6D2-59347578E77D", "All Freight Modes"));
			}

			return zoneModeList;
		}

		#endregion
	}
}

