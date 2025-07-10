using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.Business.Testing;

public static class CodeDescriptionPairListExtensions
{
	public static void AssertContainsExactCodes(this ICodeDescriptionPairList codesList, string description, IEnumerable<string> expectedCodes)
		=> AssertContainsExactElementsInAnyOrder(description, expectedCodes, codesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
}
