using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class SegregationWarningRule : ISegregationRule
	{
		public string ApplicableStandard => UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		readonly Dictionary<string, string> _segregationCodes = new Dictionary<string, string>
		{
			{ "SG23", Res.GetString("3c6aae99-4f98-448e-9680-946115275951", "Stow \"away from animal or vegetable oils.\"") },
			{ "SG29", Res.GetString("7c52a26e-5bc8-4dc6-9809-9482be421495", "Segregation from foodstuffs as described in Chapter 7.3.4.2.2. 7.6.3.1.2 or 7.7.3.7 of the IMDG Code.") },
			{ "SG41", Res.GetString("40e16e56-9fa7-433d-9113-116a98c4244d", "Stow \"separated from\" animal or vegetable oil.") },
			{ "SG43", Res.GetString("3b9a4ac9-ade7-44ad-a175-9277bffffcc7", "Stow \"separated from\" bromine.") },
			{ "SG50", Res.GetString("cb6046e6-8768-469e-b9ae-49c3518d60af", "Segregation from foodstuffs as in 7.3.4.2.1, 7.6.3.1.2 or 7.7.3.6 of the IMDG Code.") },
			{ "SG57", Res.GetString("963b79c3-5381-4d38-9698-fa9ab5015ab1", "Stow \"separated from\" odor-absorbing cargoes.") },
			{ "SG68", Res.GetString("ec240059-7f5d-4659-9d41-5c30be66d225", "If flash point 60°C C.C. or below, segregation as for class 3 but \"away from\" class 4.1.") },
			{ "SG70", Res.GetString("5750013f-271c-4952-a0c4-026740b5c948", "For arsenic sulphides, \"separated from\" SGG1 - acids.") },
			{ "SG71", Res.GetString("9ed78b13-f9eb-45e0-a884-6ee7804425a6", "Within the appliance, to the extent that the dangerous goods are integral parts of the complete life-saving appliance, there is no need to apply the provisions on segregation of substances in chapter 7.2.") },
		};

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var messages = new List<Message>();

			foreach (var code in _segregationCodes.Keys)
			{
				if (substance1.SegregationCodes.Contains(code) || substance2.SegregationCodes.Contains(code))
				{
					messages.Add(new Message(MessageType.Warning, _segregationCodes[code]));
				}
			}

			return messages;
		}
	}
}
