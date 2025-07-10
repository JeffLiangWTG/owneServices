using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	public static class ListExtensions
	{
		public static string ToCodeStrings(this ICodeDescriptionPairList list)
		{
			return new ZStringBuilder(list.OfType<ICodeDescription>().Select(x => x.Code)).ToStringWithDelimiterBetweenAppends(", ");
		}
	}
}
