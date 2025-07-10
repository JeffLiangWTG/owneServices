using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public sealed class REGMessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		var status = dataProvider.Statuses.FirstOrDefault();
		if (status?.EffectiveDateTime != null)
		{
			stringBuilder.Append((NoResString)"<table style='margin-left: 10pt'>");
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.RegistrationDate, DMSResponseMessageHelper.GetFormattedLocalLongTimeString(status.EffectiveDateTime.Value));
			stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);
		}

		if (dataProvider.Errors.Count > 0)
		{
			stringBuilder.Append("<BR><H1>");
			stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.ErrorDetailsTitle);
			stringBuilder.Append("</H1>");

			foreach (var error in dataProvider.Errors)
			{
				stringBuilder.Append((NoResString)"<BR/><table style='margin-left: 10pt'>");
				NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ErrorValidation, error.ValidationCode, ZString.Empty);
				NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ErrorDescription, error.Description, ZString.Empty);
				NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ValueThatIsRejected, error.OriginalAttributeValue, ZString.Empty);
				foreach (var location in error.PointerLocations)
				{
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AppliesTo, location, ZString.Empty);
				}

				stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);
			}
		}

		return stringBuilder.ToString();
	}
}
