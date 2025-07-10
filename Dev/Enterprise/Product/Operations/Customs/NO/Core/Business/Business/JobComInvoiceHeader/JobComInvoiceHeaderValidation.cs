using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

public class JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
	: AutoNOJobComInvoiceHeaderValidation(invoiceHeader)
{
	protected override void CheckJZ_InvoiceDate()
	{
		base.CheckJZ_InvoiceDate();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceDateInfo);
	}

	protected override void CheckJZ_ValuationCode()
	{
		base.CheckJZ_ValuationCode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_ValuationCodeInfo);

		if (Parent.InvoiceLines.Any(x => x.JI_Procedure == UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode))
		{
			var valuationCodesAllowedWith6021 = new ZString[] { "05", "10" };
			if (!valuationCodesAllowedWith6021.Contains(Parent.JZ_ValuationCode))
			{
				var errorMessage = Res.GetString("905F8792-A46A-5CBA-4D9D-DFBF429329BC", "Procedure 6021 requires transaction nature 05 or 10.");
				Parent.JZ_ValuationCodeInfo.AddMessageError(errorMessage);
			}
		}
	}

	protected override void CheckJZ_InvoiceCurrExRate()
	{
		base.CheckJZ_InvoiceCurrExRate();
		var parent = Parent as JobComInvoiceHeader;

		if (parent != null && parent.IsJZ_InvoiceCurrExRateUserEnterable)
		{
			MandatoryValidation.MessageErrorIfIsNegative(parent.JZ_InvoiceCurrExRateInfo);
			MandatoryValidation.MessageErrorIfNotEntered(parent.JZ_InvoiceCurrExRateInfo);
		}
	}

	protected override void CheckJZ_IncoTermPlace()
	{
		base.CheckJZ_IncoTermPlace();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_IncoTermPlaceInfo);
	}
}
