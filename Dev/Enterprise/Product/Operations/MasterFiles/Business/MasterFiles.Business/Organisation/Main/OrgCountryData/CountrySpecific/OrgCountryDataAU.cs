using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataAU : OrgCountryData
	{
		public OrgCountryDataAU(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override OrgCountryDataValidation GetNewValidation()
		{
			return new OrgCountryDataAUValidation(this);
		}

		#endregion
	}
}
