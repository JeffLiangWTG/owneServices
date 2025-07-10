using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CreditCardNumberBusinessObject))]
	internal sealed class CreditCardNumberBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			return new CreditCardNumberBusinessObject(bankAccount);
		}

		#endregion

		#region Tests

		public void TestEncrypt()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			CreditCardNumberBusinessObject bo = new CreditCardNumberBusinessObject(bankAccount);
			bo.UnencryptedCreditCardNumber = "4321123456788765";
			bo.Encrypt();

			TwoWayEncoder encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
			string encryptedNumber = encoder.Encrypt(bo.UnencryptedCreditCardNumber);
			AssertEquals("Encrypted Credit Card Number", encryptedNumber, bankAccount.AB_DebitCreditCardNumber);
			AssertEquals("Account Num", "**** **** ***8 8765", bankAccount.AB_AccountNum);
		}

		#endregion
	}
}
