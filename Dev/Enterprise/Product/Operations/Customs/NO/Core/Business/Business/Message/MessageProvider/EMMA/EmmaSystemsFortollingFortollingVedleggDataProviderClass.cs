
using CargoWise.Customs.NO.MessageContracts.EMMA;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class EmmaSystemsFortollingFortollingVedleggDataProviderClass(string fileName, string description, ZDateTime documentDate) : EmmaSystemsFortollingFortollingVedleggDataProviderAbstractClass
{
	public override string Filnavn => fileName;

	public override string Beskrivelse => description;

	public override string DokumentDato => documentDate.ToShortDateString();
}
