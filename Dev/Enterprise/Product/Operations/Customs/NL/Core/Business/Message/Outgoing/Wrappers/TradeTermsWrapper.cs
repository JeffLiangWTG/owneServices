using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class TradeTermsWrapper : ITradeTerms
{
	public TradeTermsWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = entryHeader;
		randomHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobComInvoiceHeader randomHeader;

	public string ConditionCode => randomHeader.IncoTerm;

	public string Description => null;

	public string LocationID
	{
		get
		{
			var incoTermPlace = new RefUNLOCO.Loader(entryHeader.Factory).Load(randomHeader.ZG_AgreedPlaceCode);
			return incoTermPlace?.Code ?? string.Empty;
		}
	}

	public string LocationName
	{
		get
		{
			var incoTermPlaceCountry = new RefCountry.Loader(entryHeader.Factory).LoadForCountry(randomHeader.ZG_AgreedPlaceCode);
			return incoTermPlaceCountry is null ? string.Empty : randomHeader.JZ_IncoTermPlace.ToString();
		}
	}

	public string CountryCode
	{
		get
		{
			var incoTermPlaceCountry = new RefCountry.Loader(entryHeader.Factory).LoadForCountry(randomHeader.ZG_AgreedPlaceCode);
			return incoTermPlaceCountry?.Code ?? string.Empty;
		}
	}
}
