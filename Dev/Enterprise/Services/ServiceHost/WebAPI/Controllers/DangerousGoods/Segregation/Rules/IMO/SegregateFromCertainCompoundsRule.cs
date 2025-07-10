using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class SegregateFromCertainCompoundsRule : ISegregationRule
	{
		public string ApplicableStandard => UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var messages = new List<Message>();
			var messages1 = _Check(substance1, substance2);
			var messages2 = _Check(substance2, substance1);
			messages.AddRange(messages1);
			messages.AddRange(messages2);
			return messages;
		}

		readonly static MultilingualString _SG22Message = ResString.GetMultilingualString("981A185A-E4BB-4E75-B865-E0E249FF6DA7", "Stow \"away from ammonium salts.\"");
		readonly static MultilingualString _SG23AndSG41Message = ResString.GetMultilingualString("A7E92C82-FAAE-4289-8976-6520D7351B85", "Stow \"away from & separated from animal or vegetable oils.\"");
		readonly static MultilingualString _SG27Message = ResString.GetMultilingualString("B5C06071-AA71-4F64-B90F-88DFBD39116A", "Stow \"separated from explosives containing chlorates or perchlorates.\"");
		readonly static MultilingualString _SG37Message = ResString.GetMultilingualString("BC5D09A6-BF24-4E4A-9F06-23237D9BB51F", "Stow \"separated from ammonia\"");
		readonly static MultilingualString _SG46Message = ResString.GetMultilingualString("643C6632-3D2F-4EEC-B4D9-BAD23E8236AA", "Stow \"separated from chlorine.\"");
		readonly static MultilingualString _SG52Message = ResString.GetMultilingualString("41D73E7F-082A-4E9B-86B0-76349246AB12", "Stow \"separated from iron oxide.\"");
		readonly static MultilingualString _SG55Message = ResString.GetMultilingualString("B183ED55-41D6-437D-AF83-1D92F59AA7E4", "Stow \"separated from mercury salts.\"");
		readonly static MultilingualString _SG57Message = ResString.GetMultilingualString("0AE01983-C7F1-4C74-A7DE-21A8C5FBA1D0", "Stow \"separated from odor-absorbing cargoes.\"");
		readonly static MultilingualString _SG62Message = ResString.GetMultilingualString("395C5E65-A799-4EEB-AA81-67713A8D50E7", "Stow \"separated from sulfur.\"");

		readonly static HashSet<ZString> _SG22IncompatibleSubstances = new HashSet<ZString>() { "0004A", "0004B", "0004C", "0222A", "0222B", "0402A", "0402B", "1310", "1439", "1442", "1444", "1512", "1546", "1630", "1727", "1835A", "1835B", "1843", "1942A", "1942B", "2067A", "2067B", "2071", "2426", "2505", "2506", "2683A", "2683B", "2687", "2817A", "2817B", "2818A", "2818B", "2854", "2859", "2861", "2863", "3375A", "3375B", "3375C", "3423", "3424A", "3424B" };
		readonly static HashSet<ZString> _SG27IncompatibleSubstances = new HashSet<ZString>() { "1461", "1481A", "1481B", "3210A", "3210B", "3211A", "3211B" };
		readonly static HashSet<ZString> _SG37IncompatibleSubstances = new HashSet<ZString>() { "1005", "1043", "1841", "2073", "2672", "3318" };
		readonly static HashSet<ZString> _SG46IncompatibleSubstances = new HashSet<ZString>() { "1017", "1749", "2548", "2761A", "2761B", "2761C", "2762A", "2762B", "2995A", "2995B", "2995C", "2996A", "2996B", "2996C", "3520" };
		readonly static HashSet<ZString> _SG52IncompatibleSubstances = new HashSet<ZString>() { "1376A", "1376B", "1994" };
		readonly static HashSet<ZString> _SG55IncompatibleSubstances = new HashSet<ZString>() { "0135", "1629", "1630", "1631", "1634", "1636", "1637", "1638", "1639", "1640", "1641", "1642", "1643", "1644", "1645", "1646", "2024A", "2024B", "2024C", "2025A", "2025B", "2025C", "2777A", "2777B", "2777C", "2778A", "2778B", "2809", "3011A", "3011B", "3011C", "3012A", "3012B", "3012C", "3506" };
		readonly static HashSet<ZString> _SG62IncompatibleSubstances = new HashSet<ZString>() { "1079", "1080", "1350", "1786", "1817", "1828", "1829", "1830A", "1830B", "1831A", "1831B", "1832A", "1832B", "1833", "1834", "2191", "2240", "2308", "2418", "2448", "2571A", "2571B", "2796A", "3456" };

		readonly static Dictionary<ZString, (HashSet<ZString> incompatibleSubstances, ZString message)> _incompatibleSubstancesAndMessagesForSegregationCode = new Dictionary<ZString, (HashSet<ZString> incompatibleSubstances, ZString message)>
			{
				{ "SG22", (_SG22IncompatibleSubstances, _SG22Message) },
				{ "SG23", (new HashSet<ZString>(), _SG23AndSG41Message) },
				{ "SG27", (_SG27IncompatibleSubstances, _SG27Message) },
				{ "SG37", (_SG37IncompatibleSubstances, _SG37Message) },
				{ "SG41", (new HashSet<ZString>(), _SG23AndSG41Message) },
				{ "SG46", (_SG46IncompatibleSubstances, _SG46Message) },
				{ "SG52", (_SG52IncompatibleSubstances, _SG52Message) },
				{ "SG55", (_SG55IncompatibleSubstances, _SG55Message) },
				{ "SG57", (new HashSet<ZString>(), _SG57Message) },
				{ "SG62", (_SG62IncompatibleSubstances, _SG62Message) },
			};

		static IEnumerable<Message> _Check(UNDGSubstance substanceWithSegregationCode, UNDGSubstance theOtherSubstance)
		{
			var messages = new List<Message>();

			foreach (var segregationCode in substanceWithSegregationCode.SegregationCodes)
			{
				if (_incompatibleSubstancesAndMessagesForSegregationCode.TryGetValue(segregationCode, out var tupleValue))
				{
					(var incompatibleSubstances, var message) = tupleValue;
					if (incompatibleSubstances.Contains(theOtherSubstance.DG_Code.ToUpper()))
					{
						messages.Add(new Message(MessageType.Error, message));
					}
					else
					{
						messages.Add(new Message(MessageType.Warning, message));
					}
				}
			}

			return messages;
		}
	}
}
