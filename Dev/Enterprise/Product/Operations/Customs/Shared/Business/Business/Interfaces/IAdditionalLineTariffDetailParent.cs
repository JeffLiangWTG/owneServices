namespace Enterprise.Customs.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Implemented in other solution: Customs.DataTransfer, Customs.ZA")]
	public interface IAdditionalLineTariffDetailParent
	{
		ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails { get; }
	}
}
