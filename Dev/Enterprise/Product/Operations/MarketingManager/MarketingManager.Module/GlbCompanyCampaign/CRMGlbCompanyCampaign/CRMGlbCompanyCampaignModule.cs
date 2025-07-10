using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class CRMGlbCompanyCampaignModule : ZFilterGridModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaign; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CampaignWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCompanyCampaignFilterControl(GridCollection, (GlbCompanyCampaignFilterBusinessObject)FilterBusinessObject);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var filter = new ZQuery();
			var newFactory = new BusinessObjectFactory();
			foreach (CodeDescriptionPair codePair in newFactory.New<GlbCompanyCampaign>().Lookups.CampaignTypeList)
			{
				filter.AddToFilter(JoinCondition.Or, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, codePair.Code);
			}
			return new GlbCompanyCampaignCollection(Factory, filter);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbCompanyCampaignFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CampaignManagement; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		protected internal BusinessObjectFactory GetFactoryInternal() => Factory;

		#endregion
	}
}
