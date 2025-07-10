using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TranCircumstanceValidation : CusCodeDataValidation
{
	public TranCircumstanceValidation(TranCircumstance parent)
		: base(parent)
	{
	}

	protected new TranCircumstance Parent => (TranCircumstance)base.Parent;

	readonly List<ZString> abcCodeCollection = new List<ZString>() { TranCircumstancesList.Codes.A00PL, TranCircumstancesList.Codes.B00PL, TranCircumstancesList.Codes.C00PL };
	readonly List<ZString> jkCodeCollection = new List<ZString>() { TranCircumstancesList.Codes.J00PL, TranCircumstancesList.Codes.K00PL };

	protected override void CheckCY_Code()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo, Parent.Lookups.TranCircumstancesList);

		if (Parent.Parent != null)
		{
			var jobComInvoiceHeader = Parent.Parent as JobComInvoiceHeader;
			foreach (var item in jobComInvoiceHeader.TranCircumstances)
			{
				if (item.CY_Code == Parent.CY_Code && item != Parent)
				{
					Parent.CY_CodeInfo.AddMessageError(Res.GetString("2B8A9B00-B581-4649-92EF-DF0D9B038C9B", "Duplicate transaction circumstance found."));
				}
				else if (item.CY_Code != Parent.CY_Code && item != Parent)
				{
					if ((abcCodeCollection.Contains(item.CY_Code) && abcCodeCollection.Contains(Parent.CY_Code)) ||
						(jkCodeCollection.Contains(item.CY_Code) && jkCodeCollection.Contains(Parent.CY_Code)))
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("PLTranCircumstanceValidation|InvalidCodeCombinationMessageError", "Transaction Circumstances combination is not allowed ({0}, {1})", Parent.CY_Code, item.CY_Code));
					}
				}
			}
		}
	}
}
