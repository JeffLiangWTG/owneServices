using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business.Tests
{
	class GeographyTypeTest : TestCaseWithFactory
	{
		public void TestCodePairList()
		{
			var registryValue = new CodeDescriptionPairList();

			using (SystemDataRegistry.Instance.UserDefinedGeographyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertCodePairListResult(GeographyType.CodePairList, new Dictionary<string, string> { { "UKN", "Nations of UK" } });
			}

			registryValue.AddPair("UABC", "Description For UABC");
			registryValue.AddPair("UDEF", "Description For UDEF");

			using (SystemDataRegistry.Instance.UserDefinedGeographyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertCodePairListResult(GeographyType.CodePairList, new Dictionary<string, string>
				{
					{ "UKN", "Nations of UK" },
					{ "UABC", "Description For UABC" },
					{ "UDEF", "Description For UDEF" }
				});
			}
		}

		void AssertCodePairListResult(CodeDescriptionPairList codePairList, Dictionary<string, string> dictionary)
		{
			AssertEquals(codePairList.Count, dictionary.Count);

			foreach (ICodeDescription codePair in codePairList)
			{
				AssertEquals(codePair.Description, dictionary[codePair.Code]);
			}
		}
	}
}
