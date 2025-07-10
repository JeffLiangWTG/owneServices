using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business;

public static class Extensions
{
	public static int ToInt(this char value) => Convert.ToInt32(char.GetNumericValue(value));

	public static int? ToIntOrNull(this ZString value) => int.TryParse(value, out var result) ? result : null;

	public static bool IsAESTransitionPeriod(this CusEntryInstruction entryInstruction) => entryInstruction != null && FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, entryInstruction.CEI_DateForDuty, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);

	public static void ValidCodeCheckAndAddErrorIfEmpty(this ZPropertyInfo propertyInfo)
	{
		MandatoryValidation.CheckEntered(propertyInfo);
		ListValidation.MessageErrorIfInvalidCode(propertyInfo);
	}

	public static IEnumerable<T> DistinctSpecificDocuments<T>(this IEnumerable<T> source, ZString[] documentCodesAsUnique)
		where T : CusSupportingInfo
	{
		if (source is null
			|| !source.Any()
			|| documentCodesAsUnique is null
			|| documentCodesAsUnique.Length == 0)
		{
			return source;
		}
		return DistinctInternal();

		IEnumerable<T> DistinctInternal()
		{
			var uniqueCodes = new HashSet<ZString>(documentCodesAsUnique);
			var returnedCodes = new HashSet<ZString>(documentCodesAsUnique.Length);
			foreach (var document in source)
			{
				var code = document.CSI_Code;
				if (!uniqueCodes.Contains(code))
				{
					yield return document;
				}
				else
				{
					if (returnedCodes.Add(code))
					{
						yield return document;
					}
				}
			}
		}
	}
}
