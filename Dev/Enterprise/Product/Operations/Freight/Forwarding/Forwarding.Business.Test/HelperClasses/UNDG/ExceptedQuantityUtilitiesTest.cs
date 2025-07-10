using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ExceptedQuantityUtilitiesTest : TestCaseWithFactory
	{
		public void TestExceptedQuantitiesCodes_AreKeysIn_MaximumInnerQuantityDictionary()
		{
			var exceptedQuantityCodes = new string[]
			{
				UNDGSubstanceLookups.ExceptedQuantity.Code.E1,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E2,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E3,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E4,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E5
			};

			foreach (var code in exceptedQuantityCodes)
			{
				Assert($"Excepted Quantity Code {code} must be listed in MaximumInnerQuantityDictionary",
					MaximumInnerQuantityDictionary.TryGetValue(code, out _));
			}
		}

		public void TestExceptedQuantitiesCodes_AreKeysIn_MaximumOuterQuantityDictionary()
		{
			var exceptedQuantityCodes = new string[]
			{
				UNDGSubstanceLookups.ExceptedQuantity.Code.E1,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E2,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E3,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E4,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E5
			};

			foreach (var code in exceptedQuantityCodes)
			{
				Assert($"Excepted Quantity Code {code} must be listed in MaximumOuterQuantityDictionary",
					MaximumOuterQuantityDictionary.TryGetValue(code, out _));
			}
		}
	}
}
