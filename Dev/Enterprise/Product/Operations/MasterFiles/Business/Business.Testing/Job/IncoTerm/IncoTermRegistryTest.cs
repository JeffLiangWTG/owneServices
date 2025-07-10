using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(IncoTermRegistry))]
	sealed class IncoTermRegistryTest : RegistryItemSetTestCaseWithFactory<IncoTermRegistry>
	{
		public void TestAlwaysDebtorCodes()
		{
			AssertEquals(0, ItemSet.ChargeLocalClientAlwaysCodes.GetAsGuidArray().Length);

			Guid new1 = Guid.NewGuid();
			Guid new2 = Guid.NewGuid();
			ItemSet.ChargeLocalClientAlwaysCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new1 + "," + new2);
			Guid[] codes = ItemSet.ChargeLocalClientAlwaysCodes.GetAsGuidArray();
			AssertEquals("New Codes[0]", new1, codes[0]);
			AssertEquals("New Codes[1]", new2, codes[1]);

			AssertEquals("", ItemSet.ChargeLocalClientAlwaysCodes.DefaultChargeCode);

			TestGenericRegistryItem(
				ItemSet.ChargeLocalClientAlwaysCodes,
				"ChargeLocalClientAlwaysCodes",
				RawDataRegistry.Categories.AutoRating_ChargeCodes,
				"Charge Local Client Always Charge Codes",
				"Charge Codes to charge Local Client regardless of Incoterm",
				RegistryStorageFlags.Company);

			AssertEquals(0, ItemSet.ChargeAgentAlwaysCodes.GetAsGuidArray().Length);

			ItemSet.ChargeAgentAlwaysCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new1 + "," + new2);
			codes = ItemSet.ChargeAgentAlwaysCodes.GetAsGuidArray();
			AssertEquals("New Codes[0]", new1, codes[0]);
			AssertEquals("New Codes[1]", new2, codes[1]);

			AssertEquals("", ItemSet.ChargeAgentAlwaysCodes.DefaultChargeCode);

			TestGenericRegistryItem(
				ItemSet.ChargeAgentAlwaysCodes,
				"ChargeAgentAlwaysCodes",
				RawDataRegistry.Categories.AutoRating_ChargeCodes,
				"Charge Agent Always Charge Codes",
				"Charge Codes to charge Agent regardless of Incoterm",
				RegistryStorageFlags.Company);
		}
	}
}
