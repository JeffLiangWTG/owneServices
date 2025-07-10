using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class ExportJobDeclarationValidation
{
	protected override void CheckJE_BorderTransportMeans()
	{
		base.CheckJE_BorderTransportMeans();
		var parent = Parent;
		var declaration = parent;
		var borderTransportMeansInfo = parent.ZG_BorderTransportMeansInfo;
		var borderTransportMeans = parent.ZG_BorderTransportMeans;
		var lookups = parent.Lookups;

		switch (ExportJobDeclarationValidationHelper.GetC0890TransportIdPresence(declaration))
		{
			case PresenceType.Optional:
			case PresenceType.Required:
				if (borderTransportMeans.IsEmpty)
				{
					borderTransportMeansInfo.AddMessageError(Res.GetString("PLExportAddInfoJobDeclarationValidation|ZG_BorderTransportMeansEmptyMessageError", "Please enter a Type of Identification"));
				}
				break;
			case PresenceType.Forbidden when !borderTransportMeans.IsEmpty:
				borderTransportMeansInfo.AddWarning(Res.GetString("EF8DA369-106F-46C0-9C82-C7FC76F089CD", "[C0890] Active Border Transport Means, Border T.O.ID. will be skipped in declaration XML."));
				break;
		}

		ListValidation.MessageErrorIfInvalidCode(borderTransportMeansInfo, lookups.BorderTransportMeansList);
	}

	protected override void CheckJE_PresentationStartDate()
	{
		base.CheckJE_PresentationStartDate();
		CheckRuleR0052E();
	}

	void CheckRuleR0052E()
	{
		var parent = Parent;
		if (parent.CustomsEntryInstructions.Any(x => x.CEI_SubStyle == SubStyleCodes.R))
		{
			var propertyInfo = parent.ZG_PresentationStartDateInfo;
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription: propertyInfo.HumanReadableName, messagePrefix: $"{ValidationRuleMessagePrefixes.R0052E} ");
		}
	}
}
