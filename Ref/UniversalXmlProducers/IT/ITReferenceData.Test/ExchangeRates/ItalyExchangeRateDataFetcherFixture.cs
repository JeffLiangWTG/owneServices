using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExchangeRates
{
	[TestFixture]
	class ItalyExchangeRateDataFetcherFixture
	{
		[Test]
		public void TestFetchData()
		{
			var metaData = new ExchangeRateMetaData();
			metaData.DataLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRates\Res\italianfx.pdf");
			IExchangeRateDataFetcher fetcher = new ItalyExchangeRateDataFetcher(metaData);

			var data = fetcher.Fetch();

			var expected = "CAMBI DOGANALI \n \nAi sensi dell’articolo 146 del regolamento (CEE) n. 2447/2015 (applicativo del Codice doganale unionale), \nin conformità dell’articolo 53, paragrafo 1, lettera a), del codice, i seguenti tassi di cambio sono utilizzati per \nla conversione valutaria ai fini della determinazione del valore in dogana: a) il tasso di cambio pubblicato \ndalla Banca centrale europea (BCE), per gli Stati membri la cui moneta è l’euro; b) il tasso di cambio \npubblicato dall’autorità nazionale competente o, se l’autorità nazionale ha designato una banca privata ai fini \ndella pubblicazione del tasso di cambio, il tasso di cambio pubblicato dalla suddetta banca privata, per gli \nStati membri la cui moneta non è l’euro. Il tasso di cambio da utilizzare in conformità del paragrafo 1 è il \ntasso di cambio pubblicato il penultimo mercoledì di ogni mese. Se in quel giorno non è stato pubblicato \nalcun tasso di cambio, si applica il tasso più recente pubblicato. Il tasso di cambio si applica per un mese, a \ndecorrere dal primo giorno del mese successivo. Se non è stato pubblicato un tasso di cambio di cui ai \nparagrafi 1 e 2, il tasso da utilizzare ai fini dell’applicazione dell’articolo 53, paragrafo 1, lettera a), del \ncodice è stabilito dallo Stato membro interessato. Tale tasso deve riflettere il più fedelmente possibile il \nvalore della moneta dello Stato membro interessato. Per le monete che non sono comprese nell’elenco deve \nessere utilizzato, invece, il tasso di cambio fissato giornalmente, per le stesse, dalla Banca d’Italia. \nTassi di cambio periodici validi nel mese di Aprile 2019 \n \nTassi di cambio in vigore per tutto il mese di aprile 2019 in conformità dell’art. 53 del CDU (Reg. UE n. \n952/2013) e dell’art. 146 del R.E. (UE) n. 2015/2447. Tutte le quotazioni sono determinate in unità di valuta \nestera contro euro. \n \nUSD US dollar 1.1354 \nJPY Japanese yen 126.63 \nBGN Bulgarian lev 1.9558 \nCZK Czech koruna 25.646 \nDKK Danish krone 7.4624 \nGBP Pound sterling 0.86280 \nHUF Hungarian forint 313.29 \nPLN Polish zloty 4.2834 \nRON Romanian leu 4.7615 \nSEK Swedish krona 10.4310 \nCHF Swiss franc 1.1338 \nISK Icelandic krona 132.90 \nNOK Norwegian krone 9.6915 \nHRK Croatian kuna 7.4178 \nRUB Russian rouble 73.0040 \nTRY Turkish lira 6.2152 \nAUD Australian dollar 1.5999 \nBRL Brazilian real 4.3012 \nCAD Canadian dollar 1.5125 \nCNY Chinese yuan renminbi 7.6014 \nHKD Hong Kong dollar 8.9128 \nIDR Indonesian rupiah 16082.94 \nILS Israeli shekel 4.0974 \nINR Indian rupee 78.1360 \nKRW South Korean won 1281.96 \nMXN Mexican peso 21.5168 \nMYR Malaysian ringgit 4.6109 \nNZD New Zealand dollar 1.6567 \nPHP Philippine peso 60.042 \nSGD Singapore dollar 1.5335 \nTHB Thai baht 36.032 \nZAR South African rand 16.3783 \n ";
			Assert.AreEqual(true, data.UnProcessedData.Contains(expected));
		}
	}
}
