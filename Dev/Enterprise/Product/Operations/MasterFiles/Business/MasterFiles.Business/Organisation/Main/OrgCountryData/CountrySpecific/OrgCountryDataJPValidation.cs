using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataJPValidation : OrgCountryDataValidation
	{
		public OrgCountryDataJPValidation(OrgCountryDataJP parent)
			: base(parent)
		{
		}

		#region Validation

		protected override void CheckOV_EXApprovedOrMajorExporter()
		{
			if (!IsValidationRequired)
			{
				return;
			}

			if (!Parent.OV_OA_ApprovedLocation.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.OV_EXApprovedOrMajorExporterInfo);
			}

			base.CheckOV_EXApprovedOrMajorExporter();
		}

		protected override void CheckOV_EXApprovalExpiryDate()
		{
			if (!IsValidationRequired || !Parent.AddedThroughCollection)
			{
				return;
			}

			AddEditApprovalErrorForUncertifiedUser(Parent.OV_EXApprovalExpiryDateInfo);

			if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(Parent.OV_EXApprovedOrMajorExporter) || Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(Parent.OV_EXApprovedOrMajorExporter))
			{
				if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(Parent.OV_EXApprovedOrMajorExporter))
				{
					if (Parent.OV_EXApprovedOrMajorExporter != AviationSecuritySchemeMembership.Codes.RegulatedAgent)
					{
						MandatoryValidation.CheckEntered(Parent.OV_EXApprovalExpiryDateInfo);
					}
				}

				if (Parent.OV_EXApprovalExpiryDate.IsValid)
				{
					CheckOV_EXApprovalExpiryDateExpiration(2);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.OV_EXApprovalExpiryDateInfo, Res.GetString("849980e0-def1-4dd2-80be-10ae56b1e20b", "Supply Chain Security Expiry Date"));
			}
		}

		public override void CheckApprovalNumberMandatoryValidation()
		{
			if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(Parent.OV_EXApprovedOrMajorExporter) && Parent.OV_EXApprovalNumber.IsEmpty)
			{
				var overriddenErrorMessage = Parent.SupplyChainSecurityConfiguration.ApprovalCodeErrorForOwnAgentApprovalNumberNotEntered(Parent.OV_EXApprovedOrMajorExporter);
				if (!overriddenErrorMessage.IsEmpty && Parent.OrgHeader != null && Parent.OrgHeader.IsProxyOrgOfAnyCompany())
				{
					Parent.OV_EXApprovalNumberInfo.AddError(overriddenErrorMessage);
				}
				else
				{
					if (Parent.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
					{
						MandatoryValidation.CheckEntered(Parent.OV_EXApprovalNumberInfo);
					}
				}
			}
			else if (!Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(Parent.OV_EXApprovedOrMajorExporter))
			{
				MandatoryValidation.CheckNotEntered(Parent.OV_EXApprovalNumberInfo);
			}
		}

		#endregion

		#region Implementation

		new OrgCountryDataJP Parent
		{
			get { return (OrgCountryDataJP)base.Parent; }
		}

		#endregion

	}
}
