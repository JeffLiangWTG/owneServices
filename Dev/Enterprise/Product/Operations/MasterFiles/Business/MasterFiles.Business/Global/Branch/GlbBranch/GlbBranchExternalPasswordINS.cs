using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// External Password for India EInvoicing Service Provider
	/// </summary>
	public sealed class GlbBranchExternalPasswordINS : GlbBranchCredential
	{
		public GlbBranchExternalPasswordINS(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override string PasswordTypeCode => PasswordTypesList.Codes.INS;
		public override string PasswordTypeDescription => PasswordTypesList.Descriptions.INS;

		[ResourceStringData("bbffeb28-2de4-4b62-a2ef-383ee908d556", Caption = "Client Id")]
		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set => base.GP_UserID = value;
		}

		[ResourceStringData("743de767-43e3-4994-ac0e-bfcbe2644df2", Caption = "Client Secret")]
		public override ZString CurrentDecryptedPassword
		{
			get => base.CurrentDecryptedPassword;
			set => base.CurrentDecryptedPassword = value;
		}

		#endregion
	}
}
