using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaTaxCollection))]
	public class AsycudaTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaTaxCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AsycudaTaxes;
		}

		public void TestIsExistOverrideTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var tax1 = bill.AsycudaTaxes.AddNew();
			tax1.AET_ChargeType = TaxCodeList.Codes.CustomsDuty;
			tax1.AET_RateOverrideReasonCode = ManifestBase.RateOverrideReasonCodeList.Codes.Additional;

			var tax2 = bill.AsycudaTaxes.AddNew();
			tax2.AET_ChargeType = TaxCodeList.Codes.TRTBandrol;
			tax2.AET_RateOverrideReasonCode = ManifestBase.RateOverrideReasonCodeList.Codes.Override;

			Assert(!bill.AsycudaTaxes.IsExistOverrideTax(TaxCodeList.Codes.CustomsDuty));
			Assert(bill.AsycudaTaxes.IsExistOverrideTax(TaxCodeList.Codes.TRTBandrol));
		}
	}
}
