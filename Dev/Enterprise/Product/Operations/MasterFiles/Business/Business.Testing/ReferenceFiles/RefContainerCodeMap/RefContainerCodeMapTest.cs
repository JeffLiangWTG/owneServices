using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainerCodeMap))]
	sealed class RefContainerCodeMapTest : EnterpriseBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RefContainerCodeMap>();
		}

		#endregion

		public void TestEquipmentTypes()
		{
			var codeMap = Factory.New<RefContainerCodeMap>();
			codeMap.RCM_RN_NKCountry = "US";
			var lookups = codeMap.Lookups;
			AssertEquals(true, lookups.CodeList.ContainsCode("40"));
			AssertEquals(true, lookups.CodeList.ContainsCode("AR"));
		}
	}
}
