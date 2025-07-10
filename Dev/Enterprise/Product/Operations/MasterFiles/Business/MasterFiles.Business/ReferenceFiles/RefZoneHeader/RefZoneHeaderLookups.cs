using System.Collections.Generic;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ZoneTypeList : List<ZoneTypeCodeDescriptionPair>
	{
		public List<string> GetCodes()
		{
			List<string> codes = new List<string>();
			foreach (ZoneTypeCodeDescriptionPair value in this)
			{
				codes.Add(value.Code);
			}
			return codes;
		}
	}

	public class ZoneTypeCodeDescriptionPair : CodeDescriptionPair
	{
		protected ZoneTypeCodeDescriptionPair(object code, MultilingualString description) : base(code, description) { }

		public static readonly ZoneTypeCodeDescriptionPair All = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.All, ResString.GetMultilingualString("MasterFiles|ZoneType|All", "All Types"));
		public static readonly ZoneTypeCodeDescriptionPair Reporting = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.Reporting, ResString.GetMultilingualString("MasterFiles|ZoneType|Reporting", "Reporting Only"));
		public static readonly ZoneTypeCodeDescriptionPair Rating = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.Rating, ResString.GetMultilingualString("MasterFiles|ZoneType|Rating", "Rating Only"));
		public static readonly ZoneTypeCodeDescriptionPair RatingImport = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.RatingImport, ResString.GetMultilingualString("MasterFiles|ZoneType|RatingImport", "Rating (Import) Only"));
		public static readonly ZoneTypeCodeDescriptionPair RatingExport = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.RatingExport, ResString.GetMultilingualString("MasterFiles|ZoneType|RatingExport", "Rating (Export) Only"));
		public static readonly ZoneTypeCodeDescriptionPair Tax = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.Tax, ResString.GetMultilingualString("MasterFiles|ZoneType|Tax", "Tax Only"));
		public static readonly ZoneTypeCodeDescriptionPair TransitWarehouse = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse, ResString.GetMultilingualString("MasterFiles|ZoneType|TransitWarehouse", "Transit Warehouse"));
		public static readonly ZoneTypeCodeDescriptionPair WiseRatesOcean = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean, ResString.GetMultilingualString("MasterFiles|ZoneTypes|WRS", "Rates Service Ocean"));
		public static readonly ZoneTypeCodeDescriptionPair OriginGateway = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway, ResString.GetMultilingualString("MasterFiles|ZoneTypes|OGT", "Origin Gateway"));
		public static readonly ZoneTypeCodeDescriptionPair DestinationGateway = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway, ResString.GetMultilingualString("MasterFiles|ZoneTypes|DGT", "Destination Gateway"));
		public static readonly ZoneTypeCodeDescriptionPair Contract = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.Contract, ResString.GetMultilingualString("MasterFiles|ZoneTypes|CON", "Contract"));
		public static readonly ZoneTypeCodeDescriptionPair Schedules = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.Schedules, ResString.GetMultilingualString("MasterFiles|ZoneTypes|SCH", "Schedules"));
		public static readonly ZoneTypeCodeDescriptionPair HVLVGateway = new ZoneTypeCodeDescriptionPair(RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway, ResString.GetMultilingualString("MasterFiles|ZoneTypes|HVL", "HVLV Gateway"));
	}

	public class ZoneTypeCodePairList : CodeDescriptionPairList
	{
		public ZoneTypeCodePairList()
		{
			Add(ZoneTypeCodeDescriptionPair.All);
			Add(ZoneTypeCodeDescriptionPair.Reporting);
			Add(ZoneTypeCodeDescriptionPair.Rating);
			Add(ZoneTypeCodeDescriptionPair.RatingImport);
			Add(ZoneTypeCodeDescriptionPair.RatingExport);
			Add(ZoneTypeCodeDescriptionPair.Tax);
			Add(ZoneTypeCodeDescriptionPair.TransitWarehouse);
			Add(ZoneTypeCodeDescriptionPair.WiseRatesOcean);
			Add(ZoneTypeCodeDescriptionPair.OriginGateway);
			Add(ZoneTypeCodeDescriptionPair.DestinationGateway);
			Add(ZoneTypeCodeDescriptionPair.Contract);
			Add(ZoneTypeCodeDescriptionPair.Schedules);
			Add(ZoneTypeCodeDescriptionPair.HVLVGateway);
		}
	}

	public class RefZoneHeaderLookups : AutoRefZoneHeaderLookups
	{
		public RefZoneHeaderLookups(AutoRefZoneHeader parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly AutoRefZoneHeader Parent;

		#region Zone Types

		public CodeDescriptionPairList ZoneTypes
		{
			get
			{
				if (fZoneTypes == null)
				{
					fZoneTypes = GetZoneTypes();
				}
				return fZoneTypes;
			}
		}

		protected virtual ZoneTypeCodePairList GetZoneTypes()
		{
			return new ZoneTypeCodePairList();
		}

		public static class ZoneTypeCodes
		{
			public const string All = "ALL";
			public const string Reporting = "RPT";
			public const string Rating = "RAT";
			public const string RatingImport = "IMP";
			public const string RatingExport = "EXP";
			public const string Tax = "TAX";
			public const string TransitWarehouse = "TWH";
			public const string WiseRatesOcean = "WRS";
			public const string OriginGateway = "OGT";
			public const string DestinationGateway = "DGT";
			public const string Contract = "CON";
			public const string Schedules = "SCH";
			public const string HVLVGateway = "HVL";
		}

		CodeDescriptionPairList fZoneTypes;

		#endregion

		public CodeDescriptionPairList ZoneModes
		{
			get { return Factory.GetCachedValue("InternationalZones_ZoneModes" + Parent.FZ_ZoneType, () => GetZoneModesPairList(Parent.FZ_ZoneType)); }
		}

		static CodeDescriptionPairList GetZoneModesPairList(string zoneType)
		{
			var zoneModeList = new CodeDescriptionPairList();
			if (zoneType == ZoneTypeCodes.Rating || zoneType == ZoneTypeCodes.RatingExport || zoneType == ZoneTypeCodes.RatingImport
				|| zoneType == ZoneTypeCodes.OriginGateway || zoneType == ZoneTypeCodes.DestinationGateway || zoneType == ZoneTypeCodes.Contract)
			{
				zoneModeList = new CodeDescriptionPairList(OLookUpEditType.RateModes);
			}
			else if (zoneType == ZoneTypeCodes.Schedules)
			{
				zoneModeList.AddPair(Core.Constants.RateMode.ALL, ResString.GetMultilingualString("BB9DAFF3-3BE4-4888-BBED-4A8E26986092", "All Freight Modes"));
				zoneModeList.AddPair(Core.Constants.RateMode.AIR, ResString.GetMultilingualString("5AD0B8E8-3161-4555-B7AC-9DD800F95986", "Air"));
				zoneModeList.AddPair(Core.Constants.RateMode.SEA, ResString.GetMultilingualString("8B1C633C-B0FC-4E00-9BF5-BF32CF7DB408", "Sea"));
			}
			else
			{
				zoneModeList.AddPair(Core.Constants.RateMode.ALL, ResString.GetMultilingualString("4D8C6A04-F9A2-4E19-ADF5-7613A9BADD4A", "All Freight Modes"));
			}

			return zoneModeList;
		}

		#region UNLOCOs

		public RefUNLOCOCollection UNLOCOs
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region Countries

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region RelatedParties

		public override OrgHeaderCollection RelatedParties
		{
			get
			{
				return Parent.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway || Parent.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway
					? new ForwarderCollection(Factory)
					: new OrganisationsFindBoxCollection(Factory);
			}
		}

		#endregion
	}
}
