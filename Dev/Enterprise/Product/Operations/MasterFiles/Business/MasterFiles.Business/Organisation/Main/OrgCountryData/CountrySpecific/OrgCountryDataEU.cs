using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataEU : OrgCountryDataEUStyle
	{
		public OrgCountryDataEU(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ZString DefaultCountryValue
		{
			get { return Constants.CountryCodes.EuropeanUnion; }
		}

		internal override BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU; }
		}

		#endregion

		#region Validation

		protected override OrgCountryDataValidation GetNewValidation()
		{
			return new OrgCountryDataEUValidation(this);
		}

		#endregion
	}
}
