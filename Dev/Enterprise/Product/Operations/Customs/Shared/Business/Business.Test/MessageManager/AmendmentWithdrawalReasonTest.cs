using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AmendmentWithdrawalReason))]
	class AmendmentWithdrawalReasonTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultMaxLength()
		{
			AmendmentWithdrawalReason reason = (AmendmentWithdrawalReason)GetNewBusinessObject();
			AssertEquals("Max length for reason should be less than the maximum of the field on the NonPersistantObject", true, reason.ReasonTextInfo.MaxLength >= GetMaxLengthOfReasonText());
		}

		[ExpectNoExceptions]
		public void TestMaxLength()
		{
			AmendmentWithdrawalReason reason = (AmendmentWithdrawalReason)GetNewBusinessObject();
			reason.ReasonText = new ZString(new string('*', reason.ReasonTextInfo.MaxLength + 10));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AmendmentWithdrawalReason();
		}

		protected virtual int GetMaxLengthOfReasonText()
		{
			return 512;
		}
		#endregion
	}
}
