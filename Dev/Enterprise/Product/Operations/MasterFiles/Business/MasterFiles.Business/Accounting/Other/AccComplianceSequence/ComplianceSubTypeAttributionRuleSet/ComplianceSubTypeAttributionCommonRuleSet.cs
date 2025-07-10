using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class ComplianceSubTypeAttributionCommonRuleSet : RegistryBusinessObjectTemplate
	{
		protected ComplianceSubTypeAttributionCommonRuleSet()
		{
		}

		protected ComplianceSubTypeAttributionCommonRuleSet(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string ruleSetCode)
			: base(fallbackLevel, factory)
		{
			RuleSetCode = ruleSetCode;
		}

		#region Bround Property

		[List("RuleSetCodeList")]
		public ZString RuleSetCode
		{
			get { return ruleSetCode; }
			set
			{
				SetNonPersistentPropertyValue(RuleSetCodeInfo, ref ruleSetCode, value);
				if (!IsValidationSuspended)
				{
					ValidateRuleSetCode();
				}
			}
		}

		public ZPropertyInfo RuleSetCodeInfo
		{
			get { return GetZPropertyInfo(nameof(RuleSetCode)); }
		}

		public virtual void ValidateRuleSetCode() { }

		ZString ruleSetCode;

		public CodeDescriptionPairList RuleSetCodeList
		{
			get
			{
				if (RuleSetProvider != null)
				{
					return RuleSetProvider.GetRuleSet();
				}

				return new CodeDescriptionPairList();
			}
		}

		protected IComplianceSubTypeRulesWithMultipleRuleSetProvider RuleSetProvider
		{
			get
			{
				if (CurrentFallbackLevel != null)
				{
					var company = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
					if (company != null)
					{
						return CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(company.Country.Code);
					}
				}

				return null;
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRuleSetCode();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(RuleSetCode), RuleSetCode);
		}
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RuleSetCode = reader.ReadElementString(nameof(RuleSetCode));
		}
	}
}
