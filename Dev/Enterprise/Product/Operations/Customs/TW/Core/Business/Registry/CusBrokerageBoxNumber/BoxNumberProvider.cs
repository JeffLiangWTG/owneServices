using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TW.Business
{
	public class BoxNumberProvider : IBoxNumberProvider
	{
		ICodeDescriptionPairList IBoxNumberProvider.GetBoxNumberList(BusinessObjectFactory factory, ZString customsDistrict) => RegistryHelper.GetBoxNumberList(factory, customsDistrict);

		ZString IBoxNumberProvider.GetDefaultBoxNumber(BusinessObjectFactory factory, ZString customsDistrict) => factory.GetCachedValue("Enterprise.Customs.TW.Business.DefaultBoxNumber|" + GlbCompany.CurrentCompany.PK.ToStringKey() + customsDistrict, () =>
		{
			return RegistryHelper.GetDefaultBoxNumberByCustomsDistrict(customsDistrict);
		});
	}
}
