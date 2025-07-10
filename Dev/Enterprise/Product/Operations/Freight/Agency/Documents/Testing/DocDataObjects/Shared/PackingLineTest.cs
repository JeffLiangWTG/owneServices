using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(PackingLine))]
	class PackingLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PackingLine(new ZGuid())
			{
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>(),
				DangerousGoods = System.Array.Empty<DangerousGood>()
			};
		}
	}
}
