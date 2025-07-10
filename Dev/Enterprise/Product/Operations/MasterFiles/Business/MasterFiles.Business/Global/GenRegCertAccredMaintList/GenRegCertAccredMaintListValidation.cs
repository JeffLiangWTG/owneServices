//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenRegCertAccredMaintListValidation
//
//    This class should be used for overriding validation in AutoGenRegCertAccredMaintListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using ZArchitecture.Schema;

	public class GenRegCertAccredMaintListValidation : AutoGenRegCertAccredMaintListValidation
	{
		public GenRegCertAccredMaintListValidation(AutoGenRegCertAccredMaintList parent)
			: base(parent)
		{
		}

		#region XZ_Type

		protected override void CheckXZ_Type()
		{
			base.CheckXZ_Type();
			MandatoryValidation.CheckEntered(Parent.XZ_TypeInfo);
			CheckTypeCodeIsInList();
			CheckUKStaffHandlingSecuredCargoCertificationRequirements();
		}

		protected virtual void CheckTypeCodeIsInList()
		{
			ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.XZ_TypeInfo, Parent.Lookups.CertificateTypes, Parent.Lookups.CertificateTypes_ActiveList);
		}

		void ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ZPropertyInfo info, ICodeDescriptionPairList allCodes, ICodeDescriptionPairList activeCodes)
		{
			if (!info.BizObj.IsInDatabase || info.HasChanges || !allCodes.ContainsCode(info.Value))
			{
				ListValidation.ErrorIfInvalidCode(info, activeCodes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, activeCodes, ListValidation.InactiveCodeMessage);
			}
		}

		void CheckUKStaffHandlingSecuredCargoCertificationRequirements()
		{
			var parent = Parent as GenRegCertAccredMaintList;
			if (parent != null
				&& parent.IsUKStaffHandlingSecureCargoCertificate)
			{
				var staffRecord = parent.MasterParent as GlbStaff;
				if (staffRecord != null)
				{
					var securityDocument = staffRecord.DocManagerInfo.AllEDocs.GetMostRecentEDoc(requiredDocTypeForUKStaffHandlingSecuredCargo);
					if (securityDocument == null)
					{
						parent.XZ_TypeInfo.AddError(Res.GetString("a8a82748-895e-44e4-9bb2-da37df61429b",
							"Please upload the {0} certificate to eDocs using Document Type \"{1}\" (Staff / Group Security Documents) before saving.",
							parent.XZ_TypeDescription,
							requiredDocTypeForUKStaffHandlingSecuredCargo));
					}
				}
			}
		}

		const string requiredDocTypeForUKStaffHandlingSecuredCargo = "SEC";

		#endregion

		protected override void CheckXZ_Comment()
		{
			base.CheckXZ_Comment();
			if (Parent.XZ_ParentTableCode != GlbStaffSchema.Constants.Prefix && Parent.XZ_ParentTableCode != HRJobApplicantSchema.Constants.Prefix)
			{
				MandatoryValidation.CheckEntered(Parent.XZ_CommentInfo);
			}

			if (Parent.XZ_Comment.IsEmpty)
			{
				switch (Parent.XZ_Type)
				{
					case Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO:
						Parent.XZ_CommentInfo.AddWarning(Res.GetString("5C18A469-C8E2-4890-9A6A-5357B3E06827", "Please enter Chinese name of the operator here if it is different from Full Name of the staff."));
						break;
					case Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK:
						if (Parent.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.China)
						{
							Parent.XZ_CommentInfo.AddWarning(Res.GetString("ECFF4C0E-F213-4C30-8F06-49D4C8017A06", "Please enter Chinese name of the broker here if it is different from Full Name of the staff."));
						}
						break;
				}
			}
		}

		protected override void CheckXZ_ExpiryOrDueDateIsValidZDateTimeRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = 15 };
			TypeValidation.CheckValidZDateTimeRange(Parent.XZ_ExpiryOrDueDateInfo, limits);
			if (!Parent.XZ_IssueDateInfo.Value.IsEmpty)
			{
				//CompareValidation.CheckDateIsAfterAnotherDate(Parent.XZ_ExpiryOrDueDateInfo, Parent.XZ_IssueDateInfo);
				if (Parent.XZ_ExpiryOrDueDate <= Parent.XZ_IssueDate)
				{
					Parent.XZ_ExpiryOrDueDateInfo.AddError(Res.GetString("F3EDD93F-39D2-4F6F-8738-63C296E08073", "The certificate expiry date must be after the issue date."));
				}
			}

			if (Parent.XZ_ExpiryOrDueDate < ZDateTime.Now)
			{
				Parent.XZ_ExpiryOrDueDateInfo.AddWarning(Res.GetString("46826717-E94B-44B2-8C55-907A41163539", "The expiry date entered is in the past."));
			}
		}

		protected override void CheckXZ_RN_NKCountryOfIssuance()
		{
			base.CheckXZ_RN_NKCountryOfIssuance();
			ListValidation.ErrorIfInvalidCode(Parent.XZ_RN_NKCountryOfIssuanceInfo);
			if (Parent.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO && Parent.XZ_RN_NKCountryOfIssuance != Core.Constants.CountryCodes.China)
			{
				Parent.XZ_RN_NKCountryOfIssuanceInfo.AddWarning(Res.GetString("D6CEBADE-4B64-4ACE-8C7C-307EF6E95EC2", "Country/Region should be CN for Type 'CNO'."));
			}
		}

		protected override void CheckXZ_StateOrProvinceOfIssuance()
		{
			base.CheckXZ_StateOrProvinceOfIssuance();
			if (Parent.Lookups.StatesOrProvinces.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.XZ_StateOrProvinceOfIssuanceInfo);
			}
		}

		protected override void CheckXZ_IssueDate()
		{
			base.CheckXZ_IssueDate();
			if (!Parent.XZ_ExpiryOrDueDateInfo.Value.IsEmpty)
			{
				//CompareValidation.CheckDateIsBeforeAnotherDate(Parent.XZ_IssueDateInfo, Parent.XZ_ExpiryOrDueDateInfo);
				if (Parent.XZ_ExpiryOrDueDate <= Parent.XZ_IssueDate)
				{
					Parent.XZ_IssueDateInfo.AddError(Res.GetString("0068E15A-DCC8-4514-A80D-DC968593F901", "The certificate issue date must be before the expiry date."));
				}
			}

			if (Parent.XZ_IssueDate > ZDateTime.Now)
			{
				Parent.XZ_IssueDateInfo.AddWarning(Res.GetString("EFE1BF0E-C60A-4C71-957D-8544F88E1AD7", "The issue date entered is in the future."));
			}
		}

		protected override void CheckXZ_ExpiryOrDueDate()
		{
			base.CheckXZ_ExpiryOrDueDate();
			var parent = Parent as GenRegCertAccredMaintList;
			if (parent != null && parent.IsUKStaffHandlingSecureCargoCertificate)
			{
				MandatoryValidation.CheckEntered(Parent.XZ_ExpiryOrDueDateInfo, Res.GetString("ad75ca88-40f3-4db3-8948-f1dcd57a6948", "Expiry Date"));
			}
		}
	}
}
