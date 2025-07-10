using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRGlbCompanyCampaignModule : ZFilterGridModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRGlbCompanyCampaign; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.HRCampaignWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRGlbCompanyCampaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new HRGlbCompanyCampaignFilterControl(GridCollection, (HRGlbCompanyCampaignFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var codes = new List<string>();

			foreach (CodeDescriptionPair codePair in newFactory.New<HRGlbCompanyCampaign>().Lookups.CampaignTypeList)
			{
				codes.Add(codePair.Code);
			}

			var filter = new ZQuery(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, codes);

			return new HRGlbCompanyCampaignCollection(Factory, filter);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HRGlbCompanyCampaignFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRCampaignManagement; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.HumanResourcesCampaignManager; }
		}

		#endregion
	}
}
