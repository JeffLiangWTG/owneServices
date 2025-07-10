using System.Collections.Specialized;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.PL.NCTS.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
sealed class CC028CMessageInterpreter(NctsCommonMovementHeader movementHeader) : MessageInterpreterBase<NctsCommonMovementHeader, IIE028>(movementHeader)
{
	NctsCommonMovementHeader MovementHeader => LinkedObject;
	NctsHeader NctsHeader => (NctsHeader)LinkedObject.Header;
	BusinessObjectFactory Factory => MovementHeader.Factory;

	protected override ZString InterpretCore(IIE028 dataProvider)
	{
		var htmlBody = new ZStringBuilder();

		var tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("IE028 - MRN ALLOCATED", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true)
		);

		htmlBody.Append(tableCreator.ToHtml());
		tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);

		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("LRN", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true),
			new CellWithFormatting(" : " + dataProvider.LRN, new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Message Sent On", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true),
			new CellWithFormatting(" : " + dataProvider.PreparationDateAndTime, new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Customs Office of Departure", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true),
			new CellWithFormatting(" : " + Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);

		htmlBody.Append(tableCreator.ToHtml());
		tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);

		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("MRN", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true),
			new CellWithFormatting(" : " + dataProvider.MRN, new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Acceptance Date", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true),
			new CellWithFormatting(" : " + dataProvider.DeclarationAcceptanceDate.ToShortDateString(), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);

		htmlBody.Append(tableCreator.ToHtml());
		tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);

		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Holder of the Transit Procedure", new NameValueCollection { { "align", "left" }, { "width", "700" } }, true)
		);
		var identificationNumber = dataProvider.HolderOfTheTransitProcedure?.IdentificationNumber;
		if (!string.IsNullOrEmpty(identificationNumber))
		{
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("EORI", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
				new CellWithFormatting(" : " + identificationNumber, new NameValueCollection { { "align", "left" }, { "width", "500" } })
			);
		}

		var tirHolderIdentificationNumber = dataProvider.HolderOfTheTransitProcedure?.TIRHolderIdentificationNumber;
		if (!string.IsNullOrEmpty(tirHolderIdentificationNumber))
		{
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("TIR Holder Identification Number", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
				new CellWithFormatting(" : " + tirHolderIdentificationNumber, new NameValueCollection { { "align", "left" }, { "width", "500" } })
			);
		}

		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Name", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
			new CellWithFormatting(" : " + GetName(dataProvider), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Street & Address", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
			new CellWithFormatting(" : " + GetStreetAndNumber(dataProvider), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Postcode", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
			new CellWithFormatting(" : " + GetPostCode(dataProvider), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("City", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
			new CellWithFormatting(" : " + GetCity(dataProvider), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);
		tableCreator.WriteRowWithFormatting(
			new CellWithFormatting("Country", new NameValueCollection { { "align", "left" }, { "width", "700" } }),
			new CellWithFormatting(" : " + GetCountry(dataProvider), new NameValueCollection { { "align", "left" }, { "width", "500" } })
		);

		htmlBody.Append(tableCreator.ToHtml());
		return htmlBody.ToStringWithDelimiterBetweenAppends("<hr />");
	}

	string GetName(IIE028 dataProvider)
	{
		var name = dataProvider.HolderOfTheTransitProcedure?.Name;
		if (string.IsNullOrEmpty(name))
		{
			name = NctsHeader.Principal.Address?.Header?.OH_FullName ?? string.Empty;
		}
		return name;
	}

	string GetStreetAndNumber(IIE028 dataProvider)
	{
		var streetAndNumber = dataProvider.HolderOfTheTransitProcedure?.Address?.StreetAndNumber;
		if (string.IsNullOrEmpty(streetAndNumber))
		{
			var address = NctsHeader.Principal;
			var address1 = address?.Address1 ?? string.Empty;
			var address2 = address?.Address2 ?? string.Empty;
			var separator = string.IsNullOrEmpty(address1) || string.IsNullOrEmpty(address2) ? string.Empty : " ";
			streetAndNumber = string.Join(separator, address1, address2);
		}
		return streetAndNumber;
	}

	string GetPostCode(IIE028 dataProvider)
	{
		var postCode = dataProvider.HolderOfTheTransitProcedure?.Address?.PostCode;
		if (string.IsNullOrEmpty(postCode))
		{
			postCode = NctsHeader.Principal?.Address?.Postcode ?? string.Empty;
		}
		return postCode;
	}

	string GetCity(IIE028 dataProvider)
	{
		var city = dataProvider.HolderOfTheTransitProcedure?.Address?.City;
		if (string.IsNullOrEmpty(city))
		{
			city = NctsHeader.Principal?.Address?.City ?? string.Empty;
		}
		return city;
	}

	string GetCountry(IIE028 dataProvider)
	{
		var country = dataProvider.HolderOfTheTransitProcedure?.Address?.CountryCode;
		if (string.IsNullOrEmpty(country))
		{
			country = NctsHeader.Principal?.Address?.OA_RN_NKCountryCode ?? string.Empty;
		}
		return country;
	}
}
