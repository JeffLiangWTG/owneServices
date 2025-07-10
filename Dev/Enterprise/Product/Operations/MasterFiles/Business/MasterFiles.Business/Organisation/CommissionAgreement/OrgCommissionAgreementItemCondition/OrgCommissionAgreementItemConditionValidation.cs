//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementItemConditionValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionAgreementItemConditionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemConditionValidation : AutoOrgCommissionAgreementItemConditionValidation
	{
		public OrgCommissionAgreementItemConditionValidation(AutoOrgCommissionAgreementItemCondition parent) : base(parent)
		{
		}

		protected override void CheckCIC_Mode()
		{
			base.CheckCIC_Mode();
			MandatoryValidation.CheckEntered(Parent.CIC_ModeInfo);
			CheckUnique(Parent.CIC_ModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CIC_ModeInfo);
			ValidateMultipleRates();
		}

		protected override void CheckCIC_RL_NKOrigin()
		{
			base.CheckCIC_RL_NKOrigin();
			CheckUnique(Parent.CIC_RL_NKOriginInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CIC_RL_NKOriginInfo);
			ValidateMultipleRates();
		}

		protected override void CheckCIC_RL_NKDestination()
		{
			base.CheckCIC_RL_NKDestination();
			CheckUnique(Parent.CIC_RL_NKDestinationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CIC_RL_NKDestinationInfo);
			ValidateMultipleRates();
		}

		void CheckUnique(ZPropertyInfo propertyInfo)
		{
			var item = Parent.CommissionAgreementItem;
			if (item != null && item.CommissionAgreement != null && item.CommissionAgreement.Opportunity != null && !item.CommissionAgreement.IsReversed)
			{
				var sameProductItems = item.CommissionAgreement.Opportunity.CommissionAgreements.Where(a => !a.IsReversed).SelectMany
					(
						a => a.ProductItems
						.Where(p => p.CAI_Code == item.CAI_Code && ((item.ParentVersion != null && item.ParentVersion.PK != p.PK) || item.ParentVersion == null)
					));

				var conflictingConditions = sameProductItems.SelectMany(p => p.ConditionCollection.Where(
					condition => condition.PK != Parent.PK
					&& condition.CIC_Mode == Parent.CIC_Mode
					&& condition.CIC_RL_NKOrigin == Parent.CIC_RL_NKOrigin
					&& condition.CIC_RL_NKDestination == Parent.CIC_RL_NKDestination
					&& condition.CommissionAgreement?.CA0_CommissionStream == Parent.CommissionAgreementItem?.CommissionAgreement?.CA0_CommissionStream));

				if (conflictingConditions.Any())
				{
					propertyInfo.AddError(Res.GetString("244C1627-495E-4240-BDB6-774E4D276239", "It is not possible to save a duplicate entry for the same Product."));
				}
			}
		}

		void ValidateMultipleRates()
		{
			if (Parent == null || Parent.CommissionAgreementItem == null)
			{
				return;
			}

			Parent.CommissionAgreementItem.Validation.ValidateMultipleRates();
		}
	}
}
