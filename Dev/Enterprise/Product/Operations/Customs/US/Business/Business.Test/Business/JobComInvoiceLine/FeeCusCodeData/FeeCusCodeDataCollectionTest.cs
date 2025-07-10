using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FeeCusCodeDataCollection))]
	sealed class FeeCusCodeDataCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIFees()
		{
			FeeCusCodeData fee1 = feeCusCodeDataCollection.AddNew();
			fee1.CY_Code = "ABC";
			fee1.CY_FeeAmount = 123m;
			FeeCusCodeData fee2 = feeCusCodeDataCollection.AddNew();
			fee2.CY_Code = "DEF";
			fee2.CY_FeeAmount = 456m;

			IFees fees = feeCusCodeDataCollection;
			AssertEquals("GetFeeFor", fee1, fees.GetFeeFor("ABC"));
			AssertEquals("GetFeeFor", fee2, fees.GetFeeFor("DEF"));

			AssertNotNull(fees.AddNew());
		}

		public void TestUpdateOrAddCharge()
		{
			feeCusCodeDataCollection.UpdateOrAddCharge("AAA", 0m);
			AssertEquals(0m, feeCusCodeDataCollection.GetValue("AAA"));

			feeCusCodeDataCollection.UpdateOrAddCharge("AAA", 10m);
			AssertEquals(10m, feeCusCodeDataCollection.GetValue("AAA"));

			feeCusCodeDataCollection.UpdateOrAddCharge("AAA", 0m);
			AssertEquals(0m, feeCusCodeDataCollection.GetValue("AAA"));
		}

		public void TestGetValue()
		{
			FeeCusCodeData fee1 = feeCusCodeDataCollection.AddNew();
			fee1.CY_Code = "ABC";
			fee1.CY_FeeAmount = 123m;
			FeeCusCodeData fee2 = feeCusCodeDataCollection.AddNew();
			fee2.CY_Code = "DEF";
			fee2.CY_FeeAmount = 456m;

			FeeCusCodeData fee3 = feeCusCodeDataCollection.AddNew();
			fee3.CY_Code = "ABC";
			fee3.CY_FeeAmount = 222m;

			AssertEquals(123m + 222m, feeCusCodeDataCollection.GetValue("ABC"));
			AssertEquals(456m, feeCusCodeDataCollection.GetValue("DEF"));
		}

		public void TestRemoveFee()
		{
			FeeCusCodeData fee1 = invoiceLine.FeeCusCodes.AddNew();
			fee1.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee1.CY_FeeAmount = 12m;

			FeeCusCodeData fee2 = invoiceLine.FeeCusCodes.AddNew();
			fee2.CY_Code = Core.Constants.USCustoms.FeeCodes.Beef;
			fee2.CY_FeeAmount = 4m;

			FeeCusCodeData fee3 = invoiceLine.FeeCusCodes.AddNew();
			fee3.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			fee3.CY_FeeAmount = 2m;

			FeeCusCodeData fee4 = invoiceLine.FeeCusCodes.AddNew();
			fee4.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee4.CY_FeeAmount = 10m;
			Factory.Save();

			AssertEquals(4, invoiceLine.FeeCusCodes.Count);
			Assert(invoiceLine.FeeCusCodes.Contains(fee1));

			invoiceLine.FeeCusCodes.RemoveFee(Core.Constants.USCustoms.FeeCodes.HMF);
			Assert(!invoiceLine.FeeCusCodes.ContainsCode(Core.Constants.USCustoms.FeeCodes.HMF));
			Assert(!invoiceLine.FeeCusCodes.Contains(fee1));
			Assert(!invoiceLine.FeeCusCodes.Contains(fee4));
		}

		public void TestGetCharge()
		{
			var fee1 = invoiceLine.FeeCusCodes.AddNew();
			fee1.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertNull(invoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertNotNull(invoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			var fee2 = invoiceLine.FeeCusCodes.AddNew();
			fee2.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			AssertEquals(2, invoiceLine.FeeCusCodes.Count);
			AssertNotNull(invoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertNotNull(invoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestSetAmount()
		{
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(0m, fee.CY_FeeAmount);

			invoiceLine.FeeCusCodes.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(100m, fee.CY_FeeAmount);

			invoiceLine.FeeCusCodes.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, 200m);
			AssertEquals(2, invoiceLine.FeeCusCodes.Count);

			fee = invoiceLine.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertEquals(200m, fee.CY_FeeAmount);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			return new FeeCusCodeDataCollection(invoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<FeeCusCodeData>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			feeCusCodeDataCollection = new FeeCusCodeDataCollection(invoiceLine);
		}

		FeeCusCodeDataCollection feeCusCodeDataCollection;
		JobComInvoiceLine invoiceLine;

		#endregion

	}
}
