using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public class Constants
	{
		public const string EUNTariffStartPage = @"http://ec.europa.eu/taxation_customs/dds2/taric/taric_consultation.jsp?Lang=en";
		public const string baseAddress = @"http://ec.europa.eu/taxation_customs/dds2/taric/";
		public const string SearchPageWithDates = @"measures.jsp?Lang=en&Domain=TARIC&Offset=$PageOffSet$&ShowMatchingGoods=false&callbackuri=CBU-1&SimDate=$EndDate$&StartPub=$StartDate$&EndPub=$EndDate$";
		public const string dateTimeFormat = @"yyyyMMdd";
	}
}
