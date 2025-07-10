using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.PassarCodeListsSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists
{
	public class PassarCodeListsParser : BaseCodeListsParser<codeLists>
	{
		public PassarCodeListsParser(DownloadResult download) : base(download)
		{
		}

		public override IEnumerable<MappingConfig> DefaultListTypeMappings => new MappingConfig[]
		{
			new MappingConfig { DomainNames = new[] { "NCL0231" }, ListType = "N0231", WithLanguagesList = true, AdditionalCodes = AdditionalCodesN0231},
			new MappingConfig { DomainNames = new[] { "NCL0239" }, ListType = "AI44N", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL0251" }, ListType = "N0251", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL0252" }, ListType = "N0252", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL0296" }, ListType = "N0296", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL0380" }, ListType = "AR44N", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL0754" }, ListType = "TD44N", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1053" }, ListType = "N1053", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1054" }, ListType = "N1054", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1057" }, ListType = "N1057", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1110" }, ListType = "RFNDT", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1213" }, ListType = "DC44E", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1214" }, ListType = "DC40E", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1115" }, ListType = "PRMAP", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1117", "NCL1118" }, ListType = "AI44E", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1119" }, ListType = "N1119", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1121" }, ListType = "N1121", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1123" }, ListType = "N1123", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1141" }, ListType = "N1141", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1150" }, ListType = "N1150", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL1212" }, ListType = "DC44N", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL1214" }, ListType = "DC40N", WithLanguagesList = true, WithAttributeList = true},
			new MappingConfig { DomainNames = new[] { "NCL2000" }, ListType = "N2000", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL3000" }, ListType = "N3000", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5001" }, ListType = "N5001", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5002" }, ListType = "N5002", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5003" }, ListType = "N5003", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5004" }, ListType = "N5004", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5005" }, ListType = "N5005", WithLanguagesList = true},
			new MappingConfig { DomainNames = new[] { "NCL5010" }, ListType = "N5010", WithLanguagesList = true},
		};

		protected override string LogFileSuffix => "PassarCodeLists";

		protected override IEnumerable<RefCusCodeList> ConvertDomain(codeLists inputDoc, DateTime actualDate, MappingConfig mapping)
		{
			foreach (var inputEntry in from codeList in inputDoc.codeList
									   where mapping.DomainNames.Contains(codeList.id)
									   from code in codeList.code
									   where code.validFrom <= actualDate && code.validTo >= actualDate
									   group (codeList, code) by code.codeNr into g
									   select g)
			{
				var code = inputEntry.First().code;
				yield return new RefCusCodeList
				{
					ZZD_Code = code.codeNr,
					ZZD_Description = code.MeaningDe.Truncate(2000),
					ZZD_StartDate = code.validFrom.Truncate(),
					ZZD_EndDate = code.validTo.Truncate().EndOfDay(),
					RefCusCodeListLanguages = ConvertLanguages(code),
					RefCusCodeListAttributes = ConvertAttributes(mapping, inputEntry.Select(c => c.codeList.id).ToArray(), code.codeNr),
				};
			}
		}

		IEnumerable<RefCusCodeList> AdditionalCodesN0231()
		{
			yield return new RefCusCodeList
			{
				ZZD_Code = "T-CH",
				ZZD_Description = "Nationaler Transit Schweiz",
				ZZD_StartDate = new DateTime(2000, 1, 1),
				ZZD_EndDate = Helper.MaximumDateTime.EndOfDay(),
				RefCusCodeListLanguages = new RefCusCodeListLanguage[]
				{
					new RefCusCodeListLanguage { ZXA_ZX6_NKLanguage = "FR", ZXA_Description = "Transit nationale Suisse" },
					new RefCusCodeListLanguage { ZXA_ZX6_NKLanguage = "IT", ZXA_Description = "Transito nazionale Svizzera" },
					new RefCusCodeListLanguage { ZXA_ZX6_NKLanguage = "EN", ZXA_Description = "National Transit Switzerland" },
				}
			};
		}

		static RefCusCodeListAttribute[] ConvertAttributes(MappingConfig mapping, string[] listIds, string code)
		{
			switch (mapping.ListType)
			{
				case "AI44E":
					return GetAttributesAI44E(listIds);
				case "AI44N":
					return GetAttributesAI44N();
				case "AR44N":
					return GetAttributesAR44N(code);
				case "DC40N":
					return GetAttributesDC40N();
				case "DC44N":
					return GetAttributesDC44N();
				case "PRMAP":
					return GetPermitNumberAllowedPRMAP(code).Concat(GetPermitExceptionReasonAllowedPRMAP(code)).Concat(GetAttributeAdditionalInformationPRMAP(code)).ToArray();
				case "TD44N":
					return GetAttributesTD44N();
				case "N1119":
					return GetRestrictionAttributesN1119(code).Concat(GetLinkedCodeTypeAttributesN1119(code)).ToArray();
				case "N1121":
					return GetAttributesN1121(code);
				case "DC44E":
					return GetAttributesDC44E(code);
				default:
					return null;
			}
		}

		static RefCusCodeListAttribute[] GetAttributesAI44E(string[] listIds)
		{
			var resultList = new List<RefCusCodeListAttribute>();
			if (listIds.Contains(CodeListIds.NCL1117))
			{
				resultList.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header });
				resultList.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House });
			}
			if (listIds.Contains(CodeListIds.NCL1118))
			{
				resultList.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item });
			}
			return resultList.ToArray();
		}

		static RefCusCodeListAttribute[] GetAttributesAI44N()
		{
			return new RefCusCodeListAttribute[]
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item},
			};
		}

		static RefCusCodeListAttribute[] GetAttributesAR44N(string code)
		{
			var resultList = new List<RefCusCodeListAttribute>()
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header },
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House },
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item },
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Reference, ZZE_Value = AttributeValues.N }
			};

			if (code == CodeValues.C651 || code == CodeValues.C658)
			{
				resultList.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.DocumentTypeExcise, ZZE_Value = AttributeValues.Y });
			}

			return resultList.ToArray();
		}

		static RefCusCodeListAttribute[] GetAttributesDC40N()
		{
			return new RefCusCodeListAttribute[]
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Reference, ZZE_Value = AttributeValues.Y},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Complement, ZZE_Value = AttributeValues.N},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ItemNumber, ZZE_Value = AttributeValues.N},
			};
		}

		static RefCusCodeListAttribute[] GetAttributesDC44N()
		{
			return new RefCusCodeListAttribute[]
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Reference, ZZE_Value = AttributeValues.Y},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Complement, ZZE_Value = AttributeValues.N},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ItemNumber, ZZE_Value = AttributeValues.N},
			};
		}

		static RefCusCodeListAttribute[] GetAttributesTD44N()
		{
			return new RefCusCodeListAttribute[]
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Reference, ZZE_Value = AttributeValues.Y},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Complement, ZZE_Value = AttributeValues.N},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ItemNumber, ZZE_Value = AttributeValues.N},
			};
		}

		static RefCusCodeListAttribute[] GetRestrictionAttributesN1119(string code)
		{
			string[] restrictions = null;
			switch (code)
			{
				case "B1001":
					restrictions = new[] { "101", "102", "310", "410", "411", "412", "413", "414", "500", "501" };
					break;
				case "B1002":
					restrictions = new[] { "201", "310", "500", "501" };
					break;
				case "B1003":
					restrictions = new[] { "101", "102", "201", "310", "500", "501" };
					break;
				case "B1004":
					restrictions = new[] { "101", "102" };
					break;
				case "B1005":
					restrictions = new[] { "201" };
					break;
				case "N1003":
					restrictions = new[] { "310" };
					break;
				case "N1004":
					restrictions = new[] { "500", "501" };
					break;
				case "N1005":
					restrictions = new[] { "410" };
					break;
				case "N1006":
					restrictions = new[] { "411", "412" };
					break;
				case "N1007":
				case "N1008":
					restrictions = new[] { "801" };
					break;
				case "N1010":
					restrictions = new[] { "413" };
					break;
				case "N1011":
				case "N1012":
					restrictions = new[] { "601" };
					break;
			}
			return CreateAttributes(AttributeNames.RestrictionCode, restrictions);
		}

		static RefCusCodeListAttribute[] GetLinkedCodeTypeAttributesN1119(string code)
		{
			string[] linkedCodes = null;
			switch (code)
			{
				case "B1002":
					linkedCodes = new[] { "N5002" };
					break;
				case "N1003":
					linkedCodes = new[] { "N5003" };
					break;
				case "N1004":
					linkedCodes = new[] { "N5004" };
					break;
				case "N1005":
					linkedCodes = new[] { "N5005" };
					break;
				case "N1010":
					linkedCodes = new[] { "N5010" };
					break;
			}
			return CreateAttributes(AttributeNames.LinkedCodeType, linkedCodes);
		}

		public static RefCusCodeListAttribute[] GetAttributeAdditionalInformationPRMAP(string code)
		{
			RefCusCodeListAttribute[] attributes = Array.Empty<RefCusCodeListAttribute>();

			string attributeValue = null;
			switch (code)
			{
				case "101":
				case "102":
				case "201":
				case "310":
				case "410":
				case "411":
				case "412":
				case "413":
				case "414":
				case "500":
				case "501":
				case "601":
				case "801":
				case "802":
					attributeValue = AttributeValues.Y;
					break;
				case "311":
				case "399":
				case "401":
				case "402":
				case "620":
				case "998":
				case "999":
					attributeValue = AttributeValues.N;
					break;
			}

			if (attributeValue != null)
			{
				attributes = new[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.AdditionalInformation, ZZE_Value = attributeValue } };
			}

			return attributes;
		}

		public static RefCusCodeListAttribute[] GetPermitNumberAllowedPRMAP(string code)
		{
			RefCusCodeListAttribute[] attributes;

			string attributeValue;
			switch (code)
			{
				case "402":
				case "801":
				case "802":
					attributeValue = AttributeValues.N;
					break;
				default:
					attributeValue = AttributeValues.Y;
					break;
			}

			attributes = new[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.PermitNumberAllowed, ZZE_Value = attributeValue } };

			return attributes;
		}
		public static RefCusCodeListAttribute[] GetPermitExceptionReasonAllowedPRMAP(string code)
		{
			RefCusCodeListAttribute[] attributes;

			string attributeValue;
			switch (code)
			{
				case "101":
				case "102":
				case "310":
				case "311":
				case "410":
				case "500":
				case "501":
				case "510":
					attributeValue = AttributeValues.Y;
					break;
				default:
					attributeValue = AttributeValues.N;
					break;
			}

			attributes = new[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.PermitExceptionReasonAllowed, ZZE_Value = attributeValue } };

			return attributes;
		}


		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		static RefCusCodeListAttribute[] GetAttributesN1121(string code)
		{
			string[] authorities = null;
			switch (code)
			{
				case "SECOBWIP001":
				case "SECOBWIP002":
				case "SECOBWIP003":
				case "SECOBWIP005":
				case "SECOBWIP006":
				case "SECOBWIP007":
					authorities = new[] { "101" };
					break;
				case "SECOBWRP001":
				case "SECOBWRP002":
				case "SECOBWRP003":
					authorities = new[] { "102" };
					break;
				case "BLVCITES001":
				case "BLVCITES002":
				case "BLVCITES003":
				case "BLVCITES007":
				case "BLVCITES008":
					authorities = new[] { "310" };
					break;
				case "BLVCITES006":
					authorities = new[] { "310", "311" };
					break;
				case "BAGRADIO001":
					authorities = new[] { "410" };
					break;
				case "SMCBTM001":
				case "SMCBTM002":
				case "SMCBTM003":
				case "SMCBTM004":
				case "SMCBTM005":
				case "SMCBTM006":
				case "SMCBTM007":
				case "SMCBTM008":
					authorities = new[] { "500", "501" };
					break;
				case "SMCAMBB001":
				case "SMCAMBB002":
				case "SMCAMBB003":
				case "SMCAMBB004":
				case "SMCAMBB005":
					authorities = new[] { "510" };
					break;
			}
			return CreateAttributes(AttributeNames.PermitAuthority, authorities);
		}

		static RefCusCodeListAttribute[] GetAttributesDC44E(string code)
		{
			var attributes = new List<RefCusCodeListAttribute>()
			{
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Header},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.House},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Level, ZZE_Value = AttributeValues.Item},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Reference, ZZE_Value = AttributeValues.Y},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Complement, ZZE_Value = AttributeValues.N},
				new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ItemNumber, ZZE_Value = AttributeValues.N},
			};

			switch (code)
			{
				case "9541":
				case "9542":
				case "9543":
				case "9544":
					attributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.OriginDocument, ZZE_Value = AttributeValues.Y });
					break;
			}

			return attributes.ToArray();
		}

		static RefCusCodeListAttribute[] CreateAttributes(string attributeName, string[] values)
		{
			var attributes = new List<RefCusCodeListAttribute>();
			if (values != null)
			{
				foreach (var value in values)
				{
					attributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = attributeName, ZZE_Value = value });
				}
			}
			return attributes.ToArray();
		}

		class AttributeNames
		{
			internal const string Level = "Level";
			internal const string Reference = "Reference";
			internal const string Complement = "Complement";
			internal const string ItemNumber = "ItemNumber";
			internal const string DocumentTypeExcise = "DocumentTypeExcise";
			internal const string RestrictionCode = "RestrictionCode";
			internal const string AdditionalInformation = "AdditionalInformation";
			internal const string PermitAuthority = "PermitAuthority";
			internal const string OriginDocument = "OriginDocument";
			internal const string LinkedCodeType = "LinkedCodeType";
			internal const string PermitNumberAllowed = "PermitNumberAllowed";
			internal const string PermitExceptionReasonAllowed = "PermitExceptionReasonAllowed";
		}

		class AttributeValues
		{
			internal const string Header = "Header";
			internal const string House = "House";
			internal const string Item = "Item";
			internal const string N = "N";
			internal const string Y = "Y";
		}

		class CodeListIds
		{
			internal const string NCL1117 = "NCL1117";
			internal const string NCL1118 = "NCL1118";
		}

		class CodeValues
		{
			internal const string C651 = "C651";
			internal const string C658 = "C658";
		}
	}
}
