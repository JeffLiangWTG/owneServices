//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionAgreementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class OrgCommissionAgreementValidation : AutoOrgCommissionAgreementValidation
	{
		public OrgCommissionAgreementValidation(AutoOrgCommissionAgreement parent) : base(parent)
		{
		}

		public OrgCommissionAgreementValidation(AutoOrgCommissionAgreement parent, bool isApproving) : base(parent)
		{
			this.isApproving = isApproving;
		}

		new OrgCommissionAgreement Parent
		{
			get { return (OrgCommissionAgreement)base.Parent; }
		}

		readonly bool isApproving;

		#region Properties

		protected override void CheckCA0_CommissionStream()
		{
			base.CheckCA0_CommissionStream();

			var originalValueBizObj = Parent.IsInDatabase ? Parent : Parent.MainVersion;
			var originalValue = originalValueBizObj.IsInDatabase ? (ZString)originalValueBizObj.CA0_CommissionStreamInfo.OriginalValue : ZString.Empty;
			var newValue = Parent.CA0_CommissionStream;

			if (originalValue != newValue || !Parent.Lookups.CommissionBasisStreams_All.ContainsCode(Parent.CA0_CommissionStream))
			{
				ListValidation.ErrorIfInvalidCode(Parent.CA0_CommissionStreamInfo, Parent.Lookups.CommissionBasisStreams_Active);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.CA0_CommissionStreamInfo, Parent.Lookups.CommissionBasisStreams_Active, ListValidation.InactiveCodeMessage);
			}
		}

		protected override void CheckCA0_CommissionTriggerType()
		{
			base.CheckCA0_CommissionTriggerType();
			MandatoryValidation.CheckEntered(Parent.CA0_CommissionTriggerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CA0_CommissionTriggerTypeInfo);
		}

		protected override void CheckCA0_CommissionBasis()
		{
			base.CheckCA0_CommissionBasis();
			MandatoryValidation.CheckEntered(Parent.CA0_CommissionBasisInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CA0_CommissionBasisInfo);
		}

		protected override void CheckCA0_Name()
		{
			base.CheckCA0_Name();
			MandatoryValidation.CheckEntered(Parent.CA0_NameInfo);

			var opportunity = Parent.Opportunity;
			if (opportunity != null && opportunity.CommissionAgreementsForEditInitialized)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CA0_NameInfo, opportunity.CommissionAgreementsForEdit);
			}
		}

		protected override void CheckCA0_EffectiveDateIsValidZDateRange()
		{
			// Do not check date time range
		}

		#endregion

		#region Row Notifications

		protected void CheckAtLeastOneProductItemIsIncluded()
		{
			OrgCommissionAgreementItemValidation.AddRowErrorIfNoItemIsIncluded(Parent, Parent.ProductItems);
		}

		protected void CheckAllRecipientShareValuesAreNotZero()
		{
			Parent.RemoveRowError(AllShareValueShouldNotBeZeroMessage);

			if (Parent.Recipients.Count > 0 && !Parent.Recipients.Any(r => r.CAR_Share != 0))
			{
				Parent.AddRowError(AllShareValueShouldNotBeZeroMessage);
			}
		}

		protected void CheckForAWolfPackMember()
		{
			Parent.RemoveRowError(WolfPackMemberRequiredMessage);
			Parent.RemoveRowWarning(WolfPackMemberRequiredMessage);

			if (Parent.Recipients.Count == 0)
			{
				if (isApproving)
				{
					Parent.AddRowError(WolfPackMemberRequiredMessage);
				}
				else
				{
					Parent.AddRowWarning(WolfPackMemberRequiredMessage);
				}
			}
		}

		#endregion

		#region Messages

		static string AllShareValueShouldNotBeZeroMessage
		{
			get { return Res.GetString("86cd37cd-1be9-4562-85f4-4c9d7027e2df", @"Cannot set Share for all Wolf Pack members to zero. Select a course of action from the following suggestions:
- To revert to previous Opportunity and Commission Agreement settings, Close the Opportunity without saving.
- To save the current Unapproved Agreement, delete all Wolf Pack members whose shares are zero.
- To nullify an Approved Commission Agreement, set the Agreement to Reversed to reverse all entitlements and/or commissions.
- To cease an Approved Commission Agreement, set the Agreement to Disabled from a specified date to cease commission generation."); }
		}

		public static string WolfPackMemberRequiredMessage
		{
			get
			{
				return Res.GetString("{0A5678D8-3F09-435F-930C-0FC00BBF317D}", @"Cannot approve agreement without a wolf pack member and a rate defined");
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckForAWolfPackMember();
			CheckAtLeastOneProductItemIsIncluded();
			CheckAllRecipientShareValuesAreNotZero();
		}

		#endregion

		#region ValidateWolfPackMemberExists

		public void ValidateWolfPackMemberExists()
		{
			CheckForAWolfPackMember();
		}

		#endregion
	}
}
