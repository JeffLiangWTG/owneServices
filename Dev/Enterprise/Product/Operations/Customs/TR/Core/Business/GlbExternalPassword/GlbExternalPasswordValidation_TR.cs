using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class GlbExternalPasswordValidation_TR : GlbExternalPasswordValidation
	{
		public GlbExternalPasswordValidation_TR(GlbExternalPassword_TR parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword_TR Parent => (GlbExternalPassword_TR)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTR_Chipset();
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			if (!Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
			}

			if (Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.CurrentDecryptedPasswordInfo);
			}
		}

		protected override void CheckGP_CertificateAuthorityIsWesternEuropean()
		{
		}

		protected override void CheckGP_CertificateAuthority()
		{
			base.CheckGP_CertificateAuthority();

			ListValidation.ErrorIfInvalidCode(Parent.GP_CertificateAuthorityInfo, Parent.Lookups.CertificateAuthorities);

			if (!Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.GP_CertificateAuthorityInfo);
			}

			if (Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.GP_CertificateAuthorityInfo);
			}
		}

		public void ValidateTR_Chipset()
		{
			ValidateCalculatedProperty(Parent.TR_ChipsetInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validation method called by reflection")]
		void CheckTR_Chipset()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TR_ChipsetInfo, Parent.Lookups.ChipsetList);

			if (!Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TR_ChipsetInfo);
			}

			if (Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.TR_ChipsetInfo);
			}
		}

		protected override void CheckGP_CertificateSerialNumber()
		{
			base.CheckGP_CertificateSerialNumber();
			if (!Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.GP_CertificateSerialNumberInfo);
				CheckHexValue(Parent.GP_CertificateSerialNumberInfo);
			}
			if (Parent.GP_UserID.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.GP_CertificateSerialNumberInfo);
			}
		}

		void CheckHexValue(ZPropertyInfo info)
		{
			string value = (ZString)info.Value;

			var regex = new Regex("^[0-9A-F]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (!regex.IsMatch(value))
			{
				info.AddError(Res.GetString("HexValue|InvalidCharacter", "Must contain only valid hexadecimal characters (0-9, A-F)."));
			}

			if (value.Length % 2 != 0)
			{
				info.AddError(Res.GetString("HexValue|OddLength", "Must contain even number of digits."));
			}
		}
	}
}
