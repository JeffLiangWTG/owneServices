using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	internal class CusStatementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB2_PaymentType()
		{
			CombineAssertions("B2_PaymentType List Validation Test", () =>
			{
				statement.B2_PaymentType = ZString.Empty;
				AssertHasMessageErrorContaining(statement.B2_PaymentTypeInfo, MandatoryValidation.YouHaveNotEntered);
				statement.B2_PaymentType = "X";
				AssertHasMessageErrorContaining(statement.B2_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
				statement.B2_PaymentType = PaymentTypesList.Codes._1;
				AssertNoMessageErrorContaining(statement.B2_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckB2_PrintDate()
		{
			statement.B2_PrintDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining("B2_PrintDate", statement.B2_PrintDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB2_Status()
		{
			CombineAssertions("B2_PaymentType List Validation Test", () =>
			{
				statement.B2_Status = ZString.Empty;
				AssertHasMessageErrorContaining(statement.B2_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				statement.B2_Status = "X";
				AssertHasMessageErrorContaining(statement.B2_StatusInfo, ListValidation.InvalidCodeMessageError);
				statement.B2_Status = StatementHeaderStatusList.Codes.PRE;
				AssertNoMessageErrorContaining(statement.B2_StatusInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckB2_PaymentStatus()
		{
			CombineAssertions("B2_PaymentStatus List Validation Test", () =>
			{
				statement.B2_PaymentStatus = ZString.Empty;
				AssertHasMessageErrorContaining(statement.B2_PaymentStatusInfo, MandatoryValidation.YouHaveNotEntered);
				statement.B2_PaymentStatus = "X";
				AssertHasMessageErrorContaining(statement.B2_PaymentStatusInfo, ListValidation.InvalidCodeMessageError);
				statement.B2_PaymentStatus = PaymentStatusList.Codes.PAD;
				AssertNoMessageErrorContaining(statement.B2_PaymentStatusInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckB2_PaymentParty()
		{
			CombineAssertions("B2_PaymentParty List Validation Test", () =>
			{
				statement.B2_PaymentParty = ZString.Empty;
				AssertHasMessageErrorContaining(statement.B2_PaymentPartyInfo, MandatoryValidation.YouHaveNotEntered);
				statement.B2_PaymentParty = "X";
				AssertHasMessageErrorContaining(statement.B2_PaymentPartyInfo, ListValidation.InvalidCodeMessageError);
				statement.B2_PaymentParty = PaymentPartyList.Codes.BRK;
				AssertNoMessageErrorContaining(statement.B2_PaymentPartyInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
		}

		CusStatementHeader statement;
	}
}
