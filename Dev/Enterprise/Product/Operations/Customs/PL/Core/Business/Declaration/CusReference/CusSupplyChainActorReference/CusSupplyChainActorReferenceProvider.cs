using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
{
	protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override ZString OverwrittenReferenceColumnCaptionCore =>
		Res.GetString("PLCusSupplyChainActorReferenceProvider|OverwrittenReferenceColumnCaptionCore", "Identification (TCUI/EORI)");
}
