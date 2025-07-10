using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbExternalPasswordWithPasswordType : GlbExternalPassword
	{
		public GlbExternalPasswordWithPasswordType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract string PasswordTypeCode { get; }
		public abstract string PasswordTypeDescription { get; }

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypeCode;
		}

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			var newValue = GP_CurrentPassword;
			if (oldValue != newValue)
			{
				GP_PasswordStatus = ZString.Empty;
			}
		}

		public new GlbExternalPasswordWithPasswordTypeLookups Lookups => (GlbExternalPasswordWithPasswordTypeLookups)base.Lookups;
		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordWithPasswordTypeLookups(this);
		}

		public new GlbExternalPasswordWithPasswordTypeValidation Validation => (GlbExternalPasswordWithPasswordTypeValidation)GetNewValidation();
		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordWithPasswordTypeValidation(this);
		}

		#endregion
	}
}
