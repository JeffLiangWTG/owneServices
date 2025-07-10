using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityValidation : AutoOrgOpportunityValidation
	{
		public OrgOpportunityValidation(AutoOrgOpportunity parent) : base(parent)
		{
		}

		public new OrgOpportunity Parent
		{
			get { return (OrgOpportunity)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (!Parent.IsValidationSuspended)
			{
				base.ValidateAll();
				ValidateAssignedOrgPK();
				ValidateSourceDetails();
			}
		}

		#region P8_OpportunityDescription

		protected override void CheckP8_OpportunityDescription()
		{
			base.CheckP8_OpportunityDescription();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_OpportunityDescription))
			{
				MandatoryValidation.CheckEntered(Parent.P8_OpportunityDescriptionInfo);
			}
		}

		#endregion

		#region P8_GS_NKPrimarySalesPerson

		protected override void CheckP8_GS_NKPrimarySalesPerson()
		{
			base.CheckP8_GS_NKPrimarySalesPerson();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson))
			{
				if (Parent.IsInDatabase && !Parent.P8_GS_NKPrimarySalesPersonInfo.HasChanges)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.P8_GS_NKPrimarySalesPersonInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(Parent.P8_GS_NKPrimarySalesPersonInfo);
				}
			}

			ListValidation.ErrorIfInvalidCode(Parent.P8_GS_NKPrimarySalesPersonInfo);
		}

		#endregion

		#region P8_OpportunityType

		protected override void CheckP8_OpportunityType()
		{
			base.CheckP8_OpportunityType();
			MandatoryValidation.CheckEntered(Parent.P8_OpportunityTypeInfo);
			if (!Parent.IsInDatabase || Parent.P8_OpportunityTypeInfo.HasChanges || !Parent.Lookups.Types.ContainsCode(Parent.P8_OpportunityType))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_OpportunityTypeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_OpportunityTypeInfo, Parent.Lookups.ActiveTypes, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_Outcome

		protected override void CheckP8_Outcome()
		{
			base.CheckP8_Outcome();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_Outcome))
			{
				MandatoryValidation.CheckEntered(Parent.P8_OutcomeInfo);
			}

			if (!Parent.IsInDatabase || Parent.P8_OutcomeInfo.HasChanges || !Parent.Lookups.Outcomes.ContainsCode(Parent.P8_Outcome))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_OutcomeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_OutcomeInfo, Parent.Lookups.ActiveOutcomes, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_Source

		protected override void CheckP8_Source()
		{
			base.CheckP8_Source();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_Source))
			{
				MandatoryValidation.CheckEntered(Parent.P8_SourceInfo);
			}

			if (!Parent.IsInDatabase || Parent.P8_SourceInfo.HasChanges || !Parent.Lookups.Sources.ContainsCode(Parent.P8_Source))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_SourceInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_SourceInfo, Parent.Lookups.ActiveSources, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_Stage

		protected override void CheckP8_Stage()
		{
			base.CheckP8_Stage();
			MandatoryValidation.CheckEntered(Parent.P8_StageInfo);
			if (!Parent.IsInDatabase || Parent.P8_StageInfo.HasChanges || !Parent.Lookups.Stages.ContainsCode(Parent.P8_Stage))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_StageInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_StageInfo, Parent.Lookups.ActiveStages, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_Status

		protected override void CheckP8_Status()
		{
			base.CheckP8_Status();
			MandatoryValidation.CheckEntered(Parent.P8_StatusInfo);

			if (!Parent.IsInDatabase || Parent.P8_StatusInfo.HasChanges || !Parent.Lookups.Statuses.ContainsCode(Parent.P8_Status))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_StatusInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_StatusInfo, Parent.Lookups.ActiveStatuses, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_ClosedDate

		protected override void CheckP8_ClosedDate()
		{
			base.CheckP8_ClosedDate();
			if (Parent.IsClosed)
			{
				MandatoryValidation.CheckEntered(Parent.P8_ClosedDateInfo);
			}
		}

		#endregion

		#region P8_RX_NKEstimatedValueCurrency

		protected override void CheckP8_RX_NKEstimatedValueCurrency()
		{
			base.CheckP8_RX_NKEstimatedValueCurrency();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_RX_NKEstimatedValueCurrency)
				&& !Parent.P8_EstimatedValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.P8_RX_NKEstimatedValueCurrencyInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.P8_RX_NKEstimatedValueCurrencyInfo);
		}

		#endregion

		#region P8_EstimatedValue

		protected override void CheckP8_EstimatedValue()
		{
			base.CheckP8_EstimatedValue();
			if (Parent.P8_EstimatedValue > 100000000000000m)
			{
				Parent.P8_EstimatedValueInfo.AddError(Res.GetString("7bd191a8-992b-4a0c-88a1-3152c1bd0b39", "The value you have entered here is too large. Please enter a value less than 100,000,000,000,000."));
			}
		}

		#endregion

		#region Close Certainty

		protected override void CheckP8_CloseCertainty()
		{
			base.CheckP8_CloseCertainty();
			if (Parent.P8_CloseCertainty < 0 || Parent.P8_CloseCertainty > 100)
			{
				Parent.P8_CloseCertaintyInfo.AddError(Res.GetString("cb52128b-dcc6-473e-93db-f009c036ead2", "Invalid percentage for close certainty. Must be between 0 and 100."));
			}
		}
		#endregion

		#region AssignedOrgPK

		public void ValidateAssignedOrgPK()
		{
			ValidateCalculatedProperty(Parent.AssignedOrgPKInfo);
		}

		protected void CheckAssignedOrgPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AssignedOrgPKInfo);
		}

		#endregion

		#region P8_PackageType

		protected override void CheckP8_PackageType()
		{
			base.CheckP8_PackageType();

			if (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value.GetBoolFromCode(OrgOpportunitySchema.Constants.P8_PackageType))
			{
				MandatoryValidation.CheckEntered(Parent.P8_PackageTypeInfo);
			}

			if (!Parent.IsInDatabase || Parent.P8_PackageTypeInfo.HasChanges || !Parent.Lookups.ExtraCategories.ContainsCode(Parent.P8_PackageType))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_PackageTypeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_PackageTypeInfo, Parent.Lookups.ActiveExtraCategories, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region P8_LostReason

		protected override void CheckP8_LostReason()
		{
			base.CheckP8_LostReason();
			if (!Parent.IsInDatabase || Parent.P8_LostReasonInfo.HasChanges || !Parent.Lookups.CloseReasons.ContainsCode(Parent.P8_LostReason))
			{
				ListValidation.ErrorIfInvalidCode(Parent.P8_LostReasonInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.P8_LostReasonInfo, Parent.Lookups.ActiveCloseReasons, ListValidation.InactiveCodeMessage);
				ListValidation.WarnIfInvalidCode(Parent.P8_LostReasonInfo, Parent.Lookups.ActiveCloseReasonsByStatus, ResString.GetMultilingualString("4cfe7c33-cd24-4eff-8bab-cf858949794b", "The entered reason is not associated to the Opportunity Status.") as IMultilingualString);
			}
		}

		#endregion

		#region SourceDetails

		public void ValidateSourceDetails()
		{
			((IValidationInternals)this).Validate(Parent.SourceDetailsInfo, () => { CheckSourceDetails(); });
		}

		protected void CheckSourceDetails()
		{
			base.CheckP8_SourceDetails();

			if (Parent.Lookups.SourceDetails.Count > 0)
			{
				if (!Parent.IsInDatabase || Parent.SourceDetailsInfo.HasChanges || !Parent.Lookups.SourceDetails.ContainsCode(Parent.P8_SourceDetails))
				{
					ListValidation.ErrorIfInvalidCode(Parent.SourceDetailsInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.SourceDetailsInfo, Parent.Lookups.ActiveSourceDetails, ListValidation.InactiveCodeMessage);
				}
			}
		}

		#endregion

		#region P8_OC

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			// Validate a related contact by loading the contact individually rather than have the architecture loading the entire lookup collection (potentially thousands of contacts).
			if (info == Parent.P8_OCInfo || info == Parent.P8_OC_ReferringContactInfo)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		protected override void CheckP8_OC()
		{
			var val = Parent.P8_OC;
			var info = Parent.P8_OCInfo;
			if (!info.Value.IsEmpty && info.Value.IsValid && !info.ReadOnly)
			{
				var contact = Parent.Contact;
				if (contact != null)
				{
					ListValidation.ErrorIfCancelled(info, contact.IsCancelled);
				}
			}
		}

		#endregion

		#region Referring Contact

		protected override void CheckP8_OC_ReferringContact()
		{
			base.CheckP8_OC_ReferringContact();

			var info = Parent.P8_OC_ReferringContactInfo;
			if (Parent.ReferringOrganisation != null)
			{
				var contact = Parent.ReferringContact;
				if (!info.Value.IsEmpty && (contact == null || contact.OC_OH != Parent.P8_OH_ReferringOrganisation || !contact.OC_IsActive))
				{
					var message = Res.GetString("98D9F390-E357-4C30-A436-9B339EF222F6", "Select an active contact for Referring Contact.");

					if (Parent.IsInDatabase && !info.HasChanges)
					{
						info.AddWarning(message);
					}
					else
					{
						info.AddError(message);
					}
				}
			}
			else
			{
				if (Parent.ReferringContact != null)
				{
					info.AddError(Res.GetString("5BF4F3E6-B368-47D8-B414-C7260D6F3949", "Select the Referring Organization before selecting the Referring Contact."));
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(info);
				}
			}
		}

		#endregion

		#region P8_OA_AssignedOffice

		protected override void CheckP8_OA_AssignedOffice()
		{
			base.CheckP8_OA_AssignedOffice();
			if (!Parent.AssignedOrgPK.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.P8_OA_AssignedOfficeInfo);
			}
		}

		#endregion

		#region P8_DiscountAmount

		protected override void CheckP8_DiscountAmount()
		{
			base.CheckP8_DiscountAmount();
			CompareValidation.CheckLessThanOrEqualTo(Parent.P8_DiscountAmountInfo, MaxDiscountAmount);
		}

		#endregion

		#region P8_RentalMultiplier

		protected override void CheckP8_RentalMultiplier()
		{
			base.CheckP8_RentalMultiplier();
			MandatoryValidation.CheckNotNegative(Parent.P8_RentalMultiplierInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.P8_RentalMultiplierInfo, MaxRentalMultiplier);
		}

		#endregion

		const decimal MaxDiscountAmount = 999999999999m;
		const decimal MaxRentalMultiplier = 999999999999m;
	}
}
