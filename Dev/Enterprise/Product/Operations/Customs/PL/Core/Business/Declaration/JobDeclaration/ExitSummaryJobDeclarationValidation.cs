using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExitSummaryJobDeclarationValidation(JobDeclaration parent) : BaseExportJobDeclarationValidation(parent)
{
	protected override void CheckJE_MessageType()
	{
		base.CheckJE_MessageType();

		var declaration = Parent;
		if (declaration.ItineraryCountries.Count == 0)
		{
			declaration.JE_MessageTypeInfo.AddMessageError(Res.GetString("B812B5D6-D330-4C52-9D93-31F24A259216", "Itinerary Countries are required for EXS - Exit Summary Declaration."));
		}
	}

	protected override void CheckJE_TransportModeMandatory()
	{
	}

	protected override void CheckJE_OH_ShippingLine()
	{
		base.CheckJE_OH_ShippingLine();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ShippingLineInfo);
	}
}
