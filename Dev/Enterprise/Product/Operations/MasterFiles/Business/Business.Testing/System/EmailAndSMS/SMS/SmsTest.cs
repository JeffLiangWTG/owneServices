using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(Sms))]
	sealed class SmsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			Sms sms1 = new Sms();
			AssertEquals(0, sms1.PhoneNumbers.Length);
			AssertEquals("", sms1.Message);

			Sms sms2 = new Sms("123", "message");
			AssertEquals("123", sms2.PhoneNumbers[0]);
			AssertEquals("message", sms2.Message);
		}

		#endregion

		#region TestPhoneNumber

		public void TestPhoneNumbers()
		{
			SMS.Validation.ValidatePhoneNumberCount();
			AssertHasErrors(SMS.PhoneNumberCountInfo);

			SMS.AddRecipient("+61 9024 1100");
			AssertEquals("+61 9024 1100", SMS.PhoneNumbers[0]);

			SMS.AddRecipient("+61 9024 1100");
			AssertEquals("Duplicate phone number should not be added.", 1, SMS.PhoneNumbers.Length);

			SMS.AddRecipient("   ");
			AssertEquals("Empty phone number should not be added.", 1, SMS.PhoneNumbers.Length);

			SMS.Validation.ValidatePhoneNumberCount();
			AssertNoErrors(SMS.PhoneNumberCountInfo);
		}

		#endregion

		#region TestMessage

		public void TestMessage()
		{
			SMS.Message = "hello";
			AssertEquals("hello", SMS.Message);
			AssertNoErrors(SMS.MessageInfo);

			SMS.Message = "";
			AssertHasErrors(SMS.MessageInfo);

			SMS.Message = ZString.Replicate('0', 160);
			AssertNoErrors(SMS.MessageInfo);

			SMS.Message = ZString.Replicate('0', 161);
			AssertHasErrors(SMS.MessageInfo);
		}

		#endregion

		#region Implementation

		Sms SMS
		{
			get
			{
				if (fSMS == null)
				{
					fSMS = new Sms();
				}
				return fSMS;
			}
		}

		Sms fSMS;

		#endregion
	}
}
