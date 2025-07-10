using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobMawbController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.JobMawb; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobMawb; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobMawb); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<RangeJobMawb>();
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			return sourceEntity.IsInDatabaseIncludingChildren ? base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) : Factory.Load<RangeJobMawb>(sourceEntity.Identifier);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ZForm result = null;
			if (businessEntity.IsInDatabaseIncludingChildren)
			{
				result = new JobMawbForm((JobMawb)businessEntity);
			}
			else
			{
				result = new AddJobMawbRangeForm((RangeJobMawb)businessEntity);
			}
			return result;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var businessEntity = GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			if (businessEntity is JobMawb mawb && !mawb.CanDelete)
			{
				Globals.Message.ShowError(mawb.ReasonForNotAbleToDelete, DeleteFormCaption);
				return null;
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.JobMAWBModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.JobMAWBModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.JobMAWBModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.JobMAWB; }
		}
	}
}
