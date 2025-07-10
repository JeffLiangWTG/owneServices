using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class BondedFactoryOrgHeaderCollection : OrgHeaderCollection
	{
		public BondedFactoryOrgHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults();
		}

		void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property1", (ZString)Core.Constants.CountryCodes.Taiwan));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property2", (ZString)OrgCusCode.TaiwanCodeTypes.CBF));
		}
	}
}
