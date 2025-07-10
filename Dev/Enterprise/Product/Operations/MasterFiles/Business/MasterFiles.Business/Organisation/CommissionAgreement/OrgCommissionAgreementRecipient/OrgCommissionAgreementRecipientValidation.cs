//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementRecipientValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionAgreementRecipientValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientValidation : AutoOrgCommissionAgreementRecipientValidation
	{
		public OrgCommissionAgreementRecipientValidation(AutoOrgCommissionAgreementRecipient parent)
			: base(parent)
		{
		}

		new OrgCommissionAgreementRecipient Parent
		{
			get { return (OrgCommissionAgreementRecipient)base.Parent; }
		}

		#region Properties

		protected override void CheckCAR_GS_NKStaff()
		{
			base.CheckCAR_GS_NKStaff();

			ListValidation.ErrorIfInvalidCode(Parent.CAR_GS_NKStaffInfo);
			if (Parent.CAR_OH_Party.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CAR_GS_NKStaffInfo);
			}
		}

		protected override void CheckCAR_OH_Party()
		{
			base.CheckCAR_OH_Party();

			if (Parent.CAR_GS_NKStaff.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CAR_OH_PartyInfo);
			}
		}

		protected override void CheckCAR_CommissionType()
		{
			base.CheckCAR_CommissionType();

			if (Parent.CAR_Share > 0)
			{
				MandatoryValidation.CheckEntered(Parent.CAR_CommissionTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.CAR_CommissionTypeInfo);
			}
		}

		protected override void CheckCAR_Share()
		{
			base.CheckCAR_Share();

			if (!Parent.CAR_GS_NKStaff.IsEmpty && !Parent.IsEditRatesAllowed && Parent.Rates.Count == 0 && !Parent.HasResponsibleStaffCommissionRule)
			{
				var notificationType = (Parent.CAR_Share > 0) ? NotificationType.Error : NotificationType.Warning;
				Parent.CAR_ShareInfo.AddNotification(notificationType, NoApplicableStaffRuleAndNoOverrideSecuriyPermissionWarningMessage);
			}

			if (Parent.CAR_Share > 0)
			{
				return;
			}

			var commissionsFound = false;
			if (!Parent.CAR_CA0.IsEmpty && !Parent.CommissionAgreement.CA0_CA0_ParentVersion.IsEmpty)
			{
				var recipient = Parent.CommissionAgreement.ParentVersion.Recipients.FirstOrDefault(r => r.CAR_GS_NKStaff == Parent.CAR_GS_NKStaff && r.CAR_CommissionType == Parent.CAR_CommissionType);
				if (recipient != null)
				{
					var ratePks = recipient.Rates.Select(r => r.PK).ToArray();
					var query = new ZQuery(AccCommissionLineSchema.CL0_CAT, ratePks);

					if (Parent.Factory.LoadTop1<IAccCommissionLine>(query) != null)
					{
						commissionsFound = true;
					}
				}
			}

			if (!commissionsFound && !Parent.CAR_ShareInfo.ReadOnly)
			{
				Parent.CAR_ShareInfo.AddNotification(NotificationType.Warning, ShareValueZeroShouldHaveCommissionsMessage);
			}
		}

		#endregion

		#region Row Notifications

		protected void CheckNoDuplicateEntityPerType()
		{
			var agreement = Parent.CommissionAgreement;
			if (agreement != null)
			{
				var hasDuplicate = agreement.Recipients.Any(
					x =>
						x.PK != Parent.PK &&
						x.CAR_CommissionType == Parent.CAR_CommissionType &&
						x.CAR_GS_NKStaff == Parent.CAR_GS_NKStaff &&
						x.CAR_OH_Party == Parent.CAR_OH_Party);

				if (hasDuplicate)
				{
					Parent.AddRowError(Res.GetString("93fc5f95-200e-47f5-9cf4-47ea1315a05e", "The recipient has been duplicated and must be unique per commission type."));
				}
			}
		}

		protected void CheckHasAtLeastOneRate()
		{
			Parent.RemoveRowError(EnterAtLeastOneRateErrorMessage);
			if (Parent.CAR_Share > 0 && Parent.Rates.Count == 0)
			{
				Parent.AddRowError(EnterAtLeastOneRateErrorMessage);
			}
		}

		#endregion

		#region Messages

		static string NoApplicableStaffRuleAndNoOverrideSecuriyPermissionWarningMessage
		{
			get { return Res.GetString("ced95c4d-90a9-4b22-b2db-063c98398037", "This staff can not receive any commission as it does not have any applicable commission rules setup for this agreement. Please add an applicable commission rule for this staff, or ask a person with the appropriate security rights to manually enter this staff's commission rates for this agreement."); }
		}

		public static string EnterAtLeastOneRateErrorMessage
		{
			get { return Res.GetString("85dd814c-1a49-4e5a-9af3-8cfe0f855b45", "Please enter at least one commission rate."); }
		}

		static string ShareValueZeroShouldHaveCommissionsMessage
		{
			get { return Res.GetString("d59ed9e3-9052-4dec-9786-658c9bd8d097", "No commissions have been created for this Wolf Pack member as part of this Commission Agreement therefore there is no benefit in having a zero Share value. Increase the Share value or delete the Wolf Pack member instead."); }
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckNoDuplicateEntityPerType();
			CheckHasAtLeastOneRate();
		}

		#endregion
	}
}
