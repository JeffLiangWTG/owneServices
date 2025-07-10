using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_TR))]
	class GlbExternalPasswordValidationTRTest : Enterprise.MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_TR, GlbExternalPasswordValidation_TR>
	{
		public void TestValidateAll()
		{
			GlbExternalPassword.GP_UserID = "TEST";
			GlbExternalPassword.CurrentDecryptedPassword = "";
			GlbExternalPassword.GP_CertificateAuthority = "";
			GlbExternalPassword.TR_Chipset = "";
			GlbExternalPassword.GP_CertificateSerialNumber = "";

			GlbExternalPassword.Validation.ValidateAll();
			AssertHasErrorContaining(GlbExternalPassword.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(GlbExternalPassword.TR_ChipsetInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, MandatoryValidation.MustBeEntered);

			GlbExternalPassword.GP_UserID = "";
			GlbExternalPassword.CurrentDecryptedPassword = "1";
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EGUVEN;
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.AKIS;
			GlbExternalPassword.GP_CertificateSerialNumber = "1";

			GlbExternalPassword.Validation.ValidateAll();
			AssertHasErrorContaining(GlbExternalPassword.CurrentDecryptedPasswordInfo, MandatoryValidation.DoNotEntered);
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, MandatoryValidation.DoNotEntered);
			AssertHasErrorContaining(GlbExternalPassword.TR_ChipsetInfo, MandatoryValidation.DoNotEntered);
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, MandatoryValidation.DoNotEntered);

			GlbExternalPassword.GP_UserID = "1";
			GlbExternalPassword.CurrentDecryptedPassword = "1";
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EGUVEN;
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.AKIS;
			GlbExternalPassword.GP_CertificateSerialNumber = "1";

			GlbExternalPassword.Validation.ValidateAll();
			AssertNoErrorContaining(GlbExternalPassword.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.TR_ChipsetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, MandatoryValidation.MustBeEntered);

			GlbExternalPassword.GP_UserID = "";
			GlbExternalPassword.CurrentDecryptedPassword = "";
			GlbExternalPassword.GP_CertificateAuthority = "";
			GlbExternalPassword.TR_Chipset = "";
			GlbExternalPassword.GP_CertificateSerialNumber = "";

			GlbExternalPassword.Validation.ValidateAll();
			AssertNoErrorContaining(GlbExternalPassword.CurrentDecryptedPasswordInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.TR_ChipsetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckGP_CertificateAuthority()
		{
			AssertEquals(5, GlbExternalPassword.Lookups.CertificateAuthorities.Count);

			GlbExternalPassword.GP_UserID = "TEST";
			GlbExternalPassword.GP_CertificateAuthority = "TEST";
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(true, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());

			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EGUVEN;
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(false, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EIMZATR;
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(false, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.ETUGRA;
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(false, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.TUBITAK;
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(false, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.TURKTRUST;
			GlbExternalPassword.Validation.ValidateGP_CertificateAuthority();
			AssertEquals(false, GlbExternalPassword.GP_CertificateAuthorityInfo.HasErrors());
		}

		public void TestValidateTR_Chipset()
		{
			AssertEquals(11, GlbExternalPassword.Lookups.ChipsetList.Count);

			GlbExternalPassword.GP_UserID = "TEST";
			GlbExternalPassword.TR_Chipset = "TEST";
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(true, GlbExternalPassword.TR_ChipsetInfo.HasErrors());

			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.AKIS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.ALAADDIN;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.CHARISMATICS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.EKART;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.KOBIL;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.NETID;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.OBERTHUR;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.SIEMENS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.STARCOS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.WINDOWS;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertEquals(false, GlbExternalPassword.TR_ChipsetInfo.HasErrors());

			GlbExternalPassword.TR_Chipset = "~";
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertHasErrorContaining(GlbExternalPassword.TR_ChipsetInfo, "Enter a valid TR Chip-set.");

			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.KOBIL;
			GlbExternalPassword.Validation.ValidateTR_Chipset();
			AssertNoErrorContaining(GlbExternalPassword.TR_ChipsetInfo, "Enter a valid TR Chip-set.");
		}

		public void TestCheckGP_CertificateSerialNumber_HexValue()
		{
			const string invalidCharacterMessage = "Must contain only valid hexadecimal characters (0-9, A-F).";
			const string oddLengthMessage = "Must contain even number of digits.";

			GlbExternalPassword.GP_UserID = "1";

			GlbExternalPassword.GP_CertificateSerialNumber = "GG";
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, invalidCharacterMessage);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, oddLengthMessage);

			GlbExternalPassword.GP_CertificateSerialNumber = "111";
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, invalidCharacterMessage);
			AssertHasErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, oddLengthMessage);

			GlbExternalPassword.GP_CertificateSerialNumber = "11FF";
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, invalidCharacterMessage);
			AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, oddLengthMessage);
		}
	}
}
