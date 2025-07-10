using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPassword_SGNTP : GlbExternalPassword_SGv4
	{
		public GlbExternalPassword_SGNTP(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.NTP;
		}

		public override ZString ConfigurationName => SGCustomsNTP;

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			var result = new List<ZPropertyInfo>();
			result.Add(GP_CurrentPasswordInfo);
			result.Add(GP_UserIDInfo);
			if (!IsInDatabase || !GP_NextPassword.IsEmpty)
			{
				result.Add(GP_NextPasswordInfo);
			}
			return result.ToArray();
		}

		protected override object[] CreateCredentialItems()
		{
			var result = new List<object>();
			result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.Current, GP_UserID, CurrentDecryptedPassword));
			var nextDecryptedPassword = NextDecryptedPassword;
			if (!nextDecryptedPassword.IsEmpty)
			{
				result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.NextPassword, GP_UserID, nextDecryptedPassword));
			}
			return result.ToArray();
		}

		protected override ZString GetCredentialStatus()
		{
			return GP_PasswordStatus == Core.Constants.PasswordOK ? PasswordStatusList.Codes.Valid : PasswordStatusList.Codes.Invalid;
		}

		public const string SGCustomsNTP = "SGCustomsNTP";
	}
}
