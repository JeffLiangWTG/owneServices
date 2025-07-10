using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class HolderOfTheTransitProcedureInterpreter
{
	readonly IHolderOfTheTransitProcedure transitProcedureHolder;

	public HolderOfTheTransitProcedureInterpreter(IHolderOfTheTransitProcedure holder)
	{
		transitProcedureHolder = holder;
	}

	public IEnumerable<IParamStringValue> GetRows(NctsHeader header)
	{
		if (transitProcedureHolder?.IdentificationNumber is string idNumber && !string.IsNullOrEmpty(idNumber))
		{
			yield return new ParamValue(HolderOfTheTransitProcedure.EORI, idNumber);
		}

		if (transitProcedureHolder?.TIRHolderIdentificationNumber is string tirIdNumber && !string.IsNullOrWhiteSpace(tirIdNumber))
		{
			yield return new ParamValue(HolderOfTheTransitProcedure.TIRIdentificationNumber, tirIdNumber);
		}

		yield return new ParamValue(HolderOfTheTransitProcedure.Name, GetName(header));
		yield return new ParamValue(HolderOfTheTransitProcedure.StreetAddress, GetStreetAndNumber(header));
		yield return new ParamValue(HolderOfTheTransitProcedure.Postcode, GetPostCode(header));
		yield return new ParamValue(HolderOfTheTransitProcedure.City, GetCity(header));
		yield return new ParamValue(HolderOfTheTransitProcedure.Country, GetCountry(header));
	}

	string GetName(NctsHeader header) => transitProcedureHolder?.Name is string name && name.Length != 0
		? name
		: header.Principal.Address?.Header?.OH_FullName ?? string.Empty;

	string GetStreetAndNumber(NctsHeader header)
	{
		var streetAndNumber = transitProcedureHolder?.Address?.StreetAndNumber;
		if (string.IsNullOrEmpty(streetAndNumber))
		{
			var address = header.Principal;
			var address1 = address?.Address1 ?? string.Empty;
			var address2 = address?.Address2 ?? string.Empty;
			var separator = string.IsNullOrEmpty(address1) || string.IsNullOrEmpty(address2) ? string.Empty : " ";
			streetAndNumber = string.Join(separator, address1, address2);
		}
		return streetAndNumber;
	}

	string GetPostCode(NctsHeader header)
	{
		var postCode = transitProcedureHolder?.Address?.PostCode;
		if (string.IsNullOrEmpty(postCode))
		{
			postCode = header.Principal?.Address?.Postcode ?? string.Empty;
		}
		return postCode;
	}

	string GetCity(NctsHeader header)
	{
		var city = transitProcedureHolder?.Address?.City;
		if (string.IsNullOrEmpty(city))
		{
			city = header.Principal?.Address?.City ?? string.Empty;
		}
		return city;
	}
	string GetCountry(NctsHeader header)
	{
		var country = transitProcedureHolder?.Address?.CountryCode;
		if (string.IsNullOrEmpty(country))
		{
			country = header.Principal?.Address?.OA_RN_NKCountryCode ?? string.Empty;
		}
		return country;
	}
}
