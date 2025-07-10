namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryOutsideCustomsSecurityAgreementAreaProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryOutsideCustomsSecurityAgreementArea();
	}
}
