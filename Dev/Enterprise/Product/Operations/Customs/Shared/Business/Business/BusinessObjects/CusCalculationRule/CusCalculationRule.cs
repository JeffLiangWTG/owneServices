using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("DHL Project")]
	public class CusCalculationRule : AutoCusCalculationRule, ITypeDeciderContext
	{
		public CusCalculationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusCalculationRuleLookups.RuleTypeList))]
		public override ZString CCR_RuleType { get => base.CCR_RuleType; set => base.CCR_RuleType = value; }

		[List(nameof(Lookups) + "." + nameof(CusCalculationRuleLookups.TransportModeList))]
		public override ZString CCR_TransportMode { get => base.CCR_TransportMode; set => base.CCR_TransportMode = value; }

		protected override CusCalculationRuleLookups GetNewLookups()
		{
			return new CusCalculationRuleLookups(this);
		}

		public new CusCalculationRuleLookups Lookups => base.Lookups;

		protected override ZString HumanReadableNameCore => Res.GetString("2062F767-09B5-4EA4-BD35-74CC8FD78641", "Customs Calculation Rule");

		#region Type Decider

		public static readonly CusCalculationRuleTypeDecider TypeDecider = new CusCalculationRuleTypeDecider();

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CCR_RuleType = "INS";
			CCR_TransportMode = TransportTypeList.Codes.Air;
		}

#endif
	}
}
