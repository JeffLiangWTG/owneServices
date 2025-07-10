//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceCFRLookups
//
//    This class should be used for overriding collections in AutoUNDGSubstanceCFRLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceCFRLookups : AutoUNDGSubstanceCFRLookups
	{
		public UNDGSubstanceCFRLookups(AutoUNDGSubstanceCFR parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PrimaryClassList => UNDGDataItemLookups.GetDGClassList(Factory);

		public CodeDescriptionPairList SecondaryClassList => GetSecondaryClassList(Factory);

		public CodeDescriptionPairList TertiaryClassList => GetTertiaryClassList(Factory);

		public CodeDescriptionPairList TechnicalNameList => UNDGSubstanceLookups.GetTechNameList(Factory);

		public CodeDescriptionPairList PackingGroupList => UNDGSubstanceLookups.GetPackingGroupList(Factory);

		public CodeDescriptionPairList ExceptedQuantityList => UNDGSubstanceLookups.GetExceptedQuantityList(Factory);

		public CodeDescriptionPairList MarinePollutantList => UNDGSubstanceLookups.GetMarinePollutantList(Factory);

		public CodeDescriptionPairList StateList => UNDGSubstanceLookups.GetStateList(Factory);

		public CodeDescriptionPairList PoisonInhalationHazardList => GetPoisonInhalationHazardList(Factory);

		public CodeDescriptionPairList Languages => Factory.GetCachedValue(
			"UNDGAttribute_Languages",
			() => new CodeDescriptionPairList(OLookUpEditType.Language)
		);

		public CodeDescriptionPairList AirRailLimitTypeList => GetAirRailLimitTypeList(Factory);

		public static class AirRailLimitTypes
		{
			public static string Forbidden
			{
				get { return Res.GetString("d40988b2-fb63-5b90-429b-be2c27b54c90", "Forbidden"); }
			}
			public static string NoLimit
			{
				get { return Res.GetString("c5434a97-2b2f-d991-4d05-3f2cef39915d", "No Limit"); }
			}

			public static string NetWeightLimit
			{
				get { return Res.GetString("b8cef67d-64a1-414d-a078-adf260e4353b", "Net Weight Limit"); }
			}

			public static string GrossWeightLimit
			{
				get { return Res.GetString("3d16885f-3532-4923-9be1-cc3277965b74", "Gross Weight Limit"); }
			}

			public static class Codes
			{
				public const string Forbidden = "FOB";
				public const string NoLimit = "NLT";
				public const string NetWeightLimit = "NLM";
				public const string GrossWeightLimit = "GLM";
			}
		}

		public static class PoisonInhalationHazardProvisions
		{
			public static class Codes
			{
				public const string SP1Raw = "A";
				public const string SP2Raw = "B";
				public const string SP3Raw = "C";
				public const string SP4Raw = "D";
				public const string SP5Raw = "*";
				public const string SP6Raw = "?";

				public const string SP1Readable = "A";
				public const string SP2Readable = "B";
				public const string SP3Readable = "C";
				public const string SP4Readable = "D";
				public const string SP5Readable = "SP5";
				public const string SP6Readable = "SP6";
			}
		}

		#region Implementation

		CodeDescriptionPairList GetSecondaryClassList(BusinessObjectFactory factory)
			=> factory.GetCachedValue("SecondaryClassList", () =>
				{
					var collection = new[] { "1", "2.1", "3", "4.1", "4.2", "5.1", "6.1", "7", "8" };
					var list = new CodeDescriptionPairList();

					foreach (var item in collection)
					{
						list.AddPair(item, Res.GetString("b6ff22c4-57f4-46e9-a573-1d0e6ee8504d", "Class {0}", item));
					}

					return list;
				});

		CodeDescriptionPairList GetTertiaryClassList(BusinessObjectFactory factory)
			=> factory.GetCachedValue("TertiaryClassList", () =>
				{
					var collection = new[] { "3", "6", "8" };
					var list = new CodeDescriptionPairList();

					foreach (var item in collection)
					{
						list.AddPair(item, Res.GetString("7908ba63-8693-47fc-bb9e-5b6c35efcd8c", "Class {0}", item));
					}

					return list;
				});

		CodeDescriptionPairList GetAirRailLimitTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AirRailLimitTypeList", () =>
			{
				var airRailLimitTypes = new CodeDescriptionPairList();
				airRailLimitTypes.AddPair(AirRailLimitTypes.Codes.Forbidden, AirRailLimitTypes.Forbidden);
				airRailLimitTypes.AddPair(AirRailLimitTypes.Codes.NoLimit, AirRailLimitTypes.NoLimit);
				airRailLimitTypes.AddPair(AirRailLimitTypes.Codes.NetWeightLimit, AirRailLimitTypes.NetWeightLimit);
				airRailLimitTypes.AddPair(AirRailLimitTypes.Codes.GrossWeightLimit, AirRailLimitTypes.GrossWeightLimit);

				return airRailLimitTypes;
			});
		}

		CodeDescriptionPairList GetPoisonInhalationHazardList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PoisonInhalationHazardList", () =>
			{
				var collection = new[]
				{
					PoisonInhalationHazardProvisions.Codes.SP1Readable,
					PoisonInhalationHazardProvisions.Codes.SP2Readable,
					PoisonInhalationHazardProvisions.Codes.SP3Readable,
					PoisonInhalationHazardProvisions.Codes.SP4Readable,
					PoisonInhalationHazardProvisions.Codes.SP5Readable,
					PoisonInhalationHazardProvisions.Codes.SP6Readable
				};

				var poisonInhalationHazardTypes = new CodeDescriptionPairList();

				foreach (var item in collection)
				{
					if (item.StartsWith("SP"))
					{
						poisonInhalationHazardTypes.AddPair(item, Res.GetString("86e30efb-e259-a39f-4247-f85f88d1cc51", "Special Provision {0}", item));
					}
					else
					{
						poisonInhalationHazardTypes.AddPair(item, Res.GetString("297b30e2-be1f-4423-b99d-2186fe3d7e14", "Hazard Zone {0}", item));
					}
				}

				return poisonInhalationHazardTypes;
			});
		}

		#endregion
	}
}
