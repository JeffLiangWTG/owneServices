using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class TWGlbStaffHelper
	{
		public static CodeDescriptionPairList GetCustomsProfile(this GlbStaff cusAgent)
		{
			string cacheKey = string.Format(System.Globalization.CultureInfo.InvariantCulture, "CustomsProfileList{0}{1}{2}", cusAgent.PK, PasswordTypesList.Codes.TVA, PasswordTypesList.Codes.UVC);
			return cusAgent.Factory.GetCachedValue(cacheKey, () =>
			{
				var glbExternalPasswordsTwCollection = new GlbExternalPasswordCollection(cusAgent);
				glbExternalPasswordsTwCollection.Load();
				var codeDescriptionPairList = new CodeDescriptionPairList();
				foreach (GlbExternalPassword glbExternalPsw in glbExternalPasswordsTwCollection)
				{
					codeDescriptionPairList.AddPair(glbExternalPsw.GP_MailBoxID, glbExternalPsw.GP_PasswordType);
				}

				return codeDescriptionPairList;
			});
		}

		public static GlbExternalPassword GetCredential(this GlbStaff cusAgent, ZString customsProfile)
		{
			var glbExternalPasswordsTwCollection = new GlbExternalPasswordCollection(cusAgent);
			glbExternalPasswordsTwCollection.Load();
			return glbExternalPasswordsTwCollection.OfType<GlbExternalPassword>().FirstOrDefault(c => c.GP_MailBoxID == customsProfile);
		}

		public static ZString GetValidTWBrokerCertificateNumber(this GlbStaff glbStaff, ZDateTime checkDate)
		{
			var certificate = glbStaff.Certificates
				.Find(cert =>
					cert.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Taiwan &&
					cert.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK &&
					(cert.XZ_ExpiryOrDueDate >= checkDate || cert.XZ_ExpiryOrDueDate.IsEmpty)
				)
				.OrderBy(x => x.XZ_IssueDate)
				.FirstOrDefault();

			return certificate?.XZ_RefNumber ?? ZString.Empty;
		}

		public static GenRegCertAccredMaintList GetTWBrkCertificate(this GlbStaff glbStaff)
		{
			return glbStaff.Certificates.Find(cert => cert.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Taiwan
				&& cert.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK)
				.OrderByDescending(x => x.XZ_ExpiryOrDueDate)
				.FirstOrDefault();
		}
	}
}
