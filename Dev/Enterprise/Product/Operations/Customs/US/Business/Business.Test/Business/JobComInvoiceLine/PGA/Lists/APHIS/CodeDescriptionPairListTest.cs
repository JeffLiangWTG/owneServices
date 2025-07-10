using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class CodeDescriptionPairListTest : TestCaseWithFactory
	{
		protected void AssertList<T>(ICodeDescriptionPairList list, string[] expectedCodes, CodeDescriptionPairList listToRemoveFrom = null)
			where T : CodeDescriptionPairList, new()
		{
			var fullList = Factory.GetCachedValue<T>();
			AssertEquals(expectedCodes.Length, list.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list.GetDescriptionFromCode(code));
				if (listToRemoveFrom != null)
				{
					listToRemoveFrom.RemoveCode(code);
				}
			}
		}
	}
}
