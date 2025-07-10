using System;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(SGAccessRegistry))]
	sealed class SGAccessRegistryTest : RegistryItemSetTestCaseWithFactory<SGAccessRegistry>
	{
		public void TestOVRLiveEffectiveDate()
		{
			TestGenericRegistryItem(
				ItemSet.OVRLiveEffectiveDate,
				"EffectiveOVRLiveDate",
				SG.Registry.SGCustomsDataRegistry.Categories.Customs_Singapore_ACCESS,
				"Effective OVR Live Date",
				"The date that the SG ACCESS requires the OVR SR data in the AIRPCM report.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				new DateTime(2023, 1, 1)
			);
		}
	}
}
