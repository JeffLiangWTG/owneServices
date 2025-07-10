using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusBrokerStaffValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			currentElement.Validation.ValidateAll();
			AssertHasErrorContaining(currentElement.BrokerStaffCodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(currentElement.MailboxInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateBrokerStaffCode()
		{
			var targetInfo = currentElement.BrokerStaffCodeInfo;
			currentElement.Validation.ValidateBrokerStaffCode();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.BrokerStaffCode = "CYO";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.BrokerStaffCode = "?";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateMailbox()
		{
			var targetInfo = currentElement.MailboxInfo;
			currentElement.BrokerStaffCode = "CYO";
			currentElement.Validation.ValidateMailbox();
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.Mailbox = "XX";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.Mailbox = "TBK0461-0";
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateBrokerStaff();
			currentElement = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		CusBrokerStaff currentElement;
	}
}
