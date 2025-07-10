using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class ControlMessageInterpreter : BaseMessageInterpreter<IControlIncomingDataProvider>
{
	public override string Interpret(IControlIncomingDataProvider dataProvider, EDIMessage ediMessage)
	{
		if (dataProvider.Response is IControlResponse response && response.Errors.Count > 0)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.Append(FormattableString.Invariant($"<h1>{NLEDIMessageInterpreterHelper.ResStrings.ErrorInformationTitle}</h1><br/>"));
			stringBuilder.Append(FormattableString.Invariant($"<b>{NLEDIMessageInterpreterHelper.ResStrings.FunctionalReferenceID}:</b> {response.FunctionalReferenceId}<br/><br/>"));
			stringBuilder.Append(FormattableString.Invariant($"<table><tr><th>{NLEDIMessageInterpreterHelper.ResStrings.ErrorText}</th><th>{NLEDIMessageInterpreterHelper.ResStrings.OriginalValue}</th><th>{NLEDIMessageInterpreterHelper.ResStrings.Position}</th></tr>"));
			foreach(var error in response.Errors)
			{
				var originalValue = Regex.Match(error.Description, @"(?<=Message:Value\s*')([^']*)(?=\')").Value.Trim();
				var lineNumber = Regex.Match(error.Pointers.FirstOrDefault()?.Location, @"(?<=Line-number:\s*)\d+(?=\s*###)").Value.Trim();
				var columnNumber = Regex.Match(error.Pointers.FirstOrDefault()?.Location, @"(?<=Column-number:\s*)\d+").Value.Trim();
				stringBuilder.Append(FormattableString.Invariant($"<tr><td>{error.Description}</td><td>{originalValue}</td><td>Seg {lineNumber} (LOC) {columnNumber}</td></tr>"));
			}
			stringBuilder.Append((NoResString)"</table>");
			return stringBuilder.ToString();
		}
		else
		{
			return string.Empty;
		}
	}
}
