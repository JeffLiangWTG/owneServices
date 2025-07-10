using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestLinesCollection))]
	sealed class ExportCustomsManifestLinesBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			return header.Lines;
		}
	}
}
