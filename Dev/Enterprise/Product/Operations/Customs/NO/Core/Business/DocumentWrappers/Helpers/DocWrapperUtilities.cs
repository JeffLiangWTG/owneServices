using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

static class DocWrapperUtilities
{
	public static string ToStringRounded(this ZDecimal value, int numberOfDecimalPlaces) => value.Round(numberOfDecimalPlaces).ToString(numberOfDecimalPlaces, true);
}
