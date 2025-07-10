using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.MasterFiles.Business
{
	public class CreditCardNumberBusinessObject : AutoCreditCardNumberBusinessObject
	{
		public CreditCardNumberBusinessObject(AccBankAccount bankAccount)
		{
			BankAccount = bankAccount;
			HasChanges = false;
		}

		AccBankAccount BankAccount { get; set; }

		public void Encrypt()
		{
			TwoWayEncoder encoder = new TwoWayEncoder(BankAccount.PK.ToGuid());
			EncryptedCreditCardNumber = encoder.Encrypt(UnencryptedCreditCardNumber);
			BankAccount.AB_DebitCreditCardNumber = EncryptedCreditCardNumber;
			string accountNum = string.Empty;
			for (int i = 0; i < UnencryptedCreditCardNumber.Length; i++)
			{
				if (i >= UnencryptedCreditCardNumber.Length - 5)
				{
					accountNum += UnencryptedCreditCardNumber[i];
				}
				else
				{
					accountNum += "*";
				}
				if (((i + 1) % 4) == 0)
				{
					accountNum += " ";
				}
			}
			if (BankAccount.IsCreditCardAccount ||
				(BankAccount.IsLinkedAccount && BankAccount.AB_AccountNum.IsEmpty))
			{
				BankAccount.AB_AccountNum = accountNum;
			}
		}
	}
}
