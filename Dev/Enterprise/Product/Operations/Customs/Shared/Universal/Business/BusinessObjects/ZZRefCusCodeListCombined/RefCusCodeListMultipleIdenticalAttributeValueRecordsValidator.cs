using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.Customs.Universal
{
	sealed class RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator
	{
		public RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator(ZZRefCusCodeListCombined codeListCombined)
		{
			this.codeListCombined = Argument.NotNull(codeListCombined, nameof(codeListCombined));
		}
		readonly ZZRefCusCodeListCombined codeListCombined;

		public void CheckMultipleIdenticalAttributeValueRecords()
		{
			var multipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage = MultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage;
			var multipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriodRowErrorMessage
				= Res.GetString("9A957B6A-6DBE-4FCF-8187-D5EDBCDD40A0", "Attributes which have the same name and value must not have overlapping validity periods. Please adjust Start Dates and End Dates accordingly.");

			RemoveExistingRelatedRowErrors();

			var multipleIdenticalAttributesValueGroups = codeListCombined.Attributes
				.Cast<ZZRefCusCodeListAttributeCombined>()
				.GroupBy(x => new { x.ZZE_ZXE_NKName, x.ZZE_Value })
				.Where(g => g.Count() > 1);

			foreach (var group in multipleIdenticalAttributesValueGroups)
			{
				if (CheckMultipleIdenticalAttributeValueRecordsHaveStartAndEndDate(group, multipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage))
				{
					CheckMultipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriod(group, multipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriodRowErrorMessage);
				}
			}

			void RemoveExistingRelatedRowErrors()
			{
				var allAttributes = codeListCombined.Attributes;
				if (allAttributes.HasErrors())
				{
					foreach (var attribute in allAttributes)
					{
						if (attribute.HasRowErrors)
						{
							attribute.RemoveRowError(multipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage);
							attribute.RemoveRowError(multipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriodRowErrorMessage);
						}
					}
				}
			}
		}

		internal string MultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage
			=> codeListCombined.Factory.GetCachedValue("F3A2C121-FB74-48C9-AE6B-D1ED8EEACDB8", () => Res.GetString("D52DD8FD-A52A-455C-96AE-80F5A4A80DFE", "For Attributes which have the same name and value it's mandatory to provide a Start Date and End Date."));

		bool CheckMultipleIdenticalAttributeValueRecordsHaveStartAndEndDate(IGrouping<object, ZZRefCusCodeListAttributeCombined> attributeGroup, string errorMessage)
		{
			var hasNoErrors = true;
			attributeGroup.ForEach(x =>
			{
				if (x.ZZE_StartDate.IsEmpty || x.ZZE_EndDate.IsEmpty)
				{
					x.RemoveRowError(x.Validation.BothOrNonDatesErrorMessage);
					x.AddRowError(errorMessage);
					hasNoErrors = false;
				}
			});
			return hasNoErrors;
		}

		void CheckMultipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriod(IGrouping<object, ZZRefCusCodeListAttributeCombined> attributeGroup, string errorMessage)
		{
			if (HasAtLeastOneOverlappingValidityDate())
			{
				attributeGroup.ForEach(x => x.AddRowError(errorMessage));
			}

			bool HasAtLeastOneOverlappingValidityDate()
			{
				var orderedAttributeArray = attributeGroup.OrderBy(x => x.ZZE_StartDate).ToArray();
				for (var i = 0; i < orderedAttributeArray.Length - 1; i++)
				{
					if (orderedAttributeArray[i].ZZE_EndDate >= orderedAttributeArray[i + 1].ZZE_StartDate)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
