//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffEmailAddressValidation
//
//    This class should be used for overriding validation in AutoGlbStaffEmailAddressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEmailAddressValidation : AutoGlbStaffEmailAddressValidation
	{
		public GlbStaffEmailAddressValidation(AutoGlbStaffEmailAddress parent) : base(parent)
		{
		}

		new GlbStaffEmailAddress Parent => (GlbStaffEmailAddress)base.Parent;

		protected override void CheckGSE_Type()
		{
			base.CheckGSE_Type();

			if (Parent != null)
			{
				MandatoryValidation.CheckEntered(Parent.GSE_TypeInfo);

				if (!Parent.IsInDatabase || Parent.GSE_TypeInfo.HasChanges)
				{
					ListValidation.ErrorIfInvalidCode(Parent.GSE_TypeInfo, Parent.Lookups.AllEmailTypeList);
				}

				if (Parent.GSE_GC_Company.IsValid && Parent.GSE_GC_Company != GlbCompany.CurrentCompany.PK && Parent.EmailTypeFromOtherCompanyList.GetDescriptionFromCode(Parent.GSE_Type) == null)
				{
					Parent.GSE_TypeInfo.AddError(Res.GetString("4D3D99F2-56DD-455D-A189-A91B05289BAE", "Company '{0}' does not contain Email Type '{1}'", Parent.Company.CompanyName, Parent.GSE_Type));
				}

				if (Parent.Staff != null)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.EmailTypeInfo, Parent.Staff.EmailAddresses);
				}
			}
		}

		protected override void CheckGSE_EmailAddress()
		{
			base.CheckGSE_EmailAddress();

			if (Parent != null)
			{
				if (Parent.GSE_Type != Core.Constants.EmailFromAddressTypes.Codes.Main)
				{
					MandatoryValidation.CheckEntered(Parent.GSE_EmailAddressInfo);
				}

				if (Parent.Staff != null)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.GSE_EmailAddressInfo, Parent.Staff.EmailAddresses);
					Parent.Staff.Validation.ValidateEmailAddress(Parent.GSE_EmailAddressInfo, Parent.GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main);

					if (IfNeededAddWarningUsedByScheduleTaskRecipients())
					{
						Parent.GSE_EmailAddressInfo.AddWarning(Res.GetString("23E1AA46-3FAD-4E9A-B5A9-7F20F38313EF", "The original email address is used in scheduled task which will be upgraded too."));
					}
				}
			}
		}

		bool IfNeededAddWarningUsedByScheduleTaskRecipients()
		{
			return Parent.IsUsedByScheduleTaskRecipients && (Parent.GSE_EmailAddressInfo.HasChanges || (Parent.GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main && Parent.Staff.GS_EmailAddressInfo.HasChanges));
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return false;
		}
		public void ValidateIsNDR()
		{
			if (Parent != null)
			{
				ValidateCalculatedProperty(Parent.IsNDRInfo);
			}
		}

		protected void CheckIsNDR()
		{
			if (Parent != null && Parent.IsNDR)
			{
				Parent.IsNDRInfo.AddWarning(Res.GetString("1E9191B0-4AA4-47C2-B80A-A2B757533EE4", "A Non-Delivery Receipt has been received at {0}.", Parent.DeliveryReportTimeUtc.ToLocalBranchTime().ToLongTimeString()));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsNDR();
		}
	}
}
