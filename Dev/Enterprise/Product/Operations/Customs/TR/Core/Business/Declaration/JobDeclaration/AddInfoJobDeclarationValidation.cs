using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		protected override void CheckJE_ShippingCountry()
		{
			base.CheckJE_ShippingCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ShippingCountryInfo);
		}

		protected override void CheckJE_CountryOfSupply()
		{
			base.CheckJE_CountryOfSupply();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CountryOfSupplyInfo);
		}

		protected override void CheckJE_BankCode()
		{
			base.CheckJE_BankCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_BankCodeInfo);
		}

		protected override void CheckJE_TradeType()
		{
			base.CheckJE_TradeType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TradeTypeInfo);
		}
	}
}
