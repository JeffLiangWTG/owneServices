using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	sealed class GenSchemaTest
	{
		[Test]
		public void CommonXSDFiles()
		{

			var resources = AllXSDResources.Where(x => x.StartsWith("CargoWise.RefDbRepo.ILReferenceData.Tests.Schemas.Schemas", System.StringComparison.InvariantCultureIgnoreCase));
			var expectedXSDFiles = new[]
			{
				"CargoWise.RefDbRepo.ILReferenceData.Tests.Schemas.Schemas.CustomsCodesRequest.SYSTBL_NG_9000_MSG_SystemTablesRequest.xsd",
				"CargoWise.RefDbRepo.ILReferenceData.Tests.Schemas.Schemas.CustomsCodesResponse.SYSTBL_NG_9001_MSG_SystemTablesResponse.xsd",
				"CargoWise.RefDbRepo.ILReferenceData.Tests.Schemas.Schemas.EAICommon.xsd",
				"CargoWise.RefDbRepo.ILReferenceData.Tests.Schemas.Schemas.ExchangeRatesRequest.CD_NG_8347_Web01_CurrencyRateSearchParam.xsd"
			};
			Assert.That(resources, Is.EquivalentTo(expectedXSDFiles));
		}

		IEnumerable<string> AllXSDResources
		{
			get
			{
				if (allXSDResources == null)
				{
					allXSDResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
				}
				return allXSDResources;
			}
		}
		IEnumerable<string> allXSDResources;
	}
}
