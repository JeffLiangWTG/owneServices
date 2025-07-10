using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallValidation : AutoOrgSalesCallValidation
	{
		public OrgSalesCallValidation(AutoOrgSalesCall parent)
			: base(parent)
		{
		}

		new OrgSalesCall Parent
		{
			get { return (OrgSalesCall)base.Parent; }
		}

		protected override void CheckOQ_OH()
		{
			base.CheckOQ_OH();

			if (!Parent.IsLinkedToInquiry)
			{
				MandatoryValidation.CheckEntered(Parent.OQ_OHInfo);
			}
		}

		protected override void CheckOQ_OHIsValidZGuid()
		{
			if (Parent.OQ_OH.IsMissing)
			{
				Parent.OQ_OHInfo.AddError(MustBeSalesOrganizationErrorMessage);
			}
			else
			{
				base.CheckOQ_OHIsValidZGuid();
			}
		}

		string MustBeSalesOrganizationErrorMessage
		{
			get { return ResString.GetMultilingualString("05441f90-e2cd-4a15-927e-390c83a392fc", "Must be a Sales Organization"); }
		}

		protected override void CheckOQ_TypeOfCall()
		{
			base.CheckOQ_TypeOfCall();

			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrgSalesCallSchema.Constants.OQ_TypeOfCall))
			{
				MandatoryValidation.CheckEntered(Parent.OQ_TypeOfCallInfo);
			}

			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.OQ_TypeOfCallInfo, Parent.Lookups.OQ_TypeOfCall_List, Parent.Lookups.OQ_TypeOfCall_ActiveList);
		}

		protected override void CheckOQ_CallSummary()
		{
			base.CheckOQ_CallSummary();
			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrgSalesCallSchema.Constants.OQ_CallSummary))
			{
				MandatoryValidation.CheckEntered(Parent.OQ_CallSummaryInfo);
			}
		}

		protected override void CheckOQ_Status()
		{
			base.CheckOQ_Status();

			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrgSalesCallSchema.Constants.OQ_Status))
			{
				MandatoryValidation.CheckEntered(Parent.OQ_StatusInfo);
			}

			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrganisationsDataRegistry.CommunicationMandatoryFieldsClosedStatusRequiredCode))
			{
				if (!Parent.OQ_CallDate.IsEmpty && !Parent.IsClosed)
				{
					Parent.OQ_StatusInfo.AddError(Res.GetString("5BD06797-B734-4257-A848-118F4FC9C9C7", "Please enter a closed status."));
				}
			}

			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.OQ_StatusInfo, Parent.Lookups.OQ_Status_List, Parent.Lookups.OQ_Status_ActiveList);
		}

		protected override void CheckOQ_GS_NKSalesRep()
		{
			base.CheckOQ_GS_NKSalesRep();
			ListValidation.ErrorIfInvalidCode(Parent.OQ_GS_NKSalesRepInfo);
		}

		protected override void CheckOQ_CallDate()
		{
			base.CheckOQ_CallDate();
			if (Parent.OQ_NextCall.IsEmpty && Parent.OQ_CallDate.IsEmpty)
			{
				Parent.OQ_CallDateInfo.AddError(Res.GetString("edcd423b-152f-48bd-8d35-f0c99a8381ed", "Please enter a scheduled date or actual date."));
			}
		}

		protected override void CheckOQ_CallDateIsValidZDateTimeRange()
		{
			if ((!Parent.IsInDatabase || Parent.OQ_CallDateInfo.HasChanges) && Parent.OQ_CallDate.IsValid && Parent.OQ_CallDate > ZDateTime.UtcNow.AddDays(1))
			{
				Parent.OQ_CallDateInfo.AddError(Res.GetString("f0658be1-b457-4fd6-8741-ddec2fc38cdc", "Actual date cannot be in the future by more than 24 hours."));
			}
			else
			{
				base.CheckOQ_CallDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckOQ_NextCall()
		{
			base.CheckOQ_NextCall();
			if (Parent.OQ_NextCall.IsEmpty && Parent.OQ_CallDate.IsEmpty)
			{
				Parent.OQ_NextCallInfo.AddError(Res.GetString("edcd423b-152f-48bd-8d35-f0c99a8381ed", "Please enter a scheduled date or actual date."));
			}
		}

		protected override void CheckOQ_IsReminderClientFacing()
		{
			base.CheckOQ_IsReminderClientFacing();
			if (Parent.HasRiskOfSendingInternalNotesToAttendees)
			{
				string warning = Res.GetString("95BCBF45-BBFA-4903-8065-26B38B0CFC7A", @"Note:  Additional attendees flagged to receive reminders, will receive the internal version invitation if you proceed to save.
If you continue, all amendments will result in attendees receiving the internal version of reminders, including automatic cancellations.  To avoid the internal version being delivered, untick all attendees first, save and then proceed to untick the client visible invitation.");
				Parent.OQ_IsReminderClientFacingInfo.AddWarning(warning);
			}
		}

		protected override void CheckOQ_Category()
		{
			base.CheckOQ_Category();

			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrgSalesCallSchema.Constants.OQ_Category))
			{
				MandatoryValidation.CheckEntered(Parent.OQ_CategoryInfo);
			}

			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.OQ_CategoryInfo, Parent.Lookups.OQ_Category_List, Parent.Lookups.OQ_Category_ActiveList);
		}
		public void ValidateShouldSendInvitation()
		{
			ValidateCalculatedProperty(Parent.ShouldSendInvitationInfo);
		}

		protected void CheckShouldSendInvitation()
		{
			GlbStaff salesRep = Parent.SalesRep;
			if ((salesRep == null || salesRep.GS_EmailAddress.IsEmpty) && Parent.ShouldSendInvitation)
			{
				Parent.ShouldSendInvitationInfo.AddError(MustHaveAnEmailAddressErrorMessage);
			}
		}

		string MustHaveAnEmailAddressErrorMessage
		{
			get { return ResString.GetMultilingualString("8ec3dc5a-cd6b-4cbd-b3e0-48d406e4d235", "Recipient email address is empty"); }
		}

		public void ValidateLocation()
		{
			ValidateCalculatedProperty(Parent.LocationInfo);
		}
		protected void CheckLocation()
		{
			if (!Parent.Lookups.LocationList.ContainsCode(Parent.Location)) //free type text
			{
				return;
			}
			ListValidation.WarnIfInvalidCode(Parent.LocationInfo, Parent.Lookups.ActiveLocationList, ListValidation.InactiveCodeMessage);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateLocation();
			}
		}
	}
}
