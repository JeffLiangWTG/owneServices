using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TR.Business
{
	[SystemDefinedValues]
	public class GlbExternalPassword_TR : GlbExternalPassword, IxTMessageAttributeProvider
	{
		public GlbExternalPassword_TR(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region overrided properties

		[MaxLength(15)]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		[MaxLength(15)]
		public override ZString CurrentDecryptedPassword { get => base.CurrentDecryptedPassword; set => base.CurrentDecryptedPassword = value; }

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_TR.CertificateAuthorities))]
		public override ZString GP_CertificateAuthority { get => base.GP_CertificateAuthority; set => base.GP_CertificateAuthority = value; }

		#endregion

		#region AddOnColumns TR_Chipset

		public static class GenAddOnColumnFieldName
		{
			public const string TR_Chipset = "TR_Chipset";
		}

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_TR.ChipsetList))]
		[ResourceStringData("Enterprise.MasterFiles.Business.GlbExternalPassword|TR_Chipset", Caption = "TR Chip-set")]
		public ZString TR_Chipset
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnFieldName.TR_Chipset);
			set
			{
				var oldValue = TR_Chipset;
				CheckMaximumLength(TR_ChipsetInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnFieldName.TR_Chipset, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTR_Chipset();
				}
				TR_ChipsetInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TR_ChipsetInfo => GetZPropertyInfo(GenAddOnColumnFieldName.TR_Chipset);

		#endregion

		#region New Properties

		public new GlbExternalPasswordLookups_TR Lookups => (GlbExternalPasswordLookups_TR)base.Lookups;

		public new GlbExternalPasswordValidation_TR Validation => (GlbExternalPasswordValidation_TR)base.Validation;

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.TRK;
		}

		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordLookups_TR(this);
		}

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_TR(this);
		}

		#endregion

		#region CreateCredential

		public override ZString ConfigurationName => TRCustomsSubscribers;
		public const string TRCustomsSubscribers = "TRCustomsSubscribers";

		string MD5Hash(string input)
		{
			StringBuilder hash = new StringBuilder();
			var md5provider = MD5.Create();
			byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

			for (int i = 0; i < bytes.Length; i++)
			{
				hash.Append(bytes[i].ToString("x2", CultureInfo.CurrentCulture));
			}
			return hash.ToString();
		}

		protected override object[] CreateCredentialItems()
		{
			if (!GP_UserID.IsEmpty)
			{
				var sender = CredentialSender.CreateCredential(Constants.CredentialDetails.Current, GP_UserID, MD5Hash(CurrentDecryptedPassword));
				return new object[] { sender };
			}
			return System.Array.Empty<object>();
		}

		protected override ZString GetCredentialStatus()
		{
			return GP_UserID == "" ? base.GetCredentialStatus().ToString() : PasswordStatusList.Codes.Valid;
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			return new ZPropertyInfo[] { GP_UserIDInfo, GP_CurrentPasswordInfo };
		}

		public ZString PINCode { get; set; }

		#endregion

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			return this.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(false);
		}
	}
}
