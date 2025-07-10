using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public class NADLineFormatter
	{
		public NADLineFormatter(OrgHeader organisation)
			: this(organisation != null ? organisation.Factory : new BusinessObjectFactory())
		{
			OrgAddress address = organisation.MainAddress;
			AddToResult(organisation.OH_FullNameTruncated, address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_State, address.OA_PostCode, organisation.OH_RL_NKClosestPort.Left(2));
		}

		public NADLineFormatter(BusinessObjectFactory factory, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode)
			: this(factory)
		{
			AddToResult(name, address1, address2, city, state, postCode, countryCode);
		}

		NADLineFormatter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;
		List<string> lines;

		public string GetLine(int index)
		{
			return index < lines.Count ? lines[index] : "";
		}
		const int lineLength = 35;
		const int maxLines = 5;

		void AddToResult(ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode)
		{
			ZStringBuilder headerLinesBuilder = new ZStringBuilder();

			headerLinesBuilder.Append(name);
			headerLinesBuilder.AppendIfNotEmpty(address1);
			headerLinesBuilder.AppendIfNotEmpty(address2);

			List<string> firstLines = GetLinesList(headerLinesBuilder.ToStringWithDelimiterBetweenAppends("\r\n"));

			ZString lastLine = GetLastLine(city, state, postCode, countryCode);
			List<string> lastLines = GetLinesList(lastLine);

			lines = new List<string>();
			if ((firstLines.Count + lastLines.Count) <= maxLines)
			{
				lines.AddRange(firstLines);
				lines.AddRange(lastLines);
			}
			else
			{
				headerLinesBuilder = new ZStringBuilder();
				ZString addressLines = new ZStringBuilder().AppendIfNotEmpty(address1).AppendIfNotEmpty(address2).ToStringWithDelimiterBetweenAppends(" ");
				if (name.Length > lineLength)
				{
					headerLinesBuilder.Append(name + ", " + addressLines);
				}
				else
				{
					headerLinesBuilder.Append(name);
					headerLinesBuilder.AppendIfNotEmpty(addressLines);
				}
				firstLines = GetLinesList(headerLinesBuilder.ToStringWithDelimiterBetweenAppends("\r\n"));
				if ((firstLines.Count + lastLines.Count) <= maxLines)
				{
					lines.AddRange(firstLines);
					lines.AddRange(lastLines);
				}
				else
				{
					for (int index = 0; index < firstLines.Count - 1; index++)
					{
						lines.Add(firstLines[index]);
					}
					lines.AddRange(GetLinesList(firstLines[firstLines.Count - 1] + ", " + lastLine));

					if (lines.Count > maxLines)
					{
						ZString all = new ZStringBuilder(this.lines).ToStringWithDelimiterBetweenAppends(" ");
						this.lines = new List<string>();
						ZString[] lines = all.Split(lineLength);
						foreach (ZString line in lines)
						{
							this.lines.Add(line);
						}
					}
				}
			}
		}

		List<string> GetLinesList(ZString source)
		{
			List<string> result = new List<string>();
			StringLineBreaker breaker = new StringLineBreaker(source.ToUpper(), true);
			ZString line = breaker.GetNextLine(lineLength);
			while (!line.IsEmpty)
			{
				result.Add(line);
				line = breaker.GetNextLine(lineLength);
			}
			return result;
		}

		string GetLastLine(ZString city, ZString state, ZString postCode, ZString countryCode)
		{
			RefCountry country = null;
			if (!countryCode.IsEmpty)
			{
				country = RefCountry.LoadFromCountryCode(factory, countryCode);
			}

			ZStringBuilder result = new ZStringBuilder();
			if (country != null)
			{
				// More specific rules are available in the Universal Postal Union's addressing guidelines, 
				// downloadable from http://www.upu.int/post_code/en/countries/
				switch (country.RN_AddressFormattingRule)
				{
					case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityAndState:
						result.AppendIfNotEmpty(postCode);
						result.AppendIfNotEmpty(city);
						result.AppendIfNotEmpty(state);
						break;
					case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityStateInBrackets:
						result.AppendIfNotEmpty(postCode);
						result.AppendIfNotEmpty(city);
						result.AppendIfNotEmpty(state.IsEmpty ? "" : "(" + state.Trim() + ")");
						break;
				}
			}
			if (result.IsEmpty)
			{
				result.AppendIfNotEmpty(city);
				result.AppendIfNotEmpty(state);
				result.AppendIfNotEmpty(postCode);
			}
			return result.ToStringWithDelimiterBetweenAppends(" ");
		}

		public override string ToString()
		{
			return new ZStringBuilder(lines).ToStringWithNewLineBetweenAppends();
		}
	}
}
