using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class LinkedCusAuthorisationRuleCollection : ActiveBusinessObjectCollection<LinkedCusAuthorisationRule>
	{
		public LinkedCusAuthorisationRuleCollection(CusAuthorisationRule master) : base(master.Factory, master, new ZQuery(), CusPermitRuleSchema.CPR_CPR_Rule)
		{
		}

		protected CusAuthorisationRule Master => (CusAuthorisationRule)Relationship.Master;

		protected override void SetDefaultsForNewElementCore(LinkedCusAuthorisationRule newElement)
		{
			newElement.CPR_CPH_PermitHeader = Master.CPR_CPH_PermitHeader;
			newElement.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		}
	}
}
