//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeBranchOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeBranchOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class AccChargeBranchOverrideValidation : AutoAccChargeBranchOverrideValidation
	{
		public AccChargeBranchOverrideValidation(AutoAccChargeBranchOverride parent) : base(parent)
		{
		}

		protected override void CheckYA_JobType()
		{
			base.CheckYA_JobType();

			MandatoryValidation.CheckEntered(Parent.YA_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.YA_JobTypeInfo, Parent.Lookups.JobTypeList);
			if (!Parent.YA_JobTypeInfo.HasErrors())
			{
				if (Parent.ChargeCode != null)
				{
					foreach (var revRecognition in Parent.ChargeCode.BranchOverrides)
					{
						if (revRecognition != Parent)
						{
							if (revRecognition.YA_JobType == Parent.YA_JobType &&
								revRecognition.YA_Direction == Parent.YA_Direction &&
								revRecognition.YA_TransportMode == Parent.YA_TransportMode)
							{
								Parent.YA_JobTypeInfo.AddError(Res.GetString("8DCB3627-2D35-4F61-9FC9-3E88C0420D2B", "At least one more record already sets a behavior for the same Job parameters."));
								break;
							}
						}
					}
				}
			}

			if (!Parent.YA_JobTypeInfo.HasErrors())
			{
				ValidateYA_Direction();
				ValidateYA_TransportMode();
			}
		}

		protected override void CheckYA_Direction()
		{
			base.CheckYA_Direction();

			if (!Parent.YA_DirectionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.YA_DirectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.YA_DirectionInfo, Parent.Lookups.DirectionList);
			}
		}

		protected override void CheckYA_TransportMode()
		{
			base.CheckYA_TransportMode();

			if (!Parent.YA_TransportModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.YA_TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.YA_TransportModeInfo, Parent.Lookups.TransportModeList);

				if (!Parent.YA_TransportModeInfo.HasErrors())
				{
					if ((Parent.YA_JobType == "CSH" || Parent.YA_JobType == "CLL") &&
						(Parent.YA_TransportMode != Core.Constants.TransportModes.Air &&
						Parent.YA_TransportMode != Core.Constants.TransportModes.Sea &&
						Parent.YA_TransportMode != Core.Constants.TransportModes.Road &&
						Parent.YA_TransportMode != Core.Constants.TransportModes.Rail &&
						Parent.YA_TransportMode != RevenueRecognitionLookups.ModeAdditionalCodes.All))
					{
						Parent.YA_TransportModeInfo.AddError(Res.GetString("721CE3AF-916C-4437-9C3C-8481430447DA", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
				}
			}
		}

		protected override void CheckYA_DefaultingRule()
		{
			base.CheckYA_DefaultingRule();

			MandatoryValidation.CheckEntered(Parent.YA_DefaultingRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.YA_DefaultingRuleInfo, Parent.Lookups.DefaultingRuleList);

			if (!Parent.YA_DefaultingRuleInfo.HasErrors())
			{
				Parent.Validation.ValidateYA_GB_SpecificBranch();
			}
		}

		protected override void CheckYA_GB_SpecificBranch()
		{
			base.CheckYA_GB_SpecificBranch();

			if (!Parent.YA_GB_SpecificBranchInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.YA_GB_SpecificBranchInfo);
				ListValidation.ErrorIfInvalidPK(Parent.YA_GB_SpecificBranchInfo, Parent.Lookups.SpecificBranches);
			}
		}
	}
}
