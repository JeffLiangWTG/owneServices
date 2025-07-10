using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	[TestedType(typeof(DGNPackingLine))]
	sealed class DGNPackingLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DGNPackingLine(new ZGuid())
			{
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>(),
				DangerousGoods = System.Array.Empty<DGNDangerousGood>()
			};
		}
	}
}
