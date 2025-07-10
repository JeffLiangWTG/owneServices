using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ImportApprovalCertificateNumberParser : RefCusCodeListParser
	{
		public ImportApprovalCertificateNumberParser()
		{
			CodeRegex = new Regex(@"^[0-9A-Z]{3,4}$");
		}

		const string Air = "AIR";
		const string Sea = "SEA";

		const string CertificateAttributeName = "CertificateType";

		const string TOKUTag = "TOKU";
		const string TOKGTag = "TOKG";
		const string TOKYTag = "TOKY";

		static readonly string[] specialCodeTags = {TOKUTag, TOKGTag, TOKYTag};
		readonly Regex CodeRegex;
		readonly string tokuDescription = "非該当または特例";
		string tokgDescription;
		string tokyDescription;

		public override bool HasAttribute => true;

		public override bool HasRefCusCodeOrAttributeTransportMode => true;

		public override string CurrentCodeType => currentCodeType;
		string currentCodeType;

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			refCusCodeLists = new List<RefCusCodeList>();

			LoadRecords(rows);

			var isTOKUWithSeaMode = false;
			var isTOKUWithAirMode = false;
			var isTOKGWithSeaMode = false;
			var isTOKGWithAirMode = false;
			var isTOKYWithSeaMode = false;
			var isTOKYWithAirMode = false;

			foreach (var record in CodeRecordDic.Values)
			{
				currentCodeType = !string.IsNullOrEmpty(record.RecordType) ? Constants.CodeType.ImportConstantApprovalCertificateNumber : Constants.CodeType.ImportApprovalCertificateNumber;
				AddRefCusCodeList(refCusCodeLists, record);

				isTOKUWithSeaMode = isTOKUWithSeaMode || (record.RecordType == TOKUTag && record.IsSea);
				isTOKUWithAirMode = isTOKUWithAirMode || (record.RecordType == TOKUTag && record.IsAir);

				if (record.RecordType == TOKGTag)
				{
					isTOKGWithSeaMode = isTOKGWithSeaMode || record.IsSea;
					isTOKGWithAirMode = isTOKGWithAirMode || record.IsAir;
					if (string.IsNullOrEmpty(tokgDescription))
					{
						tokgDescription = record.Description;
					}
					else
					{
						tokgDescription = tokgDescription + ", " + record.Description;
					}
				}

				if (record.RecordType == TOKYTag)
				{
					isTOKYWithSeaMode = isTOKYWithSeaMode || record.IsSea;
					isTOKYWithAirMode = isTOKYWithAirMode || record.IsAir;
					if (string.IsNullOrEmpty(tokyDescription))
					{
						tokyDescription = record.Description;
					}
					else
					{
						tokyDescription = tokyDescription + ", " + record.Description;
					}
				}

			}

			if (isTOKUWithSeaMode || isTOKUWithAirMode)
			{
				AddSpecialCodeToList(refCusCodeLists, isTOKUWithSeaMode, isTOKUWithAirMode, TOKUTag, tokuDescription);
			}

			if (isTOKGWithSeaMode || isTOKGWithAirMode)
			{
				AddSpecialCodeToList(refCusCodeLists, isTOKGWithSeaMode, isTOKGWithAirMode, TOKGTag, tokgDescription);
			}

			if (isTOKYWithSeaMode || isTOKYWithAirMode)
			{
				AddSpecialCodeToList(refCusCodeLists, isTOKYWithSeaMode, isTOKYWithAirMode, TOKYTag, tokyDescription);
			}

			if (refCusCodeLists.Any())
			{
				return true;
			}

			ErrorWriter.WriteError("No RefCusCodeList is parsed");
			return false;
		}

		void LoadRecords(IEnumerable<string> rows)
		{
			foreach (var row in rows)
			{
				var columns = row.Split(',');
				var code = columns[0].Trim();
				if (!CodeRegex.IsMatch(code))
				{
					continue;
				}

				var key = specialCodeTags.Any(x => x.Equals(code, System.StringComparison.OrdinalIgnoreCase)) ? columns[1].Trim() : code;

				if (CodeRecordDic.TryGetValue(key, out var record))
				{
					record.UpdateRecord(columns);
				}
				else
				{
					CodeRecordDic.Add(key, new ImportApprovalCertificateNumberRecord(key, columns));
				}
			}
		}

		void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, ImportApprovalCertificateNumberRecord record)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = record.Code,
				ZZD_Description = record.Description,
				ZZD_ZZK_NKCodeType = CurrentCodeType,
			};

			if (!string.IsNullOrEmpty(record.RecordType))
			{
				refCusCodeList.RefCusCodeListAttributes = new []
				{
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = CertificateAttributeName,
						ZZE_Value = record.RecordType,
					},
				};
			}

			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();
			if (record.IsSea)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Sea,
				});
			}

			if (record.IsAir)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Air,
				});
			}

			if (transportModeList.Count > 0)
			{
				refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModeList.ToArray();
			}

			refCusCodeLists.Add(refCusCodeList);
		}

		static void AddSpecialCodeToList(List<RefCusCodeList> refCusCodeLists, bool isSea, bool isAir, string tag, string description)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = tag,
				ZZD_Description = description,
				ZZD_ZZK_NKCodeType = Constants.CodeType.ImportApprovalCertificateNumber,
			};

			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();

			if (isSea)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Sea,
				});
			}

			if (isAir)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Air,
				});
			}

			if (transportModeList.Count > 0)
			{
				refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModeList.ToArray();
			}

			refCusCodeLists.Add(refCusCodeList);
		}

		Dictionary<string, ImportApprovalCertificateNumberRecord> CodeRecordDic
		{
			get
			{
				if (codeRecordDic == null)
				{
					codeRecordDic = new Dictionary<string, ImportApprovalCertificateNumberRecord>();
				}

				return codeRecordDic;
			}
		}

		Dictionary<string, ImportApprovalCertificateNumberRecord> codeRecordDic;

		sealed class ImportApprovalCertificateNumberRecord
		{
			public ImportApprovalCertificateNumberRecord(string code, string[] columns)
			{
				IsAir = columns[3].Contains("空");
				IsSea = columns[3].Contains("海");
				RecordType = specialCodeTags.Any(x => x.Equals(columns[0].Trim(), System.StringComparison.OrdinalIgnoreCase)) ? columns[0].Trim() : string.Empty;
				Code = code;
				UpdateRecord(columns);
			}

			public string Code { get; internal set; }

			public bool IsAir { get; }

			public bool IsSea { get; }

			public string RecordType { get; }

			public string Description => GetZZD_Description();

			string GetZZD_Description()
			{
				var description = string.Join(",", Descritpions);
				return string.IsNullOrWhiteSpace(description) ? Code : description;
			}

			public void UpdateRecord(string[] columns)
			{
				if (string.IsNullOrEmpty(RecordType))
				{
					var descriptionPart1 = columns[1].Trim();
					var descriptionPart2 = columns[2].Trim();

					var description = string.Join(" - ", new[] { descriptionPart1, descriptionPart2 }.Where(c => !string.IsNullOrWhiteSpace(c)));
					
					if (!string.IsNullOrWhiteSpace(description))
					{
						Descritpions.Add(description);
					}
				}
				else
				{
					var description = columns[2].Trim();
					if (!string.IsNullOrWhiteSpace(description))
					{
						Descritpions.Add(description);
					}
				}
			}

			HashSet<string> Descritpions
			{
				get
				{
					if (descritpions == null)
					{
						descritpions = new HashSet<string>();
					}

					return descritpions;
				}
			}

			HashSet<string> descritpions;

			public List<string> Attributes
			{
				get
				{
					if (attributes == null)
					{
						attributes = new List<string>();
					}

					return attributes;
				}
			}

			List<string> attributes;
		}
	}
}
