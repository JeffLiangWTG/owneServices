using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public class SegregationQueryValidator : ISegregationQueryValidator
	{
		public (bool validationResult, string validationMessage) IsQueryValid(SegregationQuery segregationQuery)
		{
			if (segregationQuery == null)
			{
				return (false, Res.GetString("60ee6f64-59a9-4a30-8661-0f0d8155c1b8", "Query cannot be null"));
			}

			var (standardValidationResult, standardValidationMessage) = IsStandardListValid(segregationQuery.Standards);
			if (!standardValidationResult)
			{
				return (standardValidationResult, standardValidationMessage);
			}

			var (idValidationResult, idValidationMessage) = IsEntityListValid(segregationQuery.Ids, nameof(segregationQuery.Ids));
			if (!idValidationResult)
			{
				return (idValidationResult, idValidationMessage);
			}

			var ids = segregationQuery.Ids;
			if (ids.Distinct().Count() != ids.Length)
			{
				return (false, Res.GetString("048764fc-3cda-4f7d-880e-5451caef5035", "List of ids cannot contain duplicates"));
			}

			return (true, string.Empty);
		}

		public (bool validationResult, string validationMessage) IsEntityQueryValid(SegregationEntityQuery segregationEntityQuery)
		{
			if (segregationEntityQuery == null)
			{
				return (false, Res.GetString("a6d951e4-402a-4bda-a2ea-b15d624a87dc", "Query cannot be null"));
			}

			var (standardValidationResult, standardValidationMessage) = IsStandardListValid(segregationEntityQuery.Standards);
			if (!standardValidationResult)
			{
				return (standardValidationResult, standardValidationMessage);
			}

			var (classificationDataValidationResult, classificationDataValidationMessage) = IsEntityListValid(segregationEntityQuery.UNDGDataItemDTOs, nameof(segregationEntityQuery.UNDGDataItemDTOs));
			if (!classificationDataValidationResult)
			{
				return (classificationDataValidationResult, classificationDataValidationMessage);
			}

			var classificationDatas = segregationEntityQuery.UNDGDataItemDTOs;
			foreach (var classificationData in classificationDatas)
			{
				if(classificationData.UNDGSubstanceDTOs == null || classificationData.UNDGSubstanceDTOs.Length < segregationEntityQuery.Standards.Length)
				{
					return (false, Res.GetString("fa99f301-da03-4e73-af44-182a30d988f5", "Length of substance list cannot be less than length of standards list"));
				}

				var distinctSubstanceDtos = classificationData.UNDGSubstanceDTOs.Select(d => (d.Unno, d.Variant, d.Standard)).Distinct();

				if (distinctSubstanceDtos.Count() != classificationData.UNDGSubstanceDTOs.Length)
				{
					return (false, Res.GetString("C65B7657-1752-426F-A140-DB677E21BBE3", "List of substances cannot contain duplicates"));
				}
			}
			return (true, string.Empty);
		}

		static (bool validationResult, string validationMessage) IsStandardListValid(string[] standards)
		{
			if (standards == null || standards.Length == 0)
			{
				return (false, Res.GetString("47b95281-09a0-4d4a-b5ce-7ff86be328f0", "List of standards cannot be empty"));
			}

			if (standards.Distinct().Count() != standards.Length)
			{
				return (false, Res.GetString("3ed6f979-8af8-4c0c-bad6-2cb6f55fac76", "List of standards cannot contain duplicates"));
			}

			foreach (var standard in standards)
			{
				if (string.IsNullOrWhiteSpace(standard))
				{
					return (false, Res.GetString("aa3c4f69-50bf-45f7-a2b5-35991deade7f", "Standard cannot be empty"));
				}

				if (!IsStandardValid(standard))
				{
					return (false, Res.GetString("868a0263-0d9c-4abd-8842-455d611e576e", "The following standard specified in the query is not valid: {0}", standard));
				}
			}

			return (true, string.Empty);
		}

		static (bool validationResult, string validationMessage) IsEntityListValid<T>(IEnumerable<T> validationList, string validationListType)
		{
			if (validationList == null || !validationList.Any())
			{
				return (false, Res.GetString("ad16f73f-9c4e-43e4-8044-7cd085f72507", "List of {0} cannot be empty", validationListType));
			}

			if (validationList.Count() < 2)
			{
				return (false, Res.GetString("9673e006-1d86-4257-9e14-9badb916ea6c", "List of {0} must contain at least two entries", validationListType));
			}

			return (true, string.Empty);
		}

		static bool IsStandardValid(string standard)
		{
			var type = typeof(UNDGSubstanceStandardTypes);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				var value = field.GetValue(null)?.ToString();
				if (value == standard)
				{
					return true;
				}
			}
			return false;
		}
	}
}
