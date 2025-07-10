using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataJP : OrgCountryData
	{
		public OrgCountryDataJP(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override OrgCountryDataValidation GetNewValidation()
		{
			return new OrgCountryDataJPValidation(this);
		}

		#endregion
	}
}
