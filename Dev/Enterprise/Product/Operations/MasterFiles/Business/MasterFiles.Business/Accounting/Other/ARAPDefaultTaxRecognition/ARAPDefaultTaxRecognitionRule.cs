using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARAPDefaultTaxRecognitionRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string LoginCompanyCountryRuleCode = "LoginCompanyCountryRuleCode";
			public const string LoginCompanyCountryRuleDescription = "LoginCompanyCountryRuleDescription";
			public const string OrganizationCountryRuleCode = "OrganizationCountryRuleCode";
			public const string OrganizationCountryRuleDescription = "OrganizationCountryRuleDescription";
			public const string TaxRecognitionCode = "TaxRecognitionCode";

			public const int TaxRecognitionCodeMaxLength = 3;
			public const int LoginCompanyCountryRuleCodeMaxLength = 3;
			public const int OrganizationCountryRuleCodeMaxLength = 3;
		}
		#endregion

		public ARAPDefaultTaxRecognitionRule()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARAPDefaultTaxRecognitionRule();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTaxRecognitionCode();
		}

		#region TaxRecognition

		[MaxLength(Schema.TaxRecognitionCodeMaxLength)]
		[ResourceStringData("54E8B6F4-CCE3-416E-8800-A52CEC07441C", Caption = "Tax Recognition")]
		[List("Lookups.TaxRecognitionCodeList")]
		public ZString TaxRecognitionCode
		{
			get { return taxRecognitionCode; }
			set
			{
				SetNonPersistentPropertyValue(TaxRecognitionCodeInfo, ref taxRecognitionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateTaxRecognitionCode();
				}
			}
		}

		public ZPropertyInfo TaxRecognitionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRecognitionCode); }
		}

		public void ValidateTaxRecognitionCode()
		{
			TaxRecognitionCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxRecognitionCodeInfo, (IMultilingualString)ResString.GetMultilingualString("11772C35-6FE3-4F2B-AE18-97FD945FAD5D", "tax recognition"));
			if (!TaxRecognitionCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxRecognitionCodeInfo, Lookups.TaxRecognitionCodeList);
			}
		}

		ZString taxRecognitionCode;

		#endregion

		#region LoginCompanyCountryRuleCode

		[MaxLength(Schema.LoginCompanyCountryRuleCodeMaxLength)]
		[List("Lookups.LoginCompanyCountryRuleCodeList")]
		public ZString LoginCompanyCountryRuleCode
		{
			get { return loginCompanyCountryRuleCode; }
			set
			{
				SetNonPersistentPropertyValue(LoginCompanyCountryRuleCodeInfo, ref loginCompanyCountryRuleCode, value);
				if (!IsValidationSuspended)
				{
					ValidateLoginCompanyCountryRuleCode();
				}
			}
		}

		public ZPropertyInfo LoginCompanyCountryRuleCodeInfo
		{
			get { return GetZPropertyInfo(Schema.LoginCompanyCountryRuleCode); }
		}

		ZString loginCompanyCountryRuleCode;

		public void ValidateLoginCompanyCountryRuleCode()
		{
			LoginCompanyCountryRuleCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LoginCompanyCountryRuleCodeInfo);
			if (!LoginCompanyCountryRuleCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(LoginCompanyCountryRuleCodeInfo, Lookups.LoginCompanyCountryRuleCodeList);
			}
		}

		#endregion

		#region LoginCompanyCountryRuleDescription

		[BusinessObjectTestExclude]
		public ZString LoginCompanyCountryRuleDescription
		{
			get { return Lookups.LoginCompanyCountryRuleCodeList.GetDescriptionFromCode(LoginCompanyCountryRuleCode); }
		}

		public ZPropertyInfo LoginCompanyCountryRuleDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.LoginCompanyCountryRuleCode, x => LoginCompanyCountryRuleCodeInfo); }
		}

		#endregion

		#region OrganizationCountryRuleCode

		[MaxLength(Schema.OrganizationCountryRuleCodeMaxLength)]
		[List("Lookups.OrganizationCountryRuleCodeList")]
		public ZString OrganizationCountryRuleCode
		{
			get { return organizationCountryRuleCode; }
			set
			{
				SetNonPersistentPropertyValue(OrganizationCountryRuleCodeInfo, ref organizationCountryRuleCode, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganizationCountryRuleCode();
				}
			}
		}

		public ZPropertyInfo OrganizationCountryRuleCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganizationCountryRuleCode); }
		}

		ZString organizationCountryRuleCode;

		public void ValidateOrganizationCountryRuleCode()
		{
			OrganizationCountryRuleCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganizationCountryRuleCodeInfo);
			if (!OrganizationCountryRuleCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(OrganizationCountryRuleCodeInfo, Lookups.OrganizationCountryRuleCodeList);
			}
		}

		#endregion

		#region OrganizationCountryRuleDescription

		[BusinessObjectTestExclude]
		public ZString OrganizationCountryRuleDescription
		{
			get { return Lookups.OrganizationCountryRuleCodeList.GetDescriptionFromCode(OrganizationCountryRuleCode); }
		}

		public ZPropertyInfo OrganizationCountryRuleDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OrganizationCountryRuleDescription, x => OrganizationCountryRuleCodeInfo); }
		}

		#endregion

		#region ARAPDefaultTaxRecognitionLookups

		public ARAPDefaultTaxRecognitionRuleLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		protected ARAPDefaultTaxRecognitionRuleLookups GetNewLookups()
		{
			return new ARAPDefaultTaxRecognitionRuleLookups(this);
		}

		ARAPDefaultTaxRecognitionRuleLookups lookups;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LoginCompanyCountryRuleCode, LoginCompanyCountryRuleCode);
			writer.WriteElementString(Schema.OrganizationCountryRuleCode, OrganizationCountryRuleCode);
			writer.WriteElementString(Schema.TaxRecognitionCode, TaxRecognitionCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LoginCompanyCountryRuleCode = reader.ReadElementString(Schema.LoginCompanyCountryRuleCode);
			OrganizationCountryRuleCode = reader.ReadElementString(Schema.OrganizationCountryRuleCode);
			TaxRecognitionCode = reader.ReadElementString(Schema.TaxRecognitionCode);
		}

		#endregion
	}
}
