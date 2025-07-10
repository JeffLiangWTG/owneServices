using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

static class EDIFactMessagingExtensions
{
	public static ZString ToNorwegianAmountString(this ZDecimal source)
	{
		return source == ZDecimal.Zero
			? (NoResString)"0"
			: source.ToStringTrimZeros(3).Replace(".", ",");
	}
}
