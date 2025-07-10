using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class Filter
	{
		public string PropertyName { get; set; }
		public string PropertyValue { get; set; }
		public Operation Operation { get; set; }
		public bool ThisFilter { get; set; }
		public bool KeyFilter { get; set; }
	}

	enum Operation
	{
		Equal,
		NotEqual,
		Contains
	}

	public class SegregationFromGroupRule : ISegregationRule
	{
		string ISegregationRule.ApplicableStandard => UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		readonly Dictionary<(string code, string group), (Filter[] Filters, string ErrorMessage)> segregationCodeIncompatibleMappings = new Dictionary<(string code, string group), (Filter[], string)>
		{
			{ ("SG20", "SGG1"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG20", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG1", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("dd7597ce-be02-4924-b197-a91c12e7e6b7", "Stow \"away from\" SGG1 - acids.")) },
			{ ("SG21", "SGG18"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG21", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG18", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("58606450-4ff4-4ff1-ae1c-8384e3d7bed7", "Stow \"away from\" SGG18 - alkalis.")) },
			{ ("SG24", "SGG17"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG24", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG17", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("b53c899f-b105-4637-9204-4cefcc25bd0d", "Stow \"away from\" SGG17 - azides.")) },
			{ ("SG28", "SGG2"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG28", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("712b06be-68f4-476b-bd60-e54e0330f646", "Stow \"separated from\" SGG2 - ammonium compounds and explosives containing ammonium compounds or salts.")) },
			{ ("SG30", "SGG7"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG30", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG7", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("68bab533-245a-412f-95b9-881bad4cb286", "Stow \"away from\" SGG7 - heavy metals and their salts.")) },
			{ ("SG31", "SGG9"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG31", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG9", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("00416b65-c8a9-4eb3-a350-c8f9c4e2fdba", "Stow \"away from\" SGG9 - lead and its compounds.")) },
			{ ("SG32", "SGG10"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG32", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG10", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("9846edeb-2b64-45c3-b3af-31c1c3c52b26", "Stow \"away from\" SGG10 - liquid halogenated hydrocarbons.")) },
			{ ("SG33", "SGG15"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG33", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG15", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("aa1f499d-dffc-4de8-8e68-8f3bb79c2e3f", "Stow \"away from\" SGG15 - powdered metals.")) },
			{ ("SG34", "SGG4"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG34", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = true, KeyFilter = false }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG4", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("15f8f0f6-b558-4a4c-ac51-607f6be2e867", "When containing ammonium compounds, \"separated from\" SGG4 - chlorates or SGG13 - perchlorates and explosives containing chlorates or perchlorates.")) },
			{ ("SG34", "SGG13"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG34", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = true, KeyFilter = false }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG13", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("ed90b19a-91be-4c23-8bba-10611a14875d", "When containing ammonium compounds, \"separated from\" SGG4 - chlorates or SGG13 - perchlorates and explosives containing chlorates or perchlorates.")) },
			{ ("SG35", "SGG1"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG35", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG1", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("2bcde03a-a1a3-4d66-a4d0-16942b8d0e03", "Stow \"separated from\" SGG1 - acids.")) },
			{ ("SG36", "SGG18"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG36", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG18", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("3bcbe86b-26c9-4f24-83c5-9047102b8ee6", "Stow \"separated from\" SGG18 - alkalis.")) },
			{ ("SG38", "SGG2"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG38", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("60d67638-15d6-4ee3-a2ce-60f056b4c7dd", "Stow \"separated from\" SGG2 ammonium compounds.")) },
			{ ("SG39", "SGG2"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG39", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true }, new Filter { PropertyName = "DG_UNNO", PropertyValue = "1444", Operation = Operation.NotEqual, ThisFilter = false, KeyFilter = false } }, Res.GetString("a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6", "Stow \"separated from\" SGG2 ammonium compounds other than AMMONIUM PERSULPHATE (UN 1444).")) },
			{ ("SG40", "SGG2"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG40", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG2", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true }, new Filter { PropertyName = "DG_UNNO", PropertyValue = "1444", Operation = Operation.NotEqual, ThisFilter = false, KeyFilter = false } }, Res.GetString("b2c3d4e5-f6g7-h8i9-j0k1-l2m3n4o5p6q7", "Stow \"separated from\" SGG2 - ammonium compounds other than mixtures of ammonium persulphates and/or potassium persulphates and/or sodium persulphates.")) },
			{ ("SG42", "SGG3"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG42", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG3", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("17817e0c-268f-481a-a652-fd849a92e93d", "Stow \"separated from\" SGG3 - bromates.")) },
			{ ("SG45", "SGG4"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG45", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG4", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("f877f383-c0cc-4cee-8cba-b04960e2b257", "Stow \"separated from\" SGG4 - chlorates.")) },
			{ ("SG47", "SGG5"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG47", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG5", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("252f507a-367b-4fc9-998e-a8bd0dc07201", "Stow \"separated from\" SGG5 - chlorites.")) },
			{ ("SG49", "SGG6"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG49", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG6", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("d9537d44-614c-4d7b-9ca0-16e031c5017f", "Stow \"separated from\" SGG6 - cyanides.")) },
			{ ("SG51", "SGG8"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG51", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG8", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("6bbec0d3-a059-47d7-8271-506b25bce7a4", "Stow \"separated from\" SGG8 - hypochlorites.")) },
			{ ("SG54", "SGG11"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG54", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG11", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("9bc1aa1b-5048-4df8-90be-b41a942c6b47", "Stow \"separated from\" SGG11 - mercury and mercury compounds.")) },
			{ ("SG56", "SGG12"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG56", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG12", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("ca17c9ce-a91a-47d2-b112-fc04f73c5046", "Stow \"separated from\" SGG12 - nitrites.")) },
			{ ("SG58", "SGG13"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG58", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG13", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("a8decc49-63c8-42f3-b6ab-f6a2f7a1c3ce", "Stow \"separated from\" SGG13 - perchlorates.")) },
			{ ("SG59", "SGG14"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG59", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG14", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("a915a899-a1ca-4662-a335-992c1e82a829", "Stow \"separated from\" SGG14 - permanganates.")) },
			{ ("SG60", "SGG16"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG60", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG16", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("050e9314-a217-4211-a00d-5bdec81ed86d", "Stow \"separated from\" SGG16 - peroxides.")) },
			{ ("SG61", "SGG15"), (new[] { new Filter { PropertyName = "SegregationCodes", PropertyValue = "SG61", Operation = Operation.Contains, ThisFilter = true, KeyFilter = true }, new Filter { PropertyName = "SegregationGroups", PropertyValue = "SGG15", Operation = Operation.Contains, ThisFilter = false, KeyFilter = true } }, Res.GetString("ac85a115-bf9a-409d-94df-215b0487aba3", "Stow \"separated from\" SGG15 - powdered metals.")) }
		};

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var messages = new List<Message>();

			GetErrorMessagesForInCompatiblePair(substance1, substance2, messages);
			GetErrorMessagesForInCompatiblePair(substance2, substance1, messages);

			return messages;
		}

		void GetErrorMessagesForInCompatiblePair(UNDGSubstance substance1, UNDGSubstance substance2, List<Message> messages)
		{
			foreach (var code in substance1.SegregationCodes)
			{
				foreach (var group in substance2.SegregationGroups)
				{
					var key = (code.ToString(), group.ToString());
					if (segregationCodeIncompatibleMappings.TryGetValue(key, out var value))
					{
						var nonKeyFilters = value.Filters.Where(filter => !filter.KeyFilter).ToArray();
						if (nonKeyFilters.Length == 0 || nonKeyFilters.All(filter => EvaluateFilter(filter, substance1, substance2)))
						{
							messages.Add(new Message(MessageType.Error, value.ErrorMessage));
						}
					}
				}
			}
		}

		static bool EvaluateFilter(Filter filter, UNDGSubstance substance1, UNDGSubstance substance2)
		{
			return filter.ThisFilter ? EvaluateFilter(filter, substance1) : EvaluateFilter(filter, substance2);
		}
		static bool EvaluateFilter(Filter filter, UNDGSubstance substance) => filter.Operation switch
		{
			Operation.Equal => substance[filter.PropertyName]?.ToString() == filter.PropertyValue,
			Operation.NotEqual => substance[filter.PropertyName]?.ToString() != filter.PropertyValue,
			Operation.Contains => (substance[filter.PropertyName] as IEnumerable<object>)?.Any(item => item?.ToString() == filter.PropertyValue) ?? false,
			_ => throw new NotSupportedException($"Operation {filter.Operation} not found"),
		};
	}
}
