using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;
using RefCusCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class FunctionalErrorInterpreter(BusinessObjectFactory factory)
{
	public ZString[] GetColumnTitles() => [
		FunctionalError.Code,
		FunctionalError.Reason,
		FunctionalError.ErrorPointer,
		FunctionalError.ErrorReferenceField,
	];

	public IEnumerable<ParamValues> GetRows(IEnumerable<IFunctionalError> functionalErrors)
	{
		if (functionalErrors.IsNullOrEmpty())
		{
			yield break;
		}

		var index = 0;
		foreach (var error in functionalErrors)
		{
			yield return new ParamValues(
				index.ToString(),
				[
					error.ErrorCode.ToString(),
					GetFunctionalErrorReason(error),
					error.ErrorPointer,
					error.OriginalAttributeValue,
				]);
			index++;
		}
	}

	ZString GetFunctionalErrorReason(IFunctionalError error)
	{
		var description = factory.FindCusCodeDescription(RefCusCodes.Code_CL180, error.ErrorCode.ToString())?.Description ?? string.Empty;
		return string.IsNullOrEmpty(description) ? error.ErrorReason : error.ErrorReason + "\n" + description;
	}
}
