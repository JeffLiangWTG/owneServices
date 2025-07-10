using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceLookups : AutoUNDGSubstanceLookups
	{
		public UNDGSubstanceLookups(AutoUNDGSubstance parent)
			: base(parent)
		{
		}

		#region Standard Types

		public static class UNDGSubstanceStandardTypes
		{
			public const string ADN = "ADN";
			public const string ADR = "ADR";
			public const string IATA = "IAT";
			public const string IMO = "IMO";
			public const string JTT = "JTT";
			public const string RID = "RID";
			public const string CFR = "CFR";
		}

		public static class UNDGSubstanceModes
		{
			public const string AIR = "AIR";
			public const string SEA = "SEA";
			public const string ROA = "ROA";
		}

		public CodeDescriptionPairList StandardTypes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(UNDGSubstanceStandardTypes.IMO, Res.GetString("0E050412-E38A-449A-A2DE-498F25AD747C", "IMO IMDG (International Maritime Dangerous Goods) Code"));
				list.AddPair(UNDGSubstanceStandardTypes.IATA, Res.GetString("4669B601-EEFD-4DFF-B47F-754B37C7AA12", "IATA DGR (Dangerous Goods Regulations)"));
				list.AddPair(UNDGSubstanceStandardTypes.JTT, Res.GetString("eaff19f1-cae7-48b6-ac53-f9b5dbb39e28", "JT/T 617 Part 3 (China - Carriage of Dangerous Goods by Road)"));
				list.AddPair(UNDGSubstanceStandardTypes.ADR, Res.GetString("2f72fb18-c7fb-6e9c-4899-463284e4cdbe", "ADR Part 3 (International Carriage of Dangerous Goods by Road)"));
				list.AddPair(UNDGSubstanceStandardTypes.RID, Res.GetString("7cc98b31-e83a-0286-4811-da6e0c356e01", "RID (International Carriage of Dangerous Goods by Rail)"));
				list.AddPair(UNDGSubstanceStandardTypes.ADN, Res.GetString("fd422613-6e67-cbb1-44c3-5ea9db272d1d", "ADN (International Carriage of Dangerous Goods by Inland Waterways) "));
				list.AddPair(UNDGSubstanceStandardTypes.CFR, Res.GetString("93d24c68-b803-41bd-8aa2-0c03d6e4a597", "49 CFR Hazmat Transport Regulations"));
				return list;
			}
		}

		#endregion

		#region Marine Pollutants

		public static class MarinePollutantTypes
		{
			public static string MarinePollutant
			{
				get { return Res.GetString("8137836b-0c64-49a2-a304-6659fa86d865", "Marine Pollutant"); }
			}
			public static string SevereMarinePollutant
			{
				get { return Res.GetString("6d9469a9-7cc1-45bd-94c1-0f60b4f4bee4", "Severe Marine Pollutant"); }
			}
			public static string Depends
			{
				get { return Res.GetString("a5a63258-47e9-4224-b080-23bad9154a41", "Depends"); }
			}

			public const string MarinePollutant_Code = "Y";
			public const string SevereMarinePollutant_Code = "S";
			public const string Depends_Code = "C";
		}

		public static CodeDescriptionPairList GetMarinePollutantList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstanceLookups.MarinePollutantList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(MarinePollutantTypes.MarinePollutant_Code, MarinePollutantTypes.MarinePollutant);
				list.AddPair(MarinePollutantTypes.SevereMarinePollutant_Code, MarinePollutantTypes.SevereMarinePollutant);
				list.AddPair(MarinePollutantTypes.Depends_Code, MarinePollutantTypes.Depends);

				return list;
			});
		}

		public CodeDescriptionPairList MarinePollutantList => GetMarinePollutantList(Factory);

		#endregion

		#region State

		public static class StateTypes
		{
			public static class Code
			{
				public const string Liquid = "L";
				public const string Gas = "G";
				public const string Solid = "S";
				public const string ExplosiveSubstance = "E";
				public const string ExplosiveArticle = "A";
			}

			public static class Description
			{
				public static string Liquid
				{
					get { return Res.GetString("bf382344-7463-4e8a-a19f-8c51c27d28a6", "Liquid"); }
				}
				public static string Gas
				{
					get { return Res.GetString("992218ee-7d7e-4686-8a81-5ec8db5053bf", "Gas"); }
				}
				public static string Solid
				{
					get { return Res.GetString("6f5d8861-29c7-4f67-978b-27d6460b8494", "Solid"); }
				}
				public static string ExplosiveSubstance
				{
					get { return Res.GetString("e86d3c7e-9406-429c-892f-3050a755e37d", "Explosive Substance"); }
				}
				public static string ExplosiveArticle
				{
					get { return Res.GetString("40f877d9-20d5-4f82-8ab5-29cad0f4f426", "Explosive Article"); }
				}
			}
		}

		public CodeDescriptionPairList StateList => GetStateList(Factory);

		public static CodeDescriptionPairList GetStateList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstanceLookups.StateList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(StateTypes.Code.Liquid, StateTypes.Description.Liquid);
				list.AddPair(StateTypes.Code.Solid, StateTypes.Description.Solid);
				list.AddPair(StateTypes.Code.Gas, StateTypes.Description.Gas);
				list.AddPair(StateTypes.Code.ExplosiveSubstance, StateTypes.Description.ExplosiveSubstance);
				list.AddPair(StateTypes.Code.ExplosiveArticle, StateTypes.Description.ExplosiveArticle);

				return list;
			});
		}

		#endregion

		#region Common Provisions

		public UNDGCommonDataCollection SpecialProvisions
		{
			get { return new UNDGCommonDataCollection(Factory, UNDGCommonDataLookups.TypeConstants.SpecialProvisions); }
		}

		public UNDGCommonDataCollection StowageSegmentationRequirements
		{
			get { return new UNDGCommonDataCollection(Factory, UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements); }
		}

		#endregion

		public UNDGCountryReferenceCollection UNDGCountryReferences => GetUNDGCountryReferences(Factory);

		public static UNDGCountryReferenceCollection GetUNDGCountryReferences(BusinessObjectFactory factory) => new UNDGCountryReferenceCollection(factory);

		#region Languages

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region DGClassList

		public CodeDescriptionPairList DGClassList
		{
			get { return UNDGDataItemLookups.GetDGClassList(Factory); }
		}

		#endregion

		#region Packing Group

		public static class PackingGroupTypes
		{
			public static string HighDanger
			{
				get { return Res.GetString("86E73EFA-A8E4-4A3D-87AF-159D02DC3857", "High Danger"); }
			}

			public static string MediumDanger
			{
				get { return Res.GetString("705421CB-9760-4E82-ADA9-F03F3854DB63", "Medium Danger"); }
			}

			public static string LowDanger
			{
				get { return Res.GetString("F0CE3A82-7270-455A-B57B-0085106C3C65", "Low Danger"); }
			}

			public const string HighDangerCode = "I";
			public const string MediumDangerCode = "II";
			public const string LowDangerCode = "III";
		}

		public CodeDescriptionPairList PackingGroupList => GetPackingGroupList(Factory);

		public static CodeDescriptionPairList GetPackingGroupList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstance_PackingGroup", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(PackingGroupTypes.HighDangerCode, PackingGroupTypes.HighDanger);
				list.AddPair(PackingGroupTypes.MediumDangerCode, PackingGroupTypes.MediumDanger);
				list.AddPair(PackingGroupTypes.LowDangerCode, PackingGroupTypes.LowDanger);

				return list;
			});
		}

		#endregion

		public CodeDescriptionPairList EmergencyResponseGuideList
		{
			get
			{
				if (fEmergencyResponseGuideList == null)
				{
					fEmergencyResponseGuideList = new UntranslatableCodeDescriptionPairList((NoResString)"Emergency Response Guide cannot be translated.");
					foreach (CodeDescriptionPair code in new EmergencyResponseGuideList())
					{
						fEmergencyResponseGuideList.Add(code);
					}
				}
				return fEmergencyResponseGuideList;
			}
		}
		CodeDescriptionPairList fEmergencyResponseGuideList;

		#region Excepted Quantities

		public static class ExceptedQuantity
		{
			public static class Code
			{
				public const string E0 = "E0";
				public const string E1 = "E1";
				public const string E2 = "E2";
				public const string E3 = "E3";
				public const string E4 = "E4";
				public const string E5 = "E5";
			}

			public static class Description
			{
				public static MultilingualString E0 => ResString.GetMultilingualString("8f5602fe-07a3-8891-4a55-551be22ce0e8", "Not Permitted as Excepted Quantity");
				public static MultilingualString E1 => ResString.GetMultilingualString("f6a54e6c-70d4-4e8a-9f64-83527083e33d", "Max Net Quantity - per INNER pack 30 g/30 ml, per OUTER pack 1 kg/1 L");
				public static MultilingualString E2 => ResString.GetMultilingualString("65fc4f92-4f19-4b25-b243-2f30e3da7923", "Max Net Quantity - per INNER pack 30 g/30 ml, per OUTER pack 500 g/500 ml");
				public static MultilingualString E3 => ResString.GetMultilingualString("c78f1b11-e24a-486e-8c72-b0c366ad9292", "Max Net Quantity - per INNER pack 30 g/30 ml, per OUTER pack 300 g/300 ml");
				public static MultilingualString E4 => ResString.GetMultilingualString("cb10b28d-6a88-4830-bb11-31a8ad813767", "Max Net Quantity - per INNER pack 1 g/1 ml, per OUTER pack 500 g/500 ml");
				public static MultilingualString E5 => ResString.GetMultilingualString("8d1cab32-d4ee-4408-a6a7-a9c20dd890a7", "Max Net Quantity - per INNER pack 1 g/1 ml, per OUTER pack 300 g/300 ml");
			}
		}

		public static CodeDescriptionPairList GetExceptedQuantityList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstanceLookups.ExceptedQuantityList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ExceptedQuantity.Code.E0, ExceptedQuantity.Description.E0);
				list.AddPair(ExceptedQuantity.Code.E1, ExceptedQuantity.Description.E1);
				list.AddPair(ExceptedQuantity.Code.E2, ExceptedQuantity.Description.E2);
				list.AddPair(ExceptedQuantity.Code.E3, ExceptedQuantity.Description.E3);
				list.AddPair(ExceptedQuantity.Code.E4, ExceptedQuantity.Description.E4);
				list.AddPair(ExceptedQuantity.Code.E5, ExceptedQuantity.Description.E5);

				return list;
			});
		}

		public CodeDescriptionPairList ExceptedQuantityList => GetExceptedQuantityList(Factory);

		#endregion

		#region Limited Quantity Types

		public static class LimitedQuantityTypes
		{
			public static string NLM
			{
				get { return Res.GetString("CE52F221-CAA9-46AD-9371-07212DB4DC48", "Net Weight Limit"); }
			}

			public static string FOB
			{
				get { return Res.GetString("F5BECCAF-607B-4B7E-84A0-171E86B43638", "Forbidden"); }
			}

			public static string NRE
			{
				get { return Res.GetString("328F3054-9F98-4B61-8563-1A58ED3EF55E", "Not Restricted"); }
			}
			public static string NLT
			{
				get { return Res.GetString("CFF3749E-6038-426C-B381-C7071606F827", "No Limit"); }
			}

			public static string NAP
			{
				get { return Res.GetString("271450C8-EB8D-4513-86DE-C9BA799F35F6", "Non Applicable"); }
			}

			public static string GLM
			{
				get { return Res.GetString("09921360-75E9-43C7-92CF-C3AF3FEF1104", "Gross Weight Limit"); }
			}

			public const string NLMCode = "NLM";
			public const string FOBCode = "FOB";
			public const string NRECode = "NRE";
			public const string NLTCode = "NLT";
			public const string NAPCode = "NAP";
			public const string GLMCode = "GLM";
		}

		public CodeDescriptionPairList LimitedQuantityTypesList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(LimitedQuantityTypes.NLMCode, LimitedQuantityTypes.NLM);
				list.AddPair(LimitedQuantityTypes.FOBCode, LimitedQuantityTypes.FOB);
				list.AddPair(LimitedQuantityTypes.NRECode, LimitedQuantityTypes.NRE);
				list.AddPair(LimitedQuantityTypes.NLTCode, LimitedQuantityTypes.NLT);
				list.AddPair(LimitedQuantityTypes.NAPCode, LimitedQuantityTypes.NAP);
				list.AddPair(LimitedQuantityTypes.GLMCode, LimitedQuantityTypes.GLM);
				return list;
			}
		}

		#endregion

		#region Technical Name Required Types

		public static class TechNameTypes
		{
			public static class Code
			{
				public const string Required = "REQ";
				public const string NotRequired = "NRQ";
				public const string Other = "OTH";
			}

			public static class Description
			{
				public static string Required
				{
					get { return Res.GetString("9cb613a3-85f6-4544-9b1f-df534cd22d2b", "Required"); }
				}

				public static string NotRequired
				{
					get { return Res.GetString("4d59cd1f-beb7-4586-8e4f-705f79157808", "Not Required"); }
				}

				public static string Other
				{
					get { return Res.GetString("45ea2a51-ceaf-4e1b-ba16-04d160dade98", "Other generic entries in Appendix A"); }
				}
			}
		}

		public CodeDescriptionPairList TechNameList => GetTechNameList(Factory);

		public static CodeDescriptionPairList GetTechNameList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstanceLookups.TechNameList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(TechNameTypes.Code.Required, TechNameTypes.Description.Required);
				list.AddPair(TechNameTypes.Code.NotRequired, TechNameTypes.Description.NotRequired);
				list.AddPair(TechNameTypes.Code.Other, TechNameTypes.Description.Other);

				return list;
			});
		}

		public static string GetTechNameFromCharacter(string identifier)
		{
			switch (identifier)
			{
				case "*":
					return TechNameTypes.Code.Required;
				case "+":
					return TechNameTypes.Code.Other;
				case "":
					return TechNameTypes.Code.NotRequired;
				default:
					return identifier;
			}
		}

		public static string GetCharacterForTechName(string techName)
		{
			switch (techName)
			{
				case TechNameTypes.Code.Required:
					return "*";
				case TechNameTypes.Code.Other:
					return "+";
				case TechNameTypes.Code.NotRequired:
					return "";
				default:
					return null;
			}
		}

		public static class SpecialProvisionType
		{
			public static class Code
			{
				public const string PassengerAndCargo = "A1";
				public const string CargoOnly = "A2";
			}
		}

			#endregion
		}
}
