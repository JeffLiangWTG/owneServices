using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.NCTS.Business;

[ModuleID(ModuleId.AuthorisationRule)]
public class CusAuthorisationRuleCollection : ActiveBusinessObjectCollection<CusAuthorisationRule>
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class FilterConstants
	{
		public const string RuleCode = "Rule Code";
		public const string Value = "Value";
		public const string Description = "Description";
		public const string AuthorizationType = "Authorization Type";
		public const string AuthorizationNumber = "Authorization Number";
		public const string AuthorizationHolder = "Authorization Holder";
		public const string StartDate = "Start Date";
		public const string EndDate = "End Date";
		public const string Country = "Country/Region";
	}

	public CusAuthorisationRuleCollection(BusinessObjectFactory factory)
	: base(factory)
	{
	}
}
