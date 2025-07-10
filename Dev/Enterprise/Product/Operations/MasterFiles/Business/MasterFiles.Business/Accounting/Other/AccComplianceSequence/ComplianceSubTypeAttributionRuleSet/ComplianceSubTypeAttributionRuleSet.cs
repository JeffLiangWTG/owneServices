using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAttributionRuleSet : ComplianceSubTypeAttributionCommonRuleSet
	{
		public ComplianceSubTypeAttributionRuleSet()
		{
		}

		public ComplianceSubTypeAttributionRuleSet(FallbackLevel fallbackLevel, BusinessObjectFactory factory, string ruleSetCode)
			: base(fallbackLevel, factory, ruleSetCode)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CurrentFallbackLevel = fallbackLevel;
			return new ComplianceSubTypeAttributionRuleSet(fallbackLevel, factory, RuleSetCode);
		}

		public override void ValidateRuleSetCode()
		{
			RuleSetCodeInfo.ClearAllNotifications();

			if (RuleSetProvider != null)
			{
				MandatoryValidation.CheckEntered(RuleSetCodeInfo, (IMultilingualString)ResString.GetMultilingualString("077FE1CA-EAF0-470F-A19F-AC2864AD5B46", "Rule Set Code"));
			}

			if (!RuleSetCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(RuleSetCodeInfo, RuleSetCodeList);
			}
		}
	}
}
