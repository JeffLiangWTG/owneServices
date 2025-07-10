using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class WarehouseOrganisationsFindBoxCollection : OrganisationsFindBoxCollection
	{
		public WarehouseOrganisationsFindBoxCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property1", (ZString)Core.Constants.CountryCodes.Taiwan, FilterOrCategory.None));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property2", (ZString)OrgCusCode.CodeTypes.WarehouseControlledPremisesID, FilterOrCategory.None));
		}
	}
}
