using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// External Credentials for Philippines EInvoicing Application.
	/// </summary>
	public sealed class GlbCompanyExternalPasswordPHA : GlbExternalPasswordWithPasswordType
	{
		public GlbCompanyExternalPasswordPHA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override string PasswordTypeCode => PasswordTypesList.Codes.PHA;
		public override string PasswordTypeDescription => PasswordTypesList.Descriptions.PHA;

		#endregion
	}
}
