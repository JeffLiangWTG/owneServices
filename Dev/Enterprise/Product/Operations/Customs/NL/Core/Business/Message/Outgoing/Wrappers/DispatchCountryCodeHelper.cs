using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class DispatchCountryCodeHelper
{
	public DispatchCountryCodeHelper(CusEntryHeader entryHeader)
	{
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}

	readonly JobDeclaration declaration;

	public ZString GetDispatchCountryCodeForShipment()
	{
		var dispatchCountryCode = ZString.Empty;

		if (ShouldAddCountryOfDispatchOnShipment() && IsDeclarationTypeValid())
		{
			dispatchCountryCode = declaration.JE_RL_NKOrigin.Left(2);
		}
		return dispatchCountryCode;
	}

	public ZString GetDispatchCountryCodeForGoodsItem(CusEntryLine entryLine)
	{
		if (ShouldAddCountryOfDispatchOnShipment() || !IsDeclarationTypeValid())
		{
			return ZString.Empty;
		}

		var dispatchCountryCode = declaration.JE_RL_NKOrigin.Left(2);
		if (entryLine.RandomLine != null)
		{
			return entryLine.RandomLine.ZG_CountryOfDispatch.IsEmpty ? dispatchCountryCode : entryLine.RandomLine.ZG_CountryOfDispatch;
		}

		return dispatchCountryCode;
	}

	bool ShouldAddCountryOfDispatchOnShipment()
	{
		var expectedCode = declaration.JE_RL_NKOrigin.Left(2);
		if (expectedCode.IsEmpty)
		{
			return false;
		}

		foreach (JobComInvoiceLine invLine in declaration.InvoiceLines)
		{
			if (!invLine.ZG_CountryOfDispatch.IsEmpty && invLine.ZG_CountryOfDispatch != expectedCode)
			{
				return false;
			}
		}
		return true;
	}

	bool IsDeclarationTypeValid()
	{
		var allowedDeclarationTypes = new List<ZString> { "H1", "H2", "H3", "H4", "H5", "I1" };

		var decType = declaration.CustomsEntryInstructions.FirstOrDefault()?.CEI_Style ?? ZString.Empty;

		return allowedDeclarationTypes.Contains(decType);
	}
}
