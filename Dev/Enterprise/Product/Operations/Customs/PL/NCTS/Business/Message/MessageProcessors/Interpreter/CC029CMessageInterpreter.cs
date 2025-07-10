using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC029CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE029>(movementHeader)
{
	protected override void InterpretCore(IIE029 ie029, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE029);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(new ParamValueCollection {
			{ CommonStrings.LRN, ie029.LRN },
			{ CommonStrings.MRN, ie029.MRN },
			{ TransitOperation.MessageSentOn, ie029.PreparationDateAndTime },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(ie029.CustomsOfficeOfDeparture) },
			{ TransitOperation.CustomsOfficeOfDestination, Factory.GetOfficeCodeWithDescription(ie029.CustomsOfficeOfDestinationDeclared) },
			{ TransitOperation.DeclarationType, ie029.TransitOperation.DeclarationType },
			{ TransitOperation.TIR, ie029.TransitOperation.TIRCarnetNumber },
			{ TransitOperation.ReleaseDate, ie029.TransitOperation.ReleaseDate },
			{ TransitOperation.GrossWeight, ie029.Consignment.GrossMass },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueSequence(Representative.Caption, new ParamValueCollection {
			{ Representative.EORI, ie029.Representative?.IdentificationNumber ?? string.Empty },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueSequence(HolderOfTheTransitProcedure.Caption, new ParamValueCollection {
			{ HolderOfTheTransitProcedure.EORI, ie029.HolderOfTheTransitProcedure.IdentificationNumber },
			{ HolderOfTheTransitProcedure.TIR_HolderID, ie029.HolderOfTheTransitProcedure.TIRHolderIdentificationNumber },
			{ HolderOfTheTransitProcedure.Name, ie029.HolderOfTheTransitProcedure.Name.IfNullOrEmpty(() => NctsHeader.Principal.Organisation?.OH_FullName) },
			{ HolderOfTheTransitProcedure.StreetAddress, (ie029.HolderOfTheTransitProcedure.Address?.StreetAndNumber).IfNullOrEmpty(() =>
				new ZStringBuilder(NctsHeader.Principal.Address1).AppendIfNotEmpty(NctsHeader.Principal.Address2).ToStringWithDelimiterBetweenAppends(" ")) },
			{ HolderOfTheTransitProcedure.Postcode, (ie029.HolderOfTheTransitProcedure.Address?.PostCode).IfNullOrEmpty(() => NctsHeader.Principal.Postcode) },
			{ HolderOfTheTransitProcedure.City, (ie029.HolderOfTheTransitProcedure.Address?.City).IfNullOrEmpty(() => NctsHeader.Principal.City) },
			{ HolderOfTheTransitProcedure.Country, (ie029.HolderOfTheTransitProcedure.Address?.CountryCode).IfNullOrEmpty(() => NctsHeader.Principal.Country?.Code) },
		});
	}
}
