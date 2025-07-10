using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccountDetailsDependentCollection : DependentBusinessObjectCollection<AccAPAccountDetails, OrgCompanyData>
	{
		public AccountDetailsDependentCollection(OrgCompanyData parent, BusinessObjectFactory factory)
			: base(parent, new ZQuery(AccAPAccountDetailsSchema.A1_PaymentMethod, SQLComparisonOperator.NotEqual, new string[] { AccARAccountDetails.ARBankAccPayment, AccARAccountDetails.ARCollectionRequest, AccARAccountDetails.ARNettingBankAccount }))
		{ }

		public AccAPAccountDetails GetAccountDetails(ZString paymentMethod, ZString currencyCode)
			=> GetAccountDetails(paymentMethod, currencyCode, ZBool.True)
				?? GetAccountDetails(paymentMethod, currencyCode, ZBool.False);

		public AccAPAccountDetails GetAccountDetails(ZString paymentMethod, ZString currencyCode, ZBool isDefault)
		{
			AccAPAccountDetails result = null;
			foreach (AccAPAccountDetails accDetails in this)
			{
				if (accDetails.A1_IsDefaultAccount == isDefault && accDetails.PaymentMethod == paymentMethod && accDetails.A1_RX_NKAccountCurrency == currencyCode)
				{
					result = accDetails;
					break;
				}
			}
			return result;
		}

		public AccAPAccountDetails GetAutoDirectDebitAccount(ZString currencyCode)
		{
			return GetAccountDetails(ZArchitecture.Core.ReceiptTypes.DirectDebit, currencyCode, ZBool.True);
		}
	}
}
