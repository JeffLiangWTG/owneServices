using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

public class GuaranteeBondDetailValidation : GuaranteeForEntryInstructionValidation
{
	public GuaranteeBondDetailValidation(GuaranteeForEntryInstruction parent) : base(parent)
	{
	}

	protected new GuaranteeBondDetail Parent => (GuaranteeBondDetail)base.Parent;

	protected override void CheckPW_BondType()
	{
		base.CheckPW_BondType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PW_BondTypeInfo);
	}

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();
		var targetInfo = Parent.PW_BondNumberInfo;
		var bondType = Parent.PW_BondType;
		var bondNumber = Parent.PW_BondNumber;

		MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
		if (bondType == GuaranteeBondTypeList.Codes.Comprehensive && bondNumber.ToUpper().Contains("A"))
		{
			targetInfo.AddMessageError(Res.GetString("B0D31BD3-B804-4A40-B096-59B063CBE29B", "With Type {0} Guarantee Number can't contain \"A\"", GuaranteeBondTypeList.Codes.Comprehensive));
		}
		if (!bondNumber.IsEmpty && (Parent.EntryInstruction?.HasInvoiceLineStartWithProcedureCodeValue("71") ?? false))
		{
			targetInfo.AddMessageError(Res.GetString("76276124-5A31-4C23-880B-C9B5128196D1", "Guarantees not allowed, because an invoice line has procedure code starting with 71"));
		}
	}

	protected override bool CheckBondNumberOrBondNumber2IsRequired => false;

	protected override void CheckPW_HolderIdentification()
	{
		base.CheckPW_HolderIdentification();
		if (Parent.PW_BondType == GuaranteeBondTypeList.Codes.Individual)
		{
			MandatoryValidation.MessageErrorIfIsEntered(Parent.PW_HolderIdentificationInfo);
		}
	}

	#region PW_Password

	protected override void CheckPW_Password()
	{
		base.CheckPW_HolderIdentification();
		var targetInfo = Parent.PW_PasswordInfo;
		if (Parent.PW_BondType == GuaranteeBondTypeList.Codes.Individual)
		{
			MandatoryValidation.MessageErrorIfIsEntered(targetInfo);
		}

		var password = Parent.PW_Password;
		if (!password.IsEmpty)
		{
			if (password.ContainsAnyLetters)
			{
				Parent.PW_PasswordInfo.AddMessageError(Res.GetString("PLGuaranteeBondDetailValidation|AccessCodeOnlyDigitsMessage", "Access Code should contain digits only"));
			}

			if (password.Length < 4)
			{
				Parent.PW_PasswordInfo.AddMessageError(Res.GetString("PLGuaranteeBondDetailValidation|AccessCodeFourCharactersMessage", "Access Code should contain 4 characters"));
			}
		}
	}
	#endregion

	protected override void CheckPW_BondAmount()
	{
		base.CheckPW_BondAmount();
		var targetInfo = Parent.PW_BondAmountInfo;
		MandatoryValidation.CheckNotNegative(targetInfo);
		if ((Parent.EntryInstruction?.Guarantees?.Count ?? 0) > 1 && Parent.PW_BondAmount == 0)
		{
			targetInfo.AddMessageError(Res.GetString("A6B372FE-8A48-43A4-A52B-2B80CF4AC52A", "With multiple guarantees the amount can't be zero"));
		}
	}

	protected override void CheckPW_CPH_Guarantee()
	{
		base.CheckPW_CPH_Guarantee();
		var targetInfo = Parent.PW_CPH_GuaranteeInfo;
		var bondType = Parent.PW_BondType;
		if (bondType == GuaranteeBondTypeList.Codes.Comprehensive)
		{
			CheckIsValidAccessCode(targetInfo);

			if (Parent.PW_HolderIdentification.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("PLGuaranteeBondDetailValidation|EmptyHolderIdentification", "Selected Guarantee requires TIN."));
			}
		}
		else if (bondType == GuaranteeBondTypeList.Codes.Individual)
		{
			MandatoryValidation.MessageErrorIfIsEntered(Parent.PW_CPH_GuaranteeInfo);
		}
	}

	void CheckIsValidAccessCode(ZPropertyInfo targetInfo)
	{
		var passwordValue = Parent.PW_Password;

		if (passwordValue.IsEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("PLGuaranteeBondDetailValidation|EmptyAccessCode", "Selected Guarantee requires Access Code."));
		}
		else if (passwordValue.Length < 4 || !passwordValue.IsNumbersOnlyOrEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("PLGuaranteeBondDetailValidation|InvalidAccessCodeLengthMessage", "Access Code must have 4 digits"));
		}
	}

	protected override void CheckPW_BondFiledPort() { }
}
