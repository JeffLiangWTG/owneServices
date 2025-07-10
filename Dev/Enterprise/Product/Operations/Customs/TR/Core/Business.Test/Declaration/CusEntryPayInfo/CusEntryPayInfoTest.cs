using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryPayInfo))]
	class CusEntryPayInfoTest : Customs.Business.Testing.CusEntryPayInfoTest
	{
		public void TestOnSaving()
		{
			var anotherFactory = new BusinessObjectFactory();
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var payInfo = entry.EntryPayInfos.AddNew();
			Factory.Save();

			CombineAssertions("when C9_PaymentAmount is 0", () =>
			{
				AssertEquals(true, payInfo.IsDeleted);
				AssertNull(anotherFactory.Load<CusEntryPayInfo>(payInfo.PK));
			});

			dec = Factory.New<JobDeclaration>();
			entry = dec.CustomsEntryHeaders.AddNew();
			payInfo = entry.EntryPayInfos.AddNew();
			payInfo.C9_PaymentAmount = 12.34m;
			Factory.Save();

			CombineAssertions("when C9_PaymentAmount is not 0", () =>
			{
				var payInfo2 = anotherFactory.Load<CusEntryPayInfo>(payInfo.PK);
				AssertNotNull(payInfo2);
				AssertEquals(12.34m, payInfo2.C9_PaymentAmount);
			});
		}

		public void TestCaptionOfTRCusEntryPayInfoProperties()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();
			CombineAssertions("TR CusEntryPayInfo Properties Caption", () =>
			{
				AssertCaption(cusEntryPayInfo.C9_PaymentAmountInfo, string.Empty, "Current Loan");
				AssertCaption(cusEntryPayInfo.C9_PaymentReferenceInfo, "Union Rec.Num.", "Union Record Number");
				AssertCaption(cusEntryPayInfo.C9_IncomingPayResponseNoInfo, "Union Appr.Code", "Union Approval Code");
				AssertCaption(cusEntryPayInfo.C9_BankAccountInfo, string.Empty, "TPS Reference");
			});
		}

		public void TestC9_PaymentAmountReadOnly()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_PaymentAmount_ReadOnly", false, cusEntryPayInfo.C9_PaymentAmount_ReadOnly);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_PaymentAmount_ReadOnly", true, cusEntryPayInfo.C9_PaymentAmount_ReadOnly);
		}

		public void TestC9_PaymentAmountDecimalPlace()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_PaymentAmountDecimalPlace", 4, cusEntryPayInfo.C9_PaymentAmountDecimalPlace);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_PaymentAmountDecimalPlace", 2, cusEntryPayInfo.C9_PaymentAmountDecimalPlace);
		}

		public void TestC9_PaymentReferenceReadOnly()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_PaymentReference_ReadOnly", false, cusEntryPayInfo.C9_PaymentReference_ReadOnly);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_PaymentReference_ReadOnly", true, cusEntryPayInfo.C9_PaymentReference_ReadOnly);
		}

		public void TestC9_PaymentReferenceMaxLength()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_IncomingPayResponseNoMaxLength", 20, cusEntryPayInfo.C9_PaymentReferenceMaxLength);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_IncomingPayResponseNoMaxLength", 16, cusEntryPayInfo.C9_PaymentReferenceMaxLength);
		}

		public void TestC9_IncomingPayResponseNoReadOnly()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_IncomingPayResponseNo_ReadOnly", false, cusEntryPayInfo.C9_IncomingPayResponseNo_ReadOnly);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_IncomingPayResponseNo_ReadOnly", true, cusEntryPayInfo.C9_IncomingPayResponseNo_ReadOnly);
		}

		public void TestC9_IncomingPayResponseNoMaxLength()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_IncomingPayResponseNoMaxLength", 35, cusEntryPayInfo.C9_IncomingPayResponseNoMaxLength);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_IncomingPayResponseNoMaxLength", 19, cusEntryPayInfo.C9_IncomingPayResponseNoMaxLength);
		}

		public void TestC9_BankAccountReadOnly()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_BankAccount_ReadOnly", false, cusEntryPayInfo.C9_BankAccount_ReadOnly);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_BankAccount_ReadOnly", true, cusEntryPayInfo.C9_BankAccount_ReadOnly);
		}

		public void TestC9_BankAccountNoMaxLength()
		{
			var cusEntryPayInfo = Factory.New<CusEntryPayInfo>();

			AssertEquals("C9_BankAccountMaxLength", 20, cusEntryPayInfo.C9_BankAccountMaxLength);

			cusEntryPayInfo.C9_PaymentParty = "EXU";

			AssertEquals("C9_BankAccountMaxLength", 20, cusEntryPayInfo.C9_BankAccountMaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var payInfo = (CusEntryPayInfo)GetNewBusinessObject();
			payInfo.C9_PaymentAmount = 10m;
			return payInfo;
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}
	}
}
