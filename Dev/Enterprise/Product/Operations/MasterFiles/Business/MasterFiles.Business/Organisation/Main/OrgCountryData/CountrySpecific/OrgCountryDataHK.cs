using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataHK : OrgCountryDataEUStyle
	{
		public OrgCountryDataHK(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		internal override ZArchitecture.Environment.BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK; }
		}

		#endregion

		#region Validation

		protected override OrgCountryDataValidation GetNewValidation()
		{
			return new OrgCountryDataHKValidation(this);
		}

		#endregion
	}
}
