using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class NonGAReasonAdditionalDataUpdater : IAdditionalDataUpdater<RefCusCodeList>
	{
		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration) => true;

		public static void UpdateAdditionally(RefCusCodeList nonGAReasonType)
		{
			if (nonGAReasonType != null && !string.IsNullOrEmpty(nonGAReasonType.ZZD_Code))
			{
				var (code, description) = SplitCodeAndDescription(nonGAReasonType.ZZD_Code);
				nonGAReasonType.ZZD_Code = code;
				nonGAReasonType.ZZD_Description = description;
			}
		}

		static (string code, string description) SplitCodeAndDescription(string originalText)
		{
			var code = string.Empty;
			var description = string.Empty;
			if (originalText != null && originalText.Length > 7)
			{
				originalText = originalText.TrimStart();
				code = originalText.Substring(0, 5);
				description = originalText.ElementAt(5).Equals('(') ? originalText.Substring(6, originalText.Length - 7) : originalText.Substring(5, originalText.Length - 5);
				description = description.Trim();
			}

			return (code, description);
		}

		bool IAdditionalDataUpdater<RefCusCodeList>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateAdditionally(RefCusCodeList nonGAReasonType, IRow row, EntityConfiguration configuration) => UpdateAdditionally(nonGAReasonType);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateRule(RefCusCodeList nonGAReasonType, Rule rule) { }
		void IAdditionalDataUpdater<RefCusCodeList>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
