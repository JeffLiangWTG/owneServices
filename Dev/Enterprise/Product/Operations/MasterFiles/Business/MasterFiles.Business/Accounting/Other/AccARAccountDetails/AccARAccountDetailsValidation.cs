using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccARAccountDetailsValidation : AccAPAccountDetailsValidation
	{
		public AccARAccountDetailsValidation(AutoAccAPAccountDetails parent)
				: base(parent)
		{
			this.Parent = (AccARAccountDetails)parent;
		}

		readonly new AccARAccountDetails Parent;

		protected override void CheckA1_PaymentMethod()
		{
			base.CheckA1_PaymentMethod();
			if (Parent.CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.A1_PaymentMethodInfo);
				ListValidation.ErrorIfInvalidCode(Parent.A1_PaymentMethodInfo);
			}
			ValidateA1_IsDefaultAccount();
		}

		protected override void CheckA1_IsDefaultAccount()
		{
			if (Parent.CompanyData.OB_IsDebtor && !Parent.A1_PaymentMethod.IsEmpty && !Parent.A1_RX_NKAccountCurrency.IsEmpty)
			{
				ZString errorMessage = ZString.Empty;
				if (Parent.A1_IsDefaultAccount)
				{
					errorMessage = Res.GetString("7b8acf55-5478-4ca1-a162-c6ba09860ab5", "You have more than one account listed as the default account for the currency \"{0}\" and the payment type \"{1}\".\r\n\r\nPlease check which account really should be the default, and untick the \"Is Default Account\" flag of any others.", Parent.A1_RX_NKAccountCurrency, Parent.A1_PaymentMethodList.GetDescriptionFromCode(Parent.A1_PaymentMethod));
				}
				else
				{
					errorMessage = Res.GetString("70d92829-92d5-4692-9f3b-36deff0721ad", "This account must be the default because there is no other account listed with currency \"{0}\" and payment type \"{1}\" and marked as \"Is Default Account\".", Parent.A1_RX_NKAccountCurrency, Parent.A1_PaymentMethodList.GetDescriptionFromCode(Parent.A1_PaymentMethod));
				}
				SetErrorMessage(errorMessage);
			}
		}

		void SetErrorMessage(ZString errorMessage)
		{
			if (MoreThanOneAccountDetailsFound == Parent.A1_IsDefaultAccount)
			{
				Parent.A1_IsDefaultAccountInfo.AddError(errorMessage);
			}
		}

		bool MoreThanOneAccountDetailsFound
		{
			get
			{
				return Parent.CompanyData.ARAccountDetailsCollection.Cast<AccARAccountDetails>().Any(accDetails => accDetails.PaymentMethod == Parent.PaymentMethod && accDetails.A1_RX_NKAccountCurrency == Parent.A1_RX_NKAccountCurrency && accDetails.A1_IsDefaultAccount && accDetails.PK != Parent.PK);
			}
		}

		public new void ValidateAllIsDefaultAccount()
		{
			foreach (AccARAccountDetails accDetails in Parent.CompanyData.ARAccountDetailsCollection)
			{
				accDetails.Validation.ValidateA1_IsDefaultAccount();
			}
		}

		protected override void CheckA1_BankAccount()
		{
			base.CheckA1_BankAccount();
			CheckEntered(Parent.A1_BankAccountInfo);
		}

		protected override void CheckA1_BankName()
		{
			base.CheckA1_BankName();
			CheckEntered(Parent.A1_BankNameInfo);
		}

		void CheckEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
		}
	}
}
