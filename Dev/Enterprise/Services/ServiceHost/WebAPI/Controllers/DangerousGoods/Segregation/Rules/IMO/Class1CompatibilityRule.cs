using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class Class1CompatibilityRule : ISegregationRule
	{
		public string ApplicableStandard => UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		readonly HashSet<ZString> compatibleGroups = new HashSet<ZString> { "AA", "BB", "BS", "CC", "CG", "CS", "DD", "DG", "DS", "EE", "EG", "ES", "FF", "FS", "GC", "GD", "GE", "GG", "GS",
			"HH", "HS", "JJ", "JS", "KK", "KS", "NN", "NS", "SB", "SC", "SD", "SE", "SF", "SG", "SH", "SJ", "SK", "SN", "SS" };

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var segregationMessage = Res.GetString("2067735F-2B1D-49DF-A685-38405FAD3D9B",
				"These dangerous goods require segregation because of incompatible \"Class 1 compatibility groups (letters A to S)\".");

			var classNotFoundMessage = Res.GetString("F92582AE-4DCF-41DC-B594-08BD552666EF",
				"The class is empty for the dangerous goods substance");

			var messages = new List<Message>();

			var dgClass1 = substance1.DG_Class;
			var dgClass2 = substance2.DG_Class;

			if (dgClass1.IsEmpty || dgClass2.IsEmpty)
			{
				throw new DataNotFoundException(classNotFoundMessage);
			}

			if (dgClass1.StartsWith("1") && dgClass2.StartsWith("1"))
			{
				var key = dgClass1.KeepAlphabeticCharacters() + dgClass2.KeepAlphabeticCharacters();
				if (!compatibleGroups.Contains(key))
				{
					messages.Add(new Message(MessageType.Error, segregationMessage));
				}
			}
			return messages;
		}
	}
}
