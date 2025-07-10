using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.Business.Res;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class AccountingIntegrationOptions : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string EnableAccountingIntegration = "EnableAccountingIntegration";
			public const string PreApprovalBillingJob = "PreApprovalBillingJob";

			public const string APPostDSB = "APPostDSB";
			public const string ARPostDSB = "ARPostDSB";

			public const string ChiefCustomsStatusCodes = "ChiefCustomsStatusCodes";
			public const string CDSCustomsStatusCodes = "CDSCustomsStatusCodes";
			public const string EUCustomsStatusCodes = "EUCustomsStatusCodes";
		}

		#region Properties

		public ChargePosterBehaviours Actions
		{
			get
			{
				ChargePosterBehaviours result = ChargePosterBehaviours.None;

				if (EnableAccountingIntegration)
				{
					result |= ChargePosterBehaviours.AutoRateDSB;

					if (ARPostDSB)
					{
						result |= ChargePosterBehaviours.ARPostDSB;
					}

					if (APPostDSB)
					{
						result |= ChargePosterBehaviours.APPostDSB;
					}

					if (PreApprovalBillingJob)
					{
						result |= ChargePosterBehaviours.ARAPPostNonDSB;
					}
				}

				return result;
			}
		}

		public bool PostAR
		{
			get { return ARPostDSB || PreApprovalBillingJob; }
		}

		#region EnableAccountingIntegration

		public ZBool EnableAccountingIntegration
		{
			get { return enableAccountingIntegration; }
			set
			{
				bool hasChanges = EnableAccountingIntegration != value;
				SetNonPersistentPropertyValue(EnableAccountingIntegrationInfo, ref enableAccountingIntegration, value);

				if (hasChanges && !EnableAccountingIntegration)
				{
					PreApprovalBillingJob = false;

					APPostDSB = false;
					ARPostDSB = false;
					cdsCustomsStatusCodes = ZString.Empty;
					ChiefCustomsStatusCodes = ZString.Empty;
					EUCustomsStatusCodes = ZString.Empty;
				}
			}
		}
		ZBool enableAccountingIntegration;

		public ZPropertyInfo EnableAccountingIntegrationInfo
		{
			get { return GetZPropertyInfo(Schema.EnableAccountingIntegration); }
		}

		#endregion

		#region PreApprovalBillingJob

		public ZBool PreApprovalBillingJob
		{
			get { return preApprovalBillingJob; }
			set
			{
				SetNonPersistentPropertyValue(PreApprovalBillingJobInfo, ref preApprovalBillingJob, value);
			}
		}
		ZBool preApprovalBillingJob;

		public ZPropertyInfo PreApprovalBillingJobInfo
		{
			get { return GetZPropertyInfo(Schema.PreApprovalBillingJob); }
		}

		public bool IsPreApprovalBillingJobSupported()
		{
			ZString countryCode = this.CountryCode;

			return countryCode == Core.Constants.CountryCodes.UnitedStates || countryCode == Core.Constants.CountryCodes.Canada;
		}

		#endregion

		#region CustomsStatusCodes

		[MaxLength(137)]
		public ZString ChiefCustomsStatusCodes
		{
			get { return chiefCustomsStatusCodes; }
			set
			{
				SetNonPersistentPropertyValue(ChiefCustomsStatusCodesInfo, ref chiefCustomsStatusCodes, value);
			}
		}
		ZString chiefCustomsStatusCodes;

		public ZPropertyInfo ChiefCustomsStatusCodesInfo
		{
			get { return GetZPropertyInfo(Schema.ChiefCustomsStatusCodes); }
		}

		[MaxLength(50)]
		public ZString CDSCustomsStatusCodes
		{
			get { return cdsCustomsStatusCodes; }
			set
			{
				SetNonPersistentPropertyValue(CDSCustomsStatusCodesInfo, ref cdsCustomsStatusCodes, value);
			}
		}
		ZString cdsCustomsStatusCodes;

		public ZPropertyInfo CDSCustomsStatusCodesInfo
		{
			get { return GetZPropertyInfo(Schema.CDSCustomsStatusCodes); }
		}

		[MaxLength(50)]
		public ZString EUCustomsStatusCodes
		{
			get { return euCustomsStatusCodes; }
			set
			{
				SetNonPersistentPropertyValue(EUCustomsStatusCodesInfo, ref euCustomsStatusCodes, value);
			}
		}
		ZString euCustomsStatusCodes;

		public ZPropertyInfo EUCustomsStatusCodesInfo
		{
			get { return GetZPropertyInfo(Schema.EUCustomsStatusCodes); }
		}

		public bool IsCustomsStatusCodesSupported()
		{
			ZString countryCode = this.CountryCode;

			return countryCode == Core.Constants.CountryCodes.UnitedKingdom;
		}

		public bool IsEUCustomsStatusCodesSupported()
		{
			ZString countryCode = this.CountryCode;
			var supportedCountries = new ZString[] { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Spain };
			return countryCode.In(supportedCountries);
		}

		#endregion

		#region Disbursement Charges

		#region APPostDSB

		public ZBool APPostDSB
		{
			get { return apPostDSB; }
			set
			{
				SetNonPersistentPropertyValue(APPostDSBInfo, ref apPostDSB, value);

				if (!IsValidationSuspended)
				{
					ValidateAPPostDSB();
				}
			}
		}
		ZBool apPostDSB;

		public ZPropertyInfo APPostDSBInfo
		{
			get { return GetZPropertyInfo(Schema.APPostDSB); }
		}

		#endregion

		#region ARPostDSB

		public ZBool ARPostDSB
		{
			get { return arPostDSB; }
			set
			{
				SetNonPersistentPropertyValue(ARPostDSBInfo, ref arPostDSB, value);
			}
		}
		ZBool arPostDSB;

		public ZPropertyInfo ARPostDSBInfo
		{
			get { return GetZPropertyInfo(Schema.ARPostDSB); }
		}

		#endregion

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAPPostDSB();
		}

		public void ValidateAPPostDSB()
		{
			APPostDSBInfo.ClearAllNotifications();

			if (APPostDSB)
			{
				if (CountryCode == Core.Constants.CountryCodes.UnitedStates)
				{
					APPostDSBInfo.AddWarning(APPostingOnEntryClearanceErrorForUS);
				}
			}
		}

		internal static string APPostingOnEntryClearanceErrorForUS
		{
			get { return Res.GetString("3b22f143-5987-409d-97c7-48b3812dbd4b", "For US, it is recommended that you set this to 'No'. When final statements are processed, system will post AP invoices."); }
		}

		#endregion

		#region Implementation

		ZString CountryCode
		{
			get
			{
				if (countryCode.IsEmpty)
				{
					if (companyPK.IsValid && factory != null)
					{
						GlbCompany company = factory.Load<GlbCompany>(companyPK);
						countryCode = company.GC_RN_NKCountryCode;
					}
				}
				return countryCode;
			}
		}
		ZString countryCode;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			AccountingIntegrationOptions result = new AccountingIntegrationOptions();

			if (fallbackLevel != null)
			{
				result.factory = factory;
				result.companyPK = fallbackLevel.CompanyPK(false);
			}

			result.EnableAccountingIntegration = EnableAccountingIntegration;
			result.PreApprovalBillingJob = PreApprovalBillingJob;

			result.APPostDSB = APPostDSB;
			result.ARPostDSB = ARPostDSB;

			return result;
		}

		BusinessObjectFactory factory;
		ZGuid companyPK;

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableAccountingIntegration, EnableAccountingIntegration ? "Y" : "N");
			writer.WriteElementString(Schema.PreApprovalBillingJob, PreApprovalBillingJob ? "Y" : "N");

			writer.WriteElementString(Schema.APPostDSB, APPostDSB ? "Y" : "N");
			writer.WriteElementString(Schema.ARPostDSB, ARPostDSB ? "Y" : "N");

			writer.WriteElementString(Schema.CDSCustomsStatusCodes, CDSCustomsStatusCodes);
			writer.WriteElementString(Schema.ChiefCustomsStatusCodes, ChiefCustomsStatusCodes);

			writer.WriteElementString(Schema.EUCustomsStatusCodes, EUCustomsStatusCodes);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableAccountingIntegration = reader.ReadElementString(Schema.EnableAccountingIntegration) == "Y";
			PreApprovalBillingJob = reader.ReadElementString(Schema.PreApprovalBillingJob) == "Y";

			APPostDSB = reader.ReadElementString(Schema.APPostDSB) == "Y";
			ARPostDSB = reader.ReadElementString(Schema.ARPostDSB) == "Y";

			CDSCustomsStatusCodes = reader.ReadElementString(Schema.CDSCustomsStatusCodes);
			ChiefCustomsStatusCodes = reader.ReadElementString(Schema.ChiefCustomsStatusCodes);

			EUCustomsStatusCodes = reader.ReadElementString(Schema.EUCustomsStatusCodes);
		}

		#endregion
	}
}
