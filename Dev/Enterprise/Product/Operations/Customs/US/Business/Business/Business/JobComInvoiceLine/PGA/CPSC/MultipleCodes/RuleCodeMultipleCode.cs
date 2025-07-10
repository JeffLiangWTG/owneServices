using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RuleCodeMultipleCode : CommaSeparatedNumber
	{
		public RuleCodeMultipleCode()
		{
			NumberInfo.HumanReadableName = "Rule Code";
		}

		[List(nameof(RuleCodesList))]
		public override ZString Number
		{
			get { return base.Number; }
			set { base.Number = value; }
		}

		public override int NumberMaxLength
		{
			get { return 5; }
		}

		public CPSCRuleCodesList RuleCodesList
		{
			get { return new CPSCRuleCodesList(); }
		}

		public override void ValidateNumber()
		{
			base.ValidateNumber();
			ListValidation.MessageErrorIfInvalidCode(NumberInfo, RuleCodesList);
		}
	}
}
