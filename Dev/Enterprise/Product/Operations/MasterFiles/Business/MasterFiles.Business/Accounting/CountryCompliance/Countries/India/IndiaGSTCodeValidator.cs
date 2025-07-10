using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class IndiaGSTCodeValidator
	{
		string CodeType => OrgCusCode.CodeTypes.GSTCode;

		int ValidLength => 15;

		string InvalidLengthMessage => Res.GetString("7FCAC9BF-2739-4E86-AA45-0424B6220101", "{0} Numbers in India must be 15 characters long.", CodeType);

		string InvalidPrefixMessage(string statePrefix) => Res.GetString("DC12B7C2-5931-4DC2-BFCC-7069C6D1153D", "This {0} Number must begin with {1}", CodeType, statePrefix);

		string InvalidStateMessage(string stateCode) => Res.GetString("D5338A42-331B-41DD-ACCF-0BDA555EA623", "The state code {0} is not a valid state for India", stateCode);

		readonly static ImmutableDictionary<string, string> GSTPrefixLookup = (
			new Dictionary<string, string>
			{
				{ "JK", "01" }, { "HP", "02" }, { "PB", "03" }, { "CH", "04" },
				{ "UT", "05" }, { "HR", "06" }, { "DL", "07" }, { "RJ", "08" },
				{ "UP", "09" }, { "BR", "10" }, { "SK", "11" }, { "AR", "12" },
				{ "NL", "13" }, { "MN", "14" }, { "MZ", "15" }, { "TR", "16" },
				{ "ML", "17" }, { "AS", "18" }, { "WB", "19" }, { "JH", "20" },
				{ "OR", "21" }, { "CT", "22" }, { "MP", "23" }, { "GJ", "24" },
				{ "DD", "25" }, { "DN", "26" }, { "MH", "27" }, { "KA", "29" },
				{ "GA", "30" }, { "LD", "31" }, { "KL", "32" }, { "TN", "33" },
				{ "PY", "34" }, { "AN", "35" }, { "TG", "36" }, { "TS", "36" },
				{ "AP", "37" }, { "AD", "37" }, { "FC", "96" }, { "OT", "97" },
				{ "LA", "38" }, { "DH", "26" }
			}
		).ToImmutableDictionary();

		internal void Validate(ZPropertyInfo codeInfo, OrgCusCode orgCusCode)
		{
			string countryCode = orgCusCode.Organisation?.MainAddress?.Country?.Code ?? string.Empty;
			if (countryCode == Core.Constants.CountryCodes.India)
			{
				if (((ZString)codeInfo.Value).Length != ValidLength)
				{
					codeInfo.AddError(InvalidLengthMessage);
				}
				string state = orgCusCode.Organisation?.MainAddress?.StateCode ?? string.Empty;
				if (GSTPrefixLookup.TryGetValue(state, out string prefix))
				{
					ZString expectedPrefix = ((ZString)(codeInfo.Value)).SubstringSafe(0, 2);
					if (expectedPrefix != prefix)
					{
						codeInfo.AddError(InvalidPrefixMessage(prefix));
					}
				}
				else
				{
					codeInfo.AddError(InvalidStateMessage(state));
				}
			}
		}

#if DEBUG
		public ImmutableDictionary<string, string> GSTPrefixLookup_ForTestOnly => GSTPrefixLookup;
#endif
	}
}
