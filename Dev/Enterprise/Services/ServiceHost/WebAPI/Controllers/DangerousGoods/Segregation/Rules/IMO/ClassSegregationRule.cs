using System;
using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class ClassSegregationRule : ISegregationRule
	{
		static readonly Dictionary<string, List<string>> tableOfIncompatibleClasses = new Dictionary<string, List<string>>
		{
			{ "1.1", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "1.2", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "1.3", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "1.4", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.2", "7", "8" } },
			{ "1.5", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "1.6", new List<string> { "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "2.1", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.2", "7", "8" } },
			{ "2.2", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "3", "4.2", "5.2", "6.2", "7" } },
			{ "2.3", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "3", "4.2", "5.2", "6.2", "7" } },
			{ "3", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "4.2", "4.3", "5.1", "5.2", "6.2", "7" } },
			{ "4.1", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "4.2", "5.1", "5.2", "6.2", "7", "8" } },
			{ "4.2", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8" } },
			{ "4.3", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.2", "5.1", "5.2", "6.2", "7", "8" } },
			{ "5.1", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.2", "6.1", "6.2", "7", "8" } },
			{ "5.2", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "6.1", "6.2", "7", "8" } },
			{ "6.1", new List<string> { "1.1", "1.2", "1.3", "1.5", "1.6", "4.2", "5.1", "5.2", "6.2" } },
			{ "6.2", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "7", "8" } },
			{ "7", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.2", "8" } },
			{ "8", new List<string> { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.1", "4.1", "4.2", "4.3", "5.1", "5.2", "6.2", "7" } },
			{ "9", new List<string> { } }
		};

		public string ApplicableStandard => UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var imoClass1 = RemoveCompatibilityGroup(substance1.DG_Class);
			var subLabel1_1 = RemoveCompatibilityGroup(substance1.DG_SubLabel1);
			var subLabel1_2 = RemoveCompatibilityGroup(substance1.DG_SubLabel2);

			var imoClass2 = RemoveCompatibilityGroup(substance2.DG_Class);
			var subLabel2_1 = RemoveCompatibilityGroup(substance2.DG_SubLabel1);
			var subLabel2_2 = RemoveCompatibilityGroup(substance2.DG_SubLabel2);

			ValidateClass(substance1.PK, imoClass1);
			ValidateClass(substance2.PK, imoClass2);

			var allCombinations = GetValidCombinations(imoClass1, subLabel1_1, subLabel1_2, imoClass2, subLabel2_1, subLabel2_2);

			var messages = CheckCombinations(allCombinations);

			return messages;
		}

		string RemoveCompatibilityGroup(string value)
		{
			return value?.TrimEnd("ABCDEFGHJKLNS".ToCharArray()) ?? value;
		}

		List<Tuple<string, string>> GetValidCombinations(string imoClass1, string subLabel1_1, string subLabel1_2, string imoClass2, string subLabel2_1, string subLabel2_2)
		{
			var combinations = new List<Tuple<string, string>>();

			AddIfValid(combinations, imoClass1, imoClass2);
			AddIfValid(combinations, imoClass1, subLabel2_1);
			AddIfValid(combinations, imoClass1, subLabel2_2);
			AddIfValid(combinations, subLabel1_1, imoClass2);
			AddIfValid(combinations, subLabel1_1, subLabel2_1);
			AddIfValid(combinations, subLabel1_1, subLabel2_2);
			AddIfValid(combinations, subLabel1_2, imoClass2);
			AddIfValid(combinations, subLabel1_2, subLabel2_1);
			AddIfValid(combinations, subLabel1_2, subLabel2_2);

			return combinations;
		}

		void AddIfValid(List<Tuple<string, string>> combinations, string item1, string item2)
		{
			if (!string.IsNullOrEmpty(item1) && !string.IsNullOrEmpty(item2))
			{
				combinations.Add(new Tuple<string, string>(item1, item2));
			}
		}

		List<Message> CheckCombinations(List<Tuple<string, string>> allCombinations)
		{
			var messages = new List<Message>();

			foreach (var combination in allCombinations)
			{
				var class1 = combination.Item1;
				var class2 = combination.Item2;

				if (tableOfIncompatibleClasses.ContainsKey(class1) && tableOfIncompatibleClasses[class1].Contains(class2))
				{
					string errorMessage = Res.GetString("e0a0ed8a-6064-432a-8e61-35815bdc03cd", "These dangerous goods classes require segregation.");
					messages.Add(new Message(MessageType.Error, errorMessage));
				}
			}
			return messages;
		}

		void ValidateClass(ZGuid primaryKey, string imoClass)
		{
			if (string.IsNullOrEmpty(imoClass))
			{
				throw new DataCorruptionException($"Class segregation rule: class is empty for {primaryKey}");
			}
		}
	}
}
