using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// External Credentials for Philippines EInvoicing User.
	/// </summary>
	public sealed class GlbCompanyExternalPasswordPHU : GlbExternalPasswordWithPasswordType
	{
		public GlbCompanyExternalPasswordPHU(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override string PasswordTypeCode => PasswordTypesList.Codes.PHU;
		public override string PasswordTypeDescription => PasswordTypesList.Descriptions.PHU;

		#endregion
	}
}
