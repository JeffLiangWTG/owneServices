using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	[CodeProperty(SalesDashboardActivity.Schema.VSA_ActivityParentID), DescriptionProperty(SalesDashboardActivity.Schema.VSA_ActivityDescription)]
	public class SalesDashboardActivity : AutoViewSalesDashboardActivity, ISalesRelationActivity
	{
		public SalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new SalesDashboardActivityTypeDecider();

		class SalesDashboardActivityTypeDecider : TypeDecider
		{
			public override Type GetTypeForNew()
			{
				return typeof(SalesDashboardActivity);
			}

			public override Type GetTypeForBinding()
			{
				return typeof(SalesDashboardActivity);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				string activityType = row[ViewSalesDashboardActivitySchema.Constants.VSA_ActivityType].ToString();
				switch (activityType)
				{
					case SalesDashboardActivityTypeCodeList.Codes.Opportunity:
						return typeof(OpportunitySalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.Inquiry:
						return typeof(InquirySalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.Communication:
						return typeof(CommunicationSalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.Campaign:
						return typeof(CampaignSalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.OneOffQuote:
						return typeof(OneOffQuoteSalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.Quotation:
						return typeof(QuotationSalesDashboardActivity);
					case SalesDashboardActivityTypeCodeList.Codes.Project:
						return typeof(ProjectSalesDashboardActivity);
					default:
						return typeof(SalesDashboardActivity);
				}
			}
		}

		#endregion

		#region Properties

		public ZString ActivityTypeDescription
		{
			get { return new SalesDashboardActivityTypeCodeList().GetDescriptionFromCode(VSA_ActivityType); }
		}

		public ZDateTime VSA_ActivityDateLocal
		{
			get
			{
				return VSA_ActivityDate.IsValid ? Env.Time.GetLocalTimeFromUtc(VSA_ActivityDate.ToDateTime()) : VSA_ActivityDate;
			}
			set
			{
				base.VSA_ActivityDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public virtual ZString AssignedGroupCode
		{
			get
			{
				var assignedGroup = Factory.Load<GlbGroup>(VSA_GG_AssignedGroup);
				return assignedGroup != null ? assignedGroup.GG_Code : ZString.Empty;
			}
		}

		public ZDateTime VSA_SalesRelationsLastEditLocal
		{
			get { return VSA_SalesRelationsLastEditUtc.IsValid ? Env.Time.GetLocalTimeFromUtc(VSA_SalesRelationsLastEditUtc.ToDateTime()) : VSA_SalesRelationsLastEditUtc; }
		}

		protected override ZString HumanReadableNameCore => ParentActivity?.HumanReadableName ?? Res.GetString("FCE8F06C-E4B2-4449-A702-4F5E0ECAA236", "Sales Activity");

		#region Overall Acitvity Dispostion Description
		public virtual ZString OverallActivityDispositionDescription
		{
			get { return ZString.Empty; }
		}
		#endregion

		#region Tasks

		public virtual SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks
		{
			get { return new SalesDashboardProcessTaskCollectionView(new ProcessTaskCollection(Factory)); }
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SalesDashboardActivityFetchStrategy(this);
		}

		#endregion

		#region ISalesRelationActivity

		protected virtual ISalesRelationActivity ParentActivity
		{
			get { return null; }
		}

		public ZString ActivityType
		{
			get { return ParentActivity != null ? ParentActivity.ActivityType : ZString.Empty; }
		}

		string IRelatableActivity.TablePrefix
		{
			get { return ParentActivity != null ? ParentActivity.TablePrefix : string.Empty; }
		}

		public IOrgHeader Client
		{
			get { return ParentActivity != null ? ParentActivity.Client : null; }
		}

		public ZBool ClientHasChanges
		{
			get { return ParentActivity != null ? ParentActivity.ClientHasChanges : ZBool.False; }
		}

		public IOrgContact Contact
		{
			get { return ParentActivity != null ? ParentActivity.Contact : null; }
		}

		public ZBool ContactHasChanges
		{
			get { return ParentActivity != null ? ParentActivity.ContactHasChanges : ZBool.False; }
		}

		[BusinessObjectTestExclude]
		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get { return ParentActivity != null ? ParentActivity.RelatedChildActivityPivotCollection : null; }
		}

		[BusinessObjectTestExclude]
		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get { return ParentActivity != null ? ParentActivity.RelatedParentActivityPivotCollection : null; }
		}

		public ZString Summary
		{
			get { return ParentActivity != null ? ParentActivity.Summary : ZString.Empty; }
		}

		public ZDateTime SystemCreateTimeUtc
		{
			get { return ParentActivity != null ? ParentActivity.SystemCreateTimeUtc : ZDateTime.Empty; }
		}

		public ZString SystemCreateUser
		{
			get { return ParentActivity != null ? ParentActivity.SystemCreateUser : ZString.Empty; }
		}

		public ZDateTime SystemLastEditTimeUtc
		{
			get { return ParentActivity != null ? ParentActivity.SystemLastEditTimeUtc : ZDateTime.Empty; }
		}

		public ZString SystemLastEditUser
		{
			get { return ParentActivity != null ? ParentActivity.SystemLastEditUser : ZString.Empty; }
		}

		public ISalesRelationModel SalesRelationModel
		{
			get { return ParentActivity != null ? ParentActivity.SalesRelationModel : null; }
		}

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ParentActivity != null ? ParentActivity.ActivityNotePropertyName : ZString.Empty; }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		#endregion

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return ParentActivity != null ? ParentActivity.ShouldIgnoreSuperAndSubActivityRelationships : ZBool.False; }
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;
	}
}
