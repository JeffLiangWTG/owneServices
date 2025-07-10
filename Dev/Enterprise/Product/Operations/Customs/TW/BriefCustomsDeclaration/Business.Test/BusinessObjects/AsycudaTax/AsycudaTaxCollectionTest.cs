using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaTaxCollection))]
	sealed class AsycudaTaxCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew().AsycudaTaxes;
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaTaxCollection);

		public void TestDefaultValue()
		{
			var tax = ((AsycudaTaxCollection)Collection).AddNew();
			AssertEquals("AsycudaTax.AET_MethodOfPayment's default value should be DutyLevied", tax.AET_MethodOfPayment, MethodOfPaymentList.Codes.DutyLevied);
		}

		public void TestMaxAllowed()
		{
			var testCollection = (AsycudaTaxCollection)Collection;
			CombineAssertions(() =>
			{
				Assert("Collection is empty", testCollection.AllowNew);

				for (var i = 0; i < 8; i++)
				{
					testCollection.AddNew();
					Assert($"Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
				}

				testCollection.AddNew();
				Assert($"Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);
			});
		}
	}
}
