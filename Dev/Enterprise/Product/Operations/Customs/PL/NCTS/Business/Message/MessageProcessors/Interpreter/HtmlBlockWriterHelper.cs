using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

static class HtmlBlockWriterHelper
{
	public static IHtmlBlockWriter Interpret(this IHtmlBlockWriter htmlWriter, IGuarantor guarantor)
	{
		var guarantorAddress = guarantor?.Address;
		htmlWriter.WriteParamValueSequence(
			caption: Guarantor.Caption,
			paramValues: new ParamValueCollection {
				{ Guarantor.EORI, guarantor?.IdentificationNumber },
				{ Guarantor.Name, guarantor?.Name },
				{ Guarantor.StreetAddress, guarantorAddress?.StreetAndNumber },
				{ Guarantor.Postcode, guarantorAddress?.PostCode },
				{ Guarantor.City, guarantorAddress?.City },
				{ Guarantor.Country, guarantorAddress?.CountryCode },
			},
			skipEmptyValues: true);

		return htmlWriter;
	}

	public static IHtmlBlockWriter Interpret(this IHtmlBlockWriter htmlWriter, NctsHeader nctsHeader, IHolderOfTheTransitProcedure holder)
	{
		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(holder).GetRows(nctsHeader));

		return htmlWriter;
	}
}
