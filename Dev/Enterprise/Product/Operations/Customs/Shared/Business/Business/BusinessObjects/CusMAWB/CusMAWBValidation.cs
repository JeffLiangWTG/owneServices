//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusMAWBValidation
//
//    This class should be used for overriding validation in AutoCusMAWBValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusMAWBValidation : AutoCusMAWBValidation
	{
		public CusMAWBValidation(AutoCusMAWB parent)
			: base(parent)
		{
		}

		public void ValidateCustomsCargoStatusFilter()
		{
			ValidateCalculatedProperty(Parent.CustomsCargoStatusFilterInfo);
		}

		public void ValidateCustomsMessageStatusFilter()
		{
			ValidateCalculatedProperty(Parent.CustomsMessageStatusFilterInfo);
		}

		protected override void CheckCM_GB()
		{
			base.CheckCM_GB();
			MandatoryValidation.CheckEntered(Parent.CM_GBInfo);

			if (!Parent.IsInDatabase)
			{
				var branch = Parent.Branch;
				if (branch != null && branch.GB_GC != GlbCompany.CurrentCompany.PK)
				{
					Parent.CM_GBInfo.AddError(Res.GetString("83502EA5-F8C8-40BD-98CE-01216D478494", "You cannot create a job in a branch that belongs to a different company."));
				}
			}
			else if (Parent.CM_GBInfo.HasChanges)
			{
				var originalBranchValue = (ZGuid)Parent.CM_GBInfo.OriginalValue;
				var originalBranch = originalBranchValue.IsValid ? Parent.Factory.Load<GlbBranch>(originalBranchValue) : null;

				var branch = Parent.Branch;

				if (originalBranch != null && branch != null && originalBranch.GB_GC != branch.GB_GC)
				{
					Parent.CM_GBInfo.AddError(Res.GetString("5907563A-0D16-42A9-A44A-D9EB20FE36B9", "You cannot transfer this job to a branch that belongs to a different company. Please log into the branch and create a job there."));
				}
			}
		}

		#region Implementation

		protected new CusMAWB Parent
		{
			get
			{
				return (CusMAWB)base.Parent;
			}
		}

		protected virtual void CheckCustomsCargoStatusFilter()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CustomsCargoStatusFilterInfo);
		}

		protected virtual void CheckCustomsMessageStatusFilter()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CustomsMessageStatusFilterInfo);
		}

		#endregion // Implementation
	}
}
