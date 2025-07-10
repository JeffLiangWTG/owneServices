//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleValidation
//
//    This class should be used for overriding validation in AutoAccCommissionRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleValidation : AutoAccCommissionRuleValidation
	{
		public AccCommissionRuleValidation(AutoAccCommissionRule parent)
			: base(parent)
		{
		}

		new AccCommissionRule Parent
		{
			get { return (AccCommissionRule)base.Parent; }
		}

		#region Properties

		protected override void CheckACM_Product()
		{
			base.CheckACM_Product();

			MandatoryValidation.CheckEntered(Parent.ACM_ProductInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACM_ProductInfo);
		}

		protected override void CheckACM_Service()
		{
			base.CheckACM_Service();

			MandatoryValidation.CheckEntered(Parent.ACM_ServiceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACM_ServiceInfo);
		}

		protected override void CheckACM_SubModule()
		{
			base.CheckACM_SubModule();

			MandatoryValidation.CheckEntered(Parent.ACM_SubModuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACM_SubModuleInfo);
		}

		protected override void CheckACM_CommissionBasis()
		{
			base.CheckACM_CommissionBasis();

			MandatoryValidation.CheckEntered(Parent.ACM_CommissionBasisInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACM_CommissionBasisInfo);
		}

		protected override void CheckACM_CommissionTriggerType()
		{
			base.CheckACM_CommissionTriggerType();

			MandatoryValidation.CheckEntered(Parent.ACM_CommissionTriggerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACM_CommissionTriggerTypeInfo);
		}

		protected override void CheckACM_StartDate()
		{
			base.CheckACM_StartDate();
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.ACM_StartDateInfo, Parent.ACM_EndDateInfo);
		}

		protected override void CheckACM_EndDate()
		{
			base.CheckACM_EndDate();
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.ACM_EndDateInfo, Parent.ACM_StartDateInfo);
		}

		protected override void CheckACM_NKOrigin()
		{
			base.CheckACM_NKOrigin();
			ListValidation.ErrorIfInvalidCode(Parent.ACM_NKOriginInfo);
		}

		protected override void CheckACM_NKDestination()
		{
			base.CheckACM_NKDestination();
			ListValidation.ErrorIfInvalidCode(Parent.ACM_NKDestinationInfo);
		}

		#endregion

		#region Row Notifications

		protected void CheckHasAtLeastOneRate()
		{
			string errorMessage = OrgCommissionAgreementRecipientValidation.EnterAtLeastOneRateErrorMessage;

			Parent.RemoveRowError(errorMessage);
			if (Parent.Rates.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckHasAtLeastOneRate();
		}

		#endregion
	}
}
