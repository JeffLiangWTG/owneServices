using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business
{
	class DefaultCustomsStatusStore
	{
		public static DefaultCustomsStatusStore CreateCustomsStatusStore(string countryCode, BusinessObjectFactory factory)
		{
			var result = countryCode switch
			{
				CountryCodes.Australia => new AUCustomsStatusStore(factory),
				CountryCodes.Ireland => new IECustomsStatusStore(factory),
				CountryCodes.NewZealand => new NZCustomsStatusStore(factory),
				CountryCodes.Spain => new ESCustomsStatusStore(factory),
				CountryCodes.UnitedStates => new USCustomsStatusStore(factory),
				CountryCodes.UnitedKingdom => new GBCustomsStatusStore(factory),
				_ => new DefaultCustomsStatusStore(factory),
			};

			return result;
		}

		public DefaultCustomsStatusStore(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			InitialiseCustomsStatusDictionary();
		}

		protected readonly BusinessObjectFactory Factory;

		public string GetReleaseStatus(string customsStatusCode, bool isImport, BaseJobDeclaration standaloneDeclaration)
		{
			return GetCustomsStatusInfo(customsStatusCode, isImport, HasFormalDeclaration(standaloneDeclaration))?.ReleaseStatus ?? string.Empty;
		}

		public string GetCustomStatusDescription(string customsStatusCode, bool isImport, BaseJobDeclaration standaloneDeclaration)
		{
			return customsStatusCode.IsNullOrEmpty()
				? string.Empty
				: GetCustomsStatusInfo(customsStatusCode, isImport, HasFormalDeclaration(standaloneDeclaration))?.Description ?? string.Empty;
		}

		public CodeDescriptionPairList GetAllRefCusCodeList(bool isImport)
		{
			var result = new CodeDescriptionPairList();
			var customsStatusList = new List<CustomsStatusInfo>();
			var codeTypes = isImport ? ImportCodeTypes : ExportCodeTypes;
			foreach (var codeType in codeTypes)
			{
				if (CustomsStatusDictionary.TryGetValue(codeType, out var customsStatusCodeList))
				{
					customsStatusList.AddRange(customsStatusCodeList.Values);
				}
				else
				{
					throw new ArgumentException($"Customs Code Type [{codeType}] is invalid");
				}
			}

			foreach (var customStatus in customsStatusList.Distinct().OrderBy(c => c.Code))
			{
				result.AddPair(customStatus.Code, customStatus.Description);
			}

			return result;
		}

		protected virtual IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
		{
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, CountryCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			if (!string.IsNullOrWhiteSpace(codeType))
			{
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, codeType);
			}

			var customsStatusList = Factory.Load<ZZRefCusCodeListCombined>(query);
			if (customsStatusList != null)
			{
				foreach (var customsStatus in customsStatusList)
				{
					var releaseStatus = customsStatus.Attributes.GetAttributeValue(eCommerceReleaseStatus);
					yield return new CustomsStatusInfo(customsStatus.ZZD_Code, customsStatus.ZZD_CodeType, customsStatus.ZZD_Description, releaseStatus.IsEmpty ? HVLVReleaseStatus.None : releaseStatus);
				}
			}
		}

		protected virtual string GetCodeTypeForExport()
		{
			return RefCusCodeListTypes.Codes.CustomsStatus;
		}

		protected virtual string GetCodeTypeForImport(bool hasFormalDeclaration)
		{
			return hasFormalDeclaration
				? RefCusCodeListTypes.Codes.CustomsStatusForInterface
				: RefCusCodeListTypes.Codes.CustomsStatus;
		}

		protected virtual bool HasFormalDeclaration(BaseJobDeclaration standaloneDeclaration) =>
			standaloneDeclaration != null && !standaloneDeclaration.IsCancelled;

		protected virtual string[] ImportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatusForInterface, RefCusCodeListTypes.Codes.CustomsStatus };

		protected virtual string[] ExportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatus };

		protected virtual ZString CountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		CustomsStatusInfo GetCustomsStatusInfo(string customsStatusCode, bool isImport, bool hasFormalDeclaration)
		{
			CustomsStatusInfo result = null;
			var codeType = isImport ? GetCodeTypeForImport(hasFormalDeclaration) : GetCodeTypeForExport();

			if (CustomsStatusDictionary.TryGetValue(codeType, out var customsStatusCodeList))
			{
				if (!customsStatusCodeList.TryGetValue(customsStatusCode, out result))
				{
					var codeList = string.Join(",", customsStatusCodeList.Values.OrderBy(x => x.Code).Select(x => x.Code));
					throw new CustomsStatusCodeNotExistInCodeListException(customsStatusCode, codeType, codeList, CountryCode);
				}
			}
			else
			{
				throw new ArgumentException($"Customs Code Type [{codeType}] is invalid");
			}

			return result;
		}

		void InitialiseCustomsStatusDictionary()
		{
			CustomsStatusDictionary.Clear();
			var allCodeTypes = ImportCodeTypes.Concat(ExportCodeTypes).Distinct().ToList();
			foreach (var codeType in allCodeTypes)
			{
				CustomsStatusDictionary.Add(codeType, new Dictionary<string, CustomsStatusInfo>());
			}

			foreach (var codeType in allCodeTypes)
			{
				var customsStatusList = GetCustomsStatusListForCodeType(codeType);
				foreach (var customsStatusInfo in customsStatusList)
				{
					if (!CustomsStatusDictionary[codeType].ContainsKey(customsStatusInfo.Code))
					{
						CustomsStatusDictionary[codeType].Add(customsStatusInfo.Code, customsStatusInfo);
					}
				}
			}
		}

		Dictionary<string, Dictionary<string, CustomsStatusInfo>> CustomsStatusDictionary => customsStatusDictionary ??= new Dictionary<string, Dictionary<string, CustomsStatusInfo>>();
		Dictionary<string, Dictionary<string, CustomsStatusInfo>> customsStatusDictionary;
		const string eCommerceReleaseStatus = "EcommerceReleaseStatus";
	}

	public class CustomsStatusInfo : IEquatable<CustomsStatusInfo>
	{
		public CustomsStatusInfo(string code, string type, string description, string releaseStatus)
		{
			Code = code;
			Type = type;
			Description = description;
			ReleaseStatus = releaseStatus;
		}

		public string Code { get; internal set; }

		public string Type { get; internal set; }

		public string Description { get; internal set; }

		public string ReleaseStatus { get; internal set; }

		bool IEquatable<CustomsStatusInfo>.Equals(CustomsStatusInfo other)
		{
			return string.Equals(Code.Trim(), other.Code.Trim(), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(Description.Trim(), other.Description.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			var codeHashCode = string.IsNullOrEmpty(Code) ? 0 : Code.Trim().GetHashCode();
			var descriptionHashCode = string.IsNullOrEmpty(Description) ? 0 : Description.Trim().GetHashCode();
			return codeHashCode ^ descriptionHashCode;
		}
	}
}
