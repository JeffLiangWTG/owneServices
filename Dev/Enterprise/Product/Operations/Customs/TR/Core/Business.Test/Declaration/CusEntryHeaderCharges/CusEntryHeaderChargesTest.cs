using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	public class CusEntryHeaderChargesTest : EU.Business.Declaration.Testing.CusEntryHeaderChargesTest
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		public void TestCaptionOfTRCusEntryHeaderChargesProperties()
		{
			var cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();
			CombineAssertions("TR CusEntryHeaderCharges Properties Caption", () =>
			{
				AssertCaption(cusEntryHeaderCharges.C1_ChargeAmountInfo, string.Empty, "Total Payment");
				AssertCaption(cusEntryHeaderCharges.C1_MethodOfPaymentInfo, string.Empty, "Payment Type");
			});
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}

		public void TestC1_ChargeAmountReadOnly()
		{
			var cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();

			AssertEquals("C1_ChargeAmountInfo.ReadOnly", false, cusEntryHeaderCharges.C1_ChargeAmount_ReadOnly);

			cusEntryHeaderCharges.C1_ChargeType = "EXU";

			AssertEquals("C1_ChargeAmountInfo.ReadOnly", true, cusEntryHeaderCharges.C1_ChargeAmount_ReadOnly);
		}

		public void TestC1_ChargeAmountDecimalPlaces()
		{
			var cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();

			AssertEquals("C1_ChargeAmountDecimalPlaces", 4, cusEntryHeaderCharges.C1_ChargeAmountDecimalPlaces);

			cusEntryHeaderCharges.C1_ChargeType = "EXU";

			AssertEquals("C1_ChargeAmountDecimalPlaces", 2, cusEntryHeaderCharges.C1_ChargeAmountDecimalPlaces);
		}

		public void TestC1_MethodOfPaymentReadOnly()
		{
			var cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();

			AssertEquals("C1_MethodOfPayment_ReadOnly", false, cusEntryHeaderCharges.C1_MethodOfPayment_ReadOnly);

			cusEntryHeaderCharges.C1_ChargeType = "EXU";

			AssertEquals("C1_MethodOfPayment_ReadOnly", true, cusEntryHeaderCharges.C1_MethodOfPayment_ReadOnly);
		}
	}
}
