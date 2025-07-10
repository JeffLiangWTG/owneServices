using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class GuaranteeBondDetailLookups : GuaranteeForEntryInstructionLookups
{
	public GuaranteeBondDetailLookups(GuaranteeBondDetail parent) : base(parent)
	{
	}

	protected new GuaranteeBondDetail Parent => (GuaranteeBondDetail)base.Parent;

	protected override CodeDescriptionPairList BondTypeListCore => Factory.GetCachedValue<GuaranteeBondTypeList>();

	public CusGuaranteeHeaderCollection GuaranteeCollection => new CusGuaranteeHeaderCollection(Factory, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, new ZString[] { PLGuaranteeTypeList.Codes.GEN });
}
