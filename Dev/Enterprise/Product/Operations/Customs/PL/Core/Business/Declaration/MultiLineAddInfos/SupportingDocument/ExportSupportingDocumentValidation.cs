using System.Linq;
using CargoWise.EntityFramework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportSupportingDocumentValidation(SupportingDocument parent) : SupportingDocumentValidation(parent)
{
	protected override void CheckCSI_ValueIsValidMoney()
	{
		TypeValidation.CheckValidMoney(Parent.CSI_ValueInfo, 16, 2);
	}

	protected override void CheckCSI_Value()
	{
		base.CheckCSI_Value();
		if (!Parent.CSI_RX_NKCurrency.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_ValueInfo);
		}

		MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_ValueInfo);
	}

	protected override void CheckCSI_RX_NKCurrency()
	{
		base.CheckCSI_RX_NKCurrency();
		if (Parent.CSI_Value > 0)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_RX_NKCurrencyInfo);
		}
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		CheckRuleR303();
	}

	void CheckRuleR303()
	{
		switch (Parent.CSI_Code)
		{
			case SupportingDocumentCodes.C512 when GetRelatedEntryInstructions().Any(ins => !ins.IsSimplifiedDeclaration()):
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ExportSupportingDocumentValidation|CheckRuleR303A", "(R303) Supporting document code C512 can only be used for Entry Sub Style C,F or Y."));
				break;
			case SupportingDocumentCodes.C513 when GetRelatedEntryInstructions().Any(ins => !ins.IsSimplifiedDeclaration() && !ins.IsNormalDeclaration()):
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ExportSupportingDocumentValidation|CheckRuleR303B", "(R303) Supporting document code C513 can only be used for Entry Sub Style A,C,D,F or Y."));
				break;
			case SupportingDocumentCodes.C514 when GetRelatedEntryInstructions().Any(ins => !ins.IsSimplifiedDeclaration() && !ins.IsNormalDeclaration() && !ins.IsUnderSimplifiedProcedure()):
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("ExportSupportingDocumentValidation|CheckRuleR303C", "(R303) Supporting document code C514 can only be used for Entry Sub Style A,C,D,F,Y or Z."));
				break;
		}
	}
}
