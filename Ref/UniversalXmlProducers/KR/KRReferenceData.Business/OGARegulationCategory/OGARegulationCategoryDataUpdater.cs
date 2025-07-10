using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public sealed class OGARegulationCategoryDataUpdater : IAdditionalDataUpdater<RefCusCodeList>
	{
		const string RequirementDocumentType = "Requirement Document Type";

		bool IAdditionalDataUpdater<RefCusCodeList>.IsDataRowValid(IRow row, EntityConfiguration configuration) => true;

		void IAdditionalDataUpdater<RefCusCodeList>.UpdateAdditionally(RefCusCodeList cusCodeList, IRow row, EntityConfiguration configuration) { }

		void IAdditionalDataUpdater<RefCusCodeList>.UpdateRule(RefCusCodeList cusCodeList, Rule rule)
		{
			if (rule.Name == RequirementDocumentType)
			{
				if (rule.Relationship == Relationship.OR.ToString())
				{
					var matchRule = rule.RuleValues.FirstOrDefault(x => x.Value == cusCodeList.ZZD_Code);
					if (matchRule != null)
					{
						cusCodeList.RefCusCodeListAttributes = new[] { new RefCusCodeListAttribute { ZZE_Value = matchRule.DocumentType } };
					}
				}
			}
		}

		void IAdditionalDataUpdater<RefCusCodeList>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
