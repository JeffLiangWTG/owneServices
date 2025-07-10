//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsUNDGLimitValidation
//
//    This class should be used for overriding validation in AutoWhsUNDGLimitValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	
	public class WhsUNDGLimitValidation : AutoWhsUNDGLimitValidation
	{
		public WhsUNDGLimitValidation(AutoWhsUNDGLimit parent) : base(parent)
		{
		}

		protected override void CheckWWD_DG()
		{
			base.CheckWWD_DG();

			var info = Parent.WWD_DGInfo;

			if (!info.HasErrors() && !info.Value.IsEmpty)
			{
				var undgLimit = (WhsUNDGLimit)Parent;
				if (undgLimit.Warehouse.UNDGLimits.Any(u => u.WWD_DG == undgLimit.WWD_DG && u.PK != undgLimit.PK))
				{
					Parent.WWD_DGInfo.AddError(Res.GetString("6b9588f8-26ee-4cd5-ba42-3b16092493fe", "UNDG Substance is duplicated."));
				}
			}

			CheckDGSubstanceAndCountryReferenceAndClassNotAllEmpty(info);
		}

		protected override void CheckWWD_DCR_UNDGCountryReference()
		{
			base.CheckWWD_DCR_UNDGCountryReference();

			var info = Parent.WWD_DCR_UNDGCountryReferenceInfo;
			var undgLimit = (WhsUNDGLimit)Parent;

			if (!info.HasErrors() && !info.Value.IsEmpty)
			{
				if (undgLimit.Warehouse.UNDGLimits.Any(u => u.WWD_DCR_UNDGCountryReference == undgLimit.WWD_DCR_UNDGCountryReference && u.PK != undgLimit.PK))
				{
					Parent.WWD_DCR_UNDGCountryReferenceInfo.AddError(Res.GetString("08193063-086a-483d-9164-2160ae9d2594", "UNDG Country/Region Reference is duplicated."));
				}
			}

			CheckDGSubstanceAndCountryReferenceAndClassNotAllEmpty(info);
		}

		void CheckDGSubstanceAndCountryReferenceAndClassNotAllEmpty(ZPropertyInfo info)
		{
			if (Parent.WWD_DGInfo.Value.IsEmpty && Parent.WWD_DCR_UNDGCountryReferenceInfo.Value.IsEmpty && Parent.WWD_UNDGClassInfo.Value.IsEmpty)
			{
				info.AddError(Res.GetString("d788f389-877d-424d-8271-cfefd75e8586", "You need to enter either DG Substance or UNDG Country/Region Reference or UNDG Class."));
			}
		}

		protected override void CheckWWD_TotalWeightLimit()
		{
			base.CheckWWD_TotalWeightLimit();

			var info = Parent.WWD_TotalWeightLimitInfo;
			if (!info.HasErrors())
			{
				if (Parent.WWD_TotalWeightLimit < 0)
				{
					Parent.WWD_TotalWeightLimitInfo.AddError
						(Res.GetString("c6e1eb77-8eeb-4d9b-9be4-e4733c323fbe", "Cannot enter a negative Total Weight Limit."));
				}
			}
		}

		protected override void CheckWWD_TotalWeightLimitUQ()
		{
			base.CheckWWD_TotalWeightLimitUQ();
			ListValidation.ErrorIfInvalidCode(Parent.WWD_TotalWeightLimitUQInfo);

			if (Parent.WWD_TotalWeightLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WWD_TotalWeightLimitUQInfo);
			}
		}

		protected override void CheckWWD_TotalVolumeLimit()
		{
			base.CheckWWD_TotalVolumeLimit();

			var info = Parent.WWD_TotalVolumeLimitInfo;
			if (!info.HasErrors())
			{
				if (Parent.WWD_TotalVolumeLimit < 0)
				{
					Parent.WWD_TotalVolumeLimitInfo.AddError
						(Res.GetString("644bb209-341e-40e4-9bd2-cbae2cfa56fa", "Cannot enter a negative Total Volume Limit."));
				}
			}
		}

		protected override void CheckWWD_TotalVolumeLimitUQ()
		{
			base.CheckWWD_TotalVolumeLimitUQ();
			ListValidation.ErrorIfInvalidCode(Parent.WWD_TotalVolumeLimitUQInfo);

			if (Parent.WWD_TotalVolumeLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WWD_TotalVolumeLimitUQInfo);
			}
		}

		protected override void CheckWWD_UNDGClass()
		{
			base.CheckWWD_UNDGClass();

			var info = Parent.WWD_UNDGClassInfo;

			if (!info.HasErrors() && !info.Value.IsEmpty)
			{
				var undgLimit = (WhsUNDGLimit)Parent;
				if (undgLimit.Warehouse.UNDGLimits.Any(u => u.WWD_UNDGClass == undgLimit.WWD_UNDGClass && u.PK != undgLimit.PK))
				{
					Parent.WWD_UNDGClassInfo.AddError(Res.GetString("585fccaa-195f-49cd-9cd5-7dd358ca64d5", "UNDG Class is duplicated."));
				}
				if (!new WhsUNDGLimitLookups(Parent).UNDGClass.ContainsCode(undgLimit.WWD_UNDGClass))
				{
					Parent.WWD_UNDGClassInfo.AddError(Res.GetString("32dae29d-5b1e-4f1b-8dae-67b1f20c8559", "UNDG Class is invalid."));
				}
			}

			CheckDGSubstanceAndCountryReferenceAndClassNotAllEmpty(info);
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateWarehouseUNDGLimit();
		}

		public void ValidateWarehouseUNDGLimit()
		{
			var undgLimit = (WhsUNDGLimit)Parent;
			var helperFactory = ObjectFactory.Get<IWhsUNDGLimitValidationHelperFactory>();
			var helper = helperFactory.GetWhsUNDGLimitValidationHelper(undgLimit.Warehouse.WW_WarehouseType);
			if (helper != null)
			{
				var validationMessage = helper.GetWhsUNDGLimitValidationMessage(undgLimit);
				if (!validationMessage.IsEmpty)
				{
					Parent.AddRowWarning(validationMessage);
				}
			}
		}
	}
}
