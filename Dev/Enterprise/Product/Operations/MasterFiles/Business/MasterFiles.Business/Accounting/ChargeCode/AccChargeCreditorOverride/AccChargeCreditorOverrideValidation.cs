using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCreditorOverrideValidation : AutoAccChargeCreditorOverrideValidation
	{
		public AccChargeCreditorOverrideValidation(AutoAccChargeCreditorOverride parent) : base(parent)
		{
		}

		void CheckIsUnique(ZPropertyInfo propertyInfo)
		{
			if (Parent.ChargeCode != null)
			{
				foreach (var revRecognition in Parent.ChargeCode.CreditorOverrides)
				{
					if (revRecognition != Parent)
					{
						if (revRecognition.ACC_JobType == Parent.ACC_JobType &&
							revRecognition.ACC_Direction == Parent.ACC_Direction &&
							revRecognition.ACC_TransportMode == Parent.ACC_TransportMode &&
							revRecognition.ACC_DefaultingRule == Parent.ACC_DefaultingRule &&
							revRecognition.ACC_PaymentTerm == Parent.ACC_PaymentTerm &&
							revRecognition.ACC_GE_Department == Parent.ACC_GE_Department)
						{
							Parent.FindPropertyInfo(propertyInfo.Name).AddError(Res.GetString("8DCB3627-2D35-4F61-9FC9-3E88C0420D2B", "At least one more record already sets a behavior for the same Job parameters."));
							break;
						}
					}
				}
			}
		}

		protected override void CheckACC_JobType()
		{
			base.CheckACC_JobType();

			MandatoryValidation.CheckEntered(Parent.ACC_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACC_JobTypeInfo, Parent.Lookups.JobTypeList);
			if (!Parent.ACC_JobTypeInfo.HasErrors())
			{
				CheckIsUnique(Parent.ACC_JobTypeInfo);
			}

			if (!Parent.ACC_JobTypeInfo.HasErrors())
			{
				ValidateACC_Direction();
				ValidateACC_TransportMode();
			}
		}

		protected override void CheckACC_Direction()
		{
			base.CheckACC_Direction();

			if (!Parent.ACC_DirectionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ACC_DirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ACC_DirectionInfo, Parent.Lookups.DirectionList);

				if (!Parent.ACC_DirectionInfo.HasErrors())
				{
					CheckIsUnique(Parent.ACC_DirectionInfo);
				}
			}
		}

		protected override void CheckACC_TransportMode()
		{
			base.CheckACC_TransportMode();

			if (!Parent.ACC_TransportModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ACC_TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ACC_TransportModeInfo, Parent.Lookups.TransportModeList);

				if (!Parent.ACC_TransportModeInfo.HasErrors())
				{
					if ((Parent.ACC_JobType == "CSH" || Parent.ACC_JobType == "CLL") &&
						(Parent.ACC_TransportMode != Core.Constants.TransportModes.Air &&
						Parent.ACC_TransportMode != Core.Constants.TransportModes.Sea &&
						Parent.ACC_TransportMode != Core.Constants.TransportModes.Road &&
						Parent.ACC_TransportMode != Core.Constants.TransportModes.Rail &&
						Parent.ACC_TransportMode != RevenueRecognitionLookups.ModeAdditionalCodes.All))
					{
						Parent.ACC_TransportModeInfo.AddError(Res.GetString("721CE3AF-916C-4437-9C3C-8481430447DA", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
				}

				if (!Parent.ACC_TransportModeInfo.HasErrors())
				{
					CheckIsUnique(Parent.ACC_TransportModeInfo);
				}
			}
		}

		protected override void CheckACC_GE_Department()
		{
			base.CheckACC_GE_Department();
			if (!Parent.ACC_GE_DepartmentInfo.ReadOnly)
			{
				if (!Parent.ACC_GE_DepartmentInfo.HasErrors())
				{
					CheckIsUnique(Parent.ACC_GE_DepartmentInfo);
				}
			}
		}

		protected override void CheckACC_DefaultingRule()
		{
			base.CheckACC_DefaultingRule();

			MandatoryValidation.CheckEntered(Parent.ACC_DefaultingRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACC_DefaultingRuleInfo, Parent.Lookups.DefaultingRuleList);
		}

		protected override void CheckACC_OH_Creditor()
		{
			base.CheckACC_OH_Creditor();

			CheckCreditorAndCreditorRole(Parent.ACC_OH_CreditorInfo);

			var parent = Parent as AccChargeCreditorOverride;
			if (parent?.CreditorOrganisation == null)
			{
				return;
			}

			if (!Parent.ChargeCode.AC_GC.IsEmpty)
			{
				var orgCompanyData = parent.CreditorOrganisation.GetCompanyDataForGlbCompany(parent.ChargeCode.Company);
				if (!orgCompanyData.OB_IsCreditor)
				{
					Parent.ACC_OH_CreditorInfo.AddError(Res.GetString("116707aa-189d-44db-8b1b-7db589eca2bd", "Organization should be Payable."));
				}
			}
		}

		void CheckCreditorAndCreditorRole(ZPropertyInfo propertyInfo)
		{
			if (!Parent.ACC_OH_Creditor.IsEmpty && !Parent.ACC_CreditorRole.IsEmpty)
			{
				propertyInfo.AddError(Res.GetString("1514e4e7-1850-4c29-a3c3-dc9918b37321", "Specific Creditor and Creditor Role cannot be configured at the same time."));
			}

			if (Parent.ACC_OH_Creditor.IsEmpty && Parent.ACC_CreditorRole.IsEmpty)
			{
				propertyInfo.AddError(Res.GetString("24A9C7B2-7309-4C71-BDDD-2116DD43273C", "Please configure at least one of the Specific Creditor or Creditor Role."));
			}
		}

		protected override void CheckACC_CreditorRole()
		{
			base.CheckACC_CreditorRole();

			if (!Parent.ACC_CreditorRoleInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ACC_CreditorRoleInfo, Parent.Lookups.CreditorRoleList);

				CheckCreditorAndCreditorRole(Parent.ACC_CreditorRoleInfo);
			}
		}

		protected override void CheckACC_PaymentTerm()
		{
			base.CheckACC_PaymentTerm();
			if (!Parent.ACC_PaymentTermInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ACC_PaymentTermInfo, Parent.Lookups.PaymentTermList);

				if (!Parent.ACC_PaymentTermInfo.HasErrors())
				{
					CheckIsUnique(Parent.ACC_PaymentTermInfo);
				}
			}
		}
	}
}
