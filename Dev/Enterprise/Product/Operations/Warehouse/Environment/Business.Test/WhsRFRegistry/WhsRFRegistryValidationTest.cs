using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsRFRegistryValidationTest : WhsBusinessObjectValidationTestCase
	{
		public void TestValidateWLV_WLT_LocationType_ValidCodes()
		{
			var registry = Factory.New<WhsRFRegistry>();

			AssertEquals(WhsRFRegistry.DefaultUOMPackType, registry.WRR_UOMPackType);
			AssertNoErrors(registry.WRR_UOMPackTypeInfo);

			var uomPackTypes = new UOMPackTypesList();
			uomPackTypes.AddPair(WhsRFRegistry.DefaultUOMPackType);
			foreach (CodeDescriptionPair type in uomPackTypes)
			{
				registry.WRR_UOMPackType = type.Code;
				AssertNoErrors(registry.WRR_UOMPackTypeInfo);
			}
		}

		public void TestValidateWLV_WLT_LocationType_InvalidCode()
		{
			var registry = Factory.New<WhsRFRegistry>();

			AssertEquals(WhsRFRegistry.DefaultUOMPackType, registry.WRR_UOMPackType);
			AssertNoErrors(registry.WRR_UOMPackTypeInfo);

			registry.WRR_UOMPackType = "123";
			AssertHasErrors(registry.WRR_UOMPackTypeInfo);
		}
	}
}
