using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

sealed public class CusEntryHeaderValidation : AutoNOCusEntryHeaderValidation
{
	public CusEntryHeaderValidation(CusEntryHeader parent)
		: base(parent)
	{
	}

	protected override void CheckCH_ReCalcCaseCode()
	{
		base.CheckCH_ReCalcCaseCode();
		ListValidation.ErrorIfInvalidCode(Parent.CH_ReCalcCaseCodeInfo);
	}

	protected override void CheckCH_ReCalcOrigDecl()
	{
		var parent = Parent;
		if (parent.Declaration is JobDeclaration jobDeclaration && jobDeclaration.JE_CopyStatus == NODeclarationCopyStatus.Codes.Recalculation)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CH_ReCalcOrigDeclInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(
				parent.CH_ReCalcOrigDeclInfo,
				jobDeclaration.CustomsEntryHeaders,
				Res.GetString("2F33F602-AA19-40C1-94B8-82C932198312", "{0}", duplicateFoundMessageError));
			parent.CH_ReCalcOrigDeclInfo.AddWarning(
				Res.GetString("A6DD19BA-5F50-48CA-891C-7B2A9A96CE6F", "{0}", recalcOrigDeclMessageWarning));
		}
	}

	protected override void CheckCH_PaymentMethod()
	{
		base.CheckCH_PaymentMethod();
		var targetInfo = Parent.CH_PaymentMethodInfo;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
	const string duplicateFoundMessageError = "Value already defined, please ensure that there are no duplicates.";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Warning message")]
	const string recalcOrigDeclMessageWarning = "This is a recalculation job. To see original go to Misc tab and click on Parent button.";
}
