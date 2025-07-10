using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class LinkedCusAuthorisationRuleCollection : Customs.Business.LinkedCusAuthorisationRuleCollection
	{
		public LinkedCusAuthorisationRuleCollection(CusAuthorisationRule master) : base(master)
		{
		}

		protected override void SetDefaultsForNewElementCore(LinkedCusAuthorisationRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var parentRuleType = newElement.AuthorisationRule?.CPR_RuleCode ?? ZString.Empty;
			if (parentRuleType == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice)
			{
				newElement.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			}
		}
	}
}
