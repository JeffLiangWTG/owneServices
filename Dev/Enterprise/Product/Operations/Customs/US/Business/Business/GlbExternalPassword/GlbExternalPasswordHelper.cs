using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class GlbExternalPasswordHelper
	{
		internal static string UserNameMustBeUnique
		{
			get { return Res.GetString("A05C4124-96BD-4E2D-A0E7-96A6EFB16EF0", "User Name must be unique."); }
		}

		public static CodeDescriptionPairList GetInsuranceAgents(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GlbExternalPasswordLookups_US.InsuranceAgents", () =>
			{
				var result = new CodeDescriptionPairList();
				var agents = ZZRefCusCodeListCombined.Loader.Load(factory, Enterprise.Core.Constants.CountryCodes.UnitedStates,
					Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent,
					ZDateTime.Now);

				result.AddRange(agents);
				result.Sort();

				return result;
			});
		}

		public static void CheckGP_MailBoxID(GlbExternalPassword parent)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.GP_MailBoxIDInfo);
		}

		public static void CheckGP_UserID(GlbExternalPassword parent)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.GP_UserIDInfo);
			if (!parent.GP_UserID.IsEmpty && parent.GP_UserID.Length < 3)
			{
				parent.GP_UserIDInfo.AddMessageError(Res.GetString("997F1C63-AAB1-4075-987C-A687F6887D67", "User Name must be at least 3 characters."));
			}
		}

		public static void CheckCurrentDecryptedCertificatePassphrase(GlbExternalPassword parent)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.CurrentDecryptedCertificatePassphraseInfo);
			if (!parent.CurrentDecryptedCertificatePassphrase.IsEmpty && parent.CurrentDecryptedCertificatePassphrase.Length <= 3)
			{
				parent.CurrentDecryptedCertificatePassphraseInfo.AddMessageError(Res.GetString("3F401CE1-2EAB-45E9-BC60-9B0C4D6180CE", "Password must greater than 3 characters."));
			}
		}
	}
}
