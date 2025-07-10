using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// External Password for India EInvoicing Tax Payer
	/// </summary>
	public sealed class GlbBranchExternalPasswordINT : GlbBranchCredential
	{
		public GlbBranchExternalPasswordINT(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override string PasswordTypeCode => PasswordTypesList.Codes.INT;
		public override string PasswordTypeDescription => PasswordTypesList.Descriptions.INT;

		[ResourceStringData("432a01b9-d43b-442c-b402-a12d8c003222", Caption = "User Id")]
		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set => base.GP_UserID = value;
		}

		[ResourceStringData("f98dd6d6-e4b5-45f2-a988-6e2dc5466aa0", Caption = "Password")]
		public override ZString CurrentDecryptedPassword
		{
			get => base.CurrentDecryptedPassword;
			set => base.CurrentDecryptedPassword = value;
		}

		#endregion
	}
}
