using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var bill = (AsycudaBill)GetCollectionToTest().AddNew();
			AssertEquals(Core.Constants.Weight.Kilograms, bill.ABL_GrossWeightUQ);
			AssertEquals(Core.Constants.CurrencyCodes.Taiwan, bill.ABL_RX_NKCustomsValueCurrency);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills;
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillCollection);
	}
}
