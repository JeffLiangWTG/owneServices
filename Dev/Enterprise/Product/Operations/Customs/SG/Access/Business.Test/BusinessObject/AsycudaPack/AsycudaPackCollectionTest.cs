using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection))]
	sealed class AsycudaPackCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValues()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var pack = bill.Packs.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, pack.LinePriceCurrency);
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs;
		}
	}
}
