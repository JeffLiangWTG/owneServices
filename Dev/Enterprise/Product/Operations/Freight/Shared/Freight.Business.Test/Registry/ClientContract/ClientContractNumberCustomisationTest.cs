using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ClientContractNumberCustomisation))]
	sealed class ClientContractNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<ClientContractNumberCustomisation>
	{
		public void TestGlobalOrLocal()
		{
			var customisation = GetBusinessObjectToSerialise();
			var globalOrLocal = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.GlobalOrLocal];

			AssertEquals("Should have an order of 2", 2, globalOrLocal.Order.ToZInt());
			AssertEquals("Should be included", true, globalOrLocal.Include);

			CombineAssertions("Should be read-only", () =>
			{
				Assert(globalOrLocal.OrderInfo.ReadOnly);
				Assert(globalOrLocal.IncludeInfo.ReadOnly);
				Assert(globalOrLocal.FountainInfo.ReadOnly);
				Assert(globalOrLocal.CheckDigitInfo.ReadOnly);
			});
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ClientContractNumberCustomisation GetBusinessObjectToClone() => new ClientContractNumberCustomisation();

		protected override ClientContractNumberCustomisation GetBusinessObjectToSerialise() => new ClientContractNumberCustomisation();
	}
}
