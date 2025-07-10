using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgCountryDataEUStyle : OrgCountryData
	{
		public OrgCountryDataEUStyle(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal abstract BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity { get; }
	}
}
