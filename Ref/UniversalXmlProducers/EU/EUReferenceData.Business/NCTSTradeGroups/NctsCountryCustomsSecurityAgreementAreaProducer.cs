namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCustomsSecurityAgreementAreaProducer : NctsTradeGroupXMLProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryCustomsSecurityAgreementAreaType();
	}
}
