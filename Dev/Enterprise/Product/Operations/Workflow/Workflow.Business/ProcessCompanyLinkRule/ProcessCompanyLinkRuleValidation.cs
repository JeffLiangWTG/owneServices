//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessCompanyLinkRuleValidation
//
//    This class should be used for overriding validation in AutoProcessCompanyLinkRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessCompanyLinkRuleValidation : AutoProcessCompanyLinkRuleValidation
	{
		public ProcessCompanyLinkRuleValidation(AutoProcessCompanyLinkRule parent) : base(parent)
		{
		}

		protected override void CheckPCR_GC_Company()
		{
			base.CheckPCR_GC_Company();

			var factory = Parent.Factory;
			var duplicateRuleQuery = new ZQuery(ProcessCompanyLinkRuleSchema.PCR_Type, Parent.PCR_Type)
				.AddToFilter(ProcessCompanyLinkRuleSchema.PCR_GC_Company, Parent.PCR_GC_Company);

			var count = factory.Load<ProcessCompanyLinkRule>(duplicateRuleQuery).Length;
			if (count > 1)
			{
				Parent.PCR_GC_CompanyInfo.AddWarning(Res.GetString("010ab584-23bf-41f1-9516-df7d3b72fe38", "This is one of {0} rules with the same Type and Company.", count));
			}
		}

		protected override void CheckPCR_Macro()
		{
			base.CheckPCR_Macro();
			MandatoryValidation.CheckEntered(Parent.PCR_MacroInfo);
		}

		protected override void CheckPCR_Type()
		{
			base.CheckPCR_Type();
			MandatoryValidation.CheckEntered(Parent.PCR_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PCR_TypeInfo);
		}
	}
}
