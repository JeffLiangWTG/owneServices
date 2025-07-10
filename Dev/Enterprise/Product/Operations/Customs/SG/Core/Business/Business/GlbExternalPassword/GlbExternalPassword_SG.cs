using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class GlbExternalPassword_SG : GlbExternalPassword
	{
		protected GlbExternalPassword_SG(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(32)]
		public override ZString CurrentDecryptedPassword
		{
			get { return base.CurrentDecryptedPassword; }
			set { base.CurrentDecryptedPassword = value; }
		}

		[MaxLength(32)]
		public override ZString NextDecryptedPassword
		{
			get { return base.NextDecryptedPassword; }
			set { base.NextDecryptedPassword = value; }
		}

		public new GlbExternalPasswordValidation_SG Validation
		{
			get { return (GlbExternalPasswordValidation_SG)base.Validation; }
		}
	}
}
