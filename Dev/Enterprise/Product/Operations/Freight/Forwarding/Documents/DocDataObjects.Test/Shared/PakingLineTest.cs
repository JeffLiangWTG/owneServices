using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(PackingLine))]
	sealed class PakingLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PackingLine(ZGuid.NewZGuid(), Factory)
			{
				DangerousGoods = System.Array.Empty<DangerousGood>(),
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>()
			};
		}

		public void TestEuropeanUnionECICSCusCodeListNotNull()
		{
			AssertNotNull(new PackingLine(ZGuid.NewZGuid(), Factory)
			{
				DangerousGoods = System.Array.Empty<DangerousGood>(),
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>()
			}.EuropeanUnionECICSCusCodeList);

			AssertNotNull(new PackingLine(ZGuid.NewZGuid(), null)
			{
				DangerousGoods = System.Array.Empty<DangerousGood>(),
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>()
			}.EuropeanUnionECICSCusCodeList);
		}
	}
}
