using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Accounting.AccBankAccount.Testing
{
	[TestedType(typeof(CreditCardEntryForm))]
	sealed class CreditCardEntryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			Business.AccBankAccount bankAccount = Factory.NewWithValidTestData<Business.AccBankAccount>();
			bankAccount.AB_AccountType = ZArchitecture.Core.ReceiptTypes.eNettCreditCard;
			CreditCardNumberBusinessObject creditCardNumberBusinessObject = new CreditCardNumberBusinessObject(bankAccount);
			return new CreditCardEntryForm(creditCardNumberBusinessObject);
		}

		#endregion
	}
}
