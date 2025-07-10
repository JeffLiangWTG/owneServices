namespace Enterprise.Rating.Business
{
	using System.Collections.Specialized;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;

	public class RatingEmailDefWithOrganisation : RatingEmailDef
	{
		internal RatingEmailDefWithOrganisation(IAutoRating autoRatingInfo, OrgHeader organisation)
		{
			this.autoRatingInfo = autoRatingInfo;
			this.organisation = organisation;
			factory = organisation.Factory;
			AddRecipientForUserCommunication(GetRatingNotificationRecipients());

			IsActive = Recipients.Count != 0;
		}

		readonly IAutoRating autoRatingInfo;
		readonly BusinessObjectFactory factory;
		readonly OrgHeader organisation;

		StringCollection GetRatingNotificationRecipients()
		{
			var recipientList = new StringCollection();
			if (organisation != null && autoRatingInfo != null)
			{
				GlbStaff representative = null;
				GlbStaff customerService = null;

				switch (autoRatingInfo.FreightMode)
				{
					case FreightMode.AIR:
						representative = autoRatingInfo.IsImport() ? StaffAssignments.ImportAirRepStaff : StaffAssignments.ExportAirRepStaff;
						customerService = autoRatingInfo.IsImport() ? StaffAssignments.ImportAirCustomerServiceRepStaff : StaffAssignments.ExportAirCustomerServiceRepStaff;
						break;

					case FreightMode.LCL:
					case FreightMode.FCL:
					case FreightMode.SEA:
					case FreightMode.GRP:
						representative = autoRatingInfo.IsImport() ? StaffAssignments.ImportSeaRepStaff : StaffAssignments.ExportSeaRepStaff;
						customerService = autoRatingInfo.IsImport() ? StaffAssignments.ImportSeaCustomerServiceRepStaff : StaffAssignments.ExportSeaCustomerServiceRepStaff;
						break;
				}

				if (RatingDataRegistry.Instance.IncludeSalesRepresentative.Value)
				{
					if (representative == null || representative.GS_EmailAddress.IsEmpty)
					{
						representative = StaffAssignments.OverallSalesRepStaff;
					}
					if (representative == null || representative.GS_EmailAddress.IsEmpty)
					{
						representative = DefaultSalesRepresentative;
					}

					if (representative != null && !representative.GS_EmailAddress.IsEmpty)
					{
						recipientList.Add(representative.GS_EmailAddress);
						AddRecipientLanguage(representative.GS_WorkingLanguage);
					}
				}

				if (RatingDataRegistry.Instance.IncludeCustomerService.Value)
				{
					if (customerService == null || customerService.GS_EmailAddress.IsEmpty)
					{
						customerService = StaffAssignments.OverallCustomerServiceRepStaff;
					}
					if (customerService == null || customerService.GS_EmailAddress.IsEmpty)
					{
						customerService = DefaultCustomerService;
					}

					if (customerService != null && !customerService.GS_EmailAddress.IsEmpty && !recipientList.Contains(customerService.GS_EmailAddress))
					{
						recipientList.Add(customerService.GS_EmailAddress);
						AddRecipientLanguage(customerService.GS_WorkingLanguage);
					}
				}
			}

			return recipientList;
		}

		protected override bool SendCore(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				Env.OutgoingMailManager.CreateAndSave(this);
			}
			else
			{
				Env.OutgoingMailManager.Create(factory, this);
			}

			return true;
		}

		OrgStaffAssignmentsCollection StaffAssignments
		{
			get { return staffAssignments ?? (staffAssignments = organisation.GetStaffAssignmentsForGlbCompany(RatingHelper.GetCompany(autoRatingInfo))); }
		}

		OrgStaffAssignmentsCollection staffAssignments;

		GlbStaff DefaultSalesRepresentative
		{
			get { return factory.Load<GlbStaff>(RatingDataRegistry.Instance.DefaultSalesRepresentative.Value); }
		}

		GlbStaff DefaultCustomerService
		{
			get { return factory.Load<GlbStaff>(RatingDataRegistry.Instance.DefaultCustomerService.Value); }
		}
	}
}
