using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRGlbCompanyCampaign : GlbCompanyCampaign,
		Enterprise.Integration.Recruiter.IHRGlbCompanyCampaign
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public HRGlbCompanyCampaign(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ContactDataSource = HRContactDataSourceList.Codes.Staff;
		}

		public override ZString CampaignTypeCaption
		{
			get { return Res.GetString("B4E6DAE1-8AA2-45BD-8E7F-24CDC81B6EF5", "HR Campaign"); }
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G0_IsSalesAndMarketing = false;
		}

		[List("Lookups.HRContactDataSourceList")]
		public override ZString ContactDataSource
		{
			get { return contactDataSource; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					contactDataSource = IsTouchCampaign && !IsMasterCampaign
						? HRContactDataSourceList.Codes.CampaignTracking
						: value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactDataSource();
					Validation.ValidateG0_UseLastEmailSenderAddress();
				}
				if (ContactDataSourceInfo.HasErrors())
				{
					contactDataSource = IsTouchCampaign && !IsMasterCampaign
						? HRContactDataSourceList.Codes.CampaignTracking
						: HRContactDataSourceList.Codes.Staff;
				}
				ContactDataSourceInfo.RefreshBinding();
			}
		}

		ZString contactDataSource;

		public bool IsUsingStaffDataSource
		{
			get { return ContactDataSource == HRContactDataSourceList.Codes.Staff; }
		}

		public bool IsUsingJobApplicantDataSource
		{
			get { return ContactDataSource == HRContactDataSourceList.Codes.JobApplicant; }
		}

		public override string ContactDataSourceAsFilterModuleIDSuffix
		{
			get { return Lookups.HRContactDataSourceList.GetDescriptionFromCode(ContactDataSource); }
		}

		public new HRGlbCompanyCampaignLookups Lookups
		{
			get { return (HRGlbCompanyCampaignLookups)base.Lookups; }
		}

		protected override GlbCompanyCampaignLookups GetNewLookups()
		{
			return new HRGlbCompanyCampaignLookups(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("0B1E381E-106B-4DE7-852F-B6AEFA0005D9", "HR Campaign {0}", G0_CampaignID).Trim(); }
		}

		protected override GlbCompanyCampaignValidation GetNewValidation()
		{
			return new HRGlbCompanyCampaignValidation(this);
		}

		public override ModuleIdentifier DripMarketingFilterRuleModule => ModuleIDs.DripMarketingFilterRuleHR;

		protected override CampaignEmailTemplateEditor GetNewCampaignEmailTemplateEditor()
		{
			return new HRCampaignEmailTemplateEditor(this);
		}

		public new HRCampaignEmailTemplateEditor TemplateEditor => (HRCampaignEmailTemplateEditor)base.TemplateEditor;

		#endregion

		#region Simulation Contact

		[List("Lookups.HRSimulationContactDataSourceList")]
		public ZString SimulationContactDataSource
		{
			get { return simulationContactDataSource; }
			set
			{
				simulationContactDataSource = value;
				if (!IsValidationSuspended)
				{
					((HRGlbCompanyCampaignValidation)Validation).ValidateSimulationContactDataSource();
				}
				SimulationContactDataSourceInfo.RefreshBinding();
			}
		}

		ZString simulationContactDataSource;

		public ZPropertyInfo SimulationContactDataSourceInfo
		{
			get { return GetZPropertyInfo(nameof(SimulationContactDataSource)); }
		}

		#endregion

		#region IWorkflowProvider Overrides

		protected override ZString WorkflowType => new HRCampaignWorkflowDescriptor().Code;

		protected override ProcessTaskCollection GetWorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new HRCampaignProcessTasksCollection(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}

		#endregion

		internal bool GetIsValidationOnNonPersistentPropertiesSuspended => IsValidationOnNonPersistentPropertiesSuspended;
	}
}
