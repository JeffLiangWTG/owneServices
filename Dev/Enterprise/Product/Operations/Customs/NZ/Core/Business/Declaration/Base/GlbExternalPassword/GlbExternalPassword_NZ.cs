using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public sealed class GlbExternalPassword_NZ : GlbExternalPassword
	{
		public GlbExternalPassword_NZ(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.NZB;
		}

		[MaxLength(35)]
		public override ZString CurrentDecryptedPassword
		{
			get { return base.CurrentDecryptedPassword; }
			set { base.CurrentDecryptedPassword = value; }
		}
	}
}
