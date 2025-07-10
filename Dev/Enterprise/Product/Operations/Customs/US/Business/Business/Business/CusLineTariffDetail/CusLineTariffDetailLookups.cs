using CargoWise.Integration;

namespace Enterprise.Customs.US.Business
{
	public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		public USCTariffCollection ImportTariffs => new USCTariffCollection(Factory);

		public override ICodeDescriptionPairList TariffTypeList => Factory.GetCachedValue<CusLineTariffTypeList>();
	}
}
