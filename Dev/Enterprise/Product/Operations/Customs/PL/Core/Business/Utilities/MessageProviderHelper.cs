using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public static class MessageProviderHelper
{
	public static T? ReturnNullIfEmpty<T>(T input) where T : struct, IZType => input.IsEmpty ? null : input;

	public static DateTime? ReturnNullIfInvalid(ZDateTime input) => input.IsValid ? input.ToDateTime() : null;

	public static int? IntReturnNullIfEmpty(ZString input) => !input.IsEmpty && int.TryParse(input, out int result) ? result : null;

	public static string GetLabelingOfTransportAtArrival(JobDeclaration declaration)
	{
		var result = declaration?.JE_TransportIDInland ?? ZString.Empty;

		if (declaration?.IsRoadInland ?? false)
		{
			result = AddToLabelIfNotEmpty(result, declaration.JE_Trailer1RegNo);
			result = AddToLabelIfNotEmpty(result, declaration.JE_Trailer2RegNo);
		}

		return ReturnNullIfEmpty(result);
	}

	static ZString AddToLabelIfNotEmpty(ZString label, ZString regNoToAdd) => regNoToAdd.IsEmpty
		? label.ToString()
		: label.IsEmpty
			? regNoToAdd.ToString()
			: $"{label}/{regNoToAdd}";
}
