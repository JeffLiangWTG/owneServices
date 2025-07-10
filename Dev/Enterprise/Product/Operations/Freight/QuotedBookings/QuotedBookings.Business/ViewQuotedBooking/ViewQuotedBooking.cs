using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[PreventDelete(true)]
	[CodeProperty(nameof(Reference), nameof(ViewQuotedBooking.BuildCodeQuery)), DescriptionProperty(nameof(Reference))]
	[UniversalCopyInstanceType(InstanceType = typeof(QuotedBooking))]
	[ViewComplianceRiskStatusProvider(ProviderBusinessObjectType = typeof(QuotedBooking))]
	public class ViewQuotedBooking : AutoViewQuotedBooking, IViewQuotedBooking, IDocManagerSupport, ICancellable, ISalesRelationActivity, IImportParentRelatedActivityInfoOnNew, IWorkflowProvider, ITemplateRecordProvider, IUniversalCopyValidationStrategy, IEDocsPluginHostDecider, IViewComplianceRiskStatusProvider
	{
		public ViewQuotedBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public QuotedBooking QuotedBooking
		{
			get { return quotedBooking ?? (quotedBooking = GetQuotedBookingCore()); }
		}

		IComplianceItemRiskStatusProvider IViewComplianceRiskStatusProvider.GetProviderBusinessObject()
		{
			return QuotedBooking?.Booking is not null && !QuotedBooking.IsTemplate
				? QuotedBooking
				: null;
		}

#if DEBUG
		internal
#endif
		QuotedBooking quotedBooking;

		protected virtual QuotedBooking GetQuotedBookingCore()
		{
			QuotedBooking result = null;
			if (!VB_TH.IsEmpty || !VB_JS.IsEmpty)
			{
				result = QuotedBooking.New(VB_TH, VB_JS, Factory);
			}
			return result;
		}

		#endregion

		public ZString ScreeningStatus
		{
			get
			{
				var result = ZString.Empty;

				if (QuotedBooking?.Booking != null && !QuotedBooking.IsTemplate)
				{
					result = QuotedBooking.Booking.JS_ScreeningStatus;
				}

				return result;
			}
		}

		public static ViewQuotedBooking LoadOrCreate(QuotedBooking quotedBooking)
		{
			Argument.NotNull(quotedBooking, "quotedBooking");

			var result = quotedBooking.Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			if (result == null)
			{
				result = quotedBooking.Factory.NewWithPrimaryKey<ViewQuotedBooking>(quotedBooking.PK.ToGuid());
				result.Initialize(quotedBooking);
			}
			result.quotedBooking = quotedBooking;

			return result;
		}

		void Initialize(QuotedBooking parent)
		{
			VB_IsCanceled = ZBool.False;

			if (parent.Booking != null)
			{
				VB_JS = parent.Booking.PK;
				VB_IsConsolidated = parent.Booking.JS_IsForwardRegistered;
			}

			if (parent.Quote != null)
			{
				VB_TH = parent.Quote.PK;
				VB_GC = parent.Quote.TH_GC;
				VB_QuoteNumber = parent.Quote.TH_QuoteNumber;
			}

			if (parent.Quote != null)
			{
				parent.Quote.OnQuoteSaved += OnQuoteSaved;
			}
			else if (parent.Booking != null)
			{
				parent.Booking.OnShipmentSaved += OnShipmentSaved;
			}
		}

		void OnShipmentSaved(object sender, EventArgs e)
		{
			if (e is CommonShipment.SavedEventArgs savedEventArgs && savedEventArgs.HasSaveSucceeded)
			{
				PopulateAuditColumns();
			}
		}

		void OnQuoteSaved(object sender, EventArgs e)
		{
			if (e is Quote.SavedEventArgs savedEventArgs && savedEventArgs.HasSaveSucceeded)
			{
				PopulateAuditColumns();
			}
		}

		void PopulateAuditColumns()
		{
			if (QuotedBooking is IAuditDetails quotedBookingWithAudits)
			{
				VB_SystemCreateTimeUtc = quotedBookingWithAudits.SystemCreateTimeUtc;
				VB_SystemCreateUser = quotedBookingWithAudits.SystemCreateUser;
				VB_SystemLastEditTimeUtc = quotedBookingWithAudits.SystemLastEditTimeUtc;
				VB_SystemLastEditUser = quotedBookingWithAudits.SystemLastEditUser;
			}
		}

		#region Save

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			base.OnSaving();
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (QuotedBooking != null)
			{
				throw new NotSupportedException("Deletion of the ViewQuotedBooking is not supported.");
			}
			else
			{
				if (RelatedChildActivityPivotCollection != null)
				{
					RelatedChildActivityPivotCollection.DeleteAll();
				}

				if (RelatedParentActivityPivotCollection != null)
				{
					RelatedParentActivityPivotCollection.DeleteAll();
				}

				base.Delete();
			}
		}

		#endregion

		#region ICancellable Members

		string ICancellable.CanCancel()
		{
			return ((ICancellable)QuotedBooking).CanCancel();
		}

		string ICancellable.CanReactivate()
		{
			return ((ICancellable)QuotedBooking).CanReactivate();
		}

		bool ICancellable.IsCancelled
		{
			get { return ((ICancellable)QuotedBooking).IsCancelled; }
			set { ((ICancellable)QuotedBooking).IsCancelled = value; }
		}

		bool ICancellable.IsCancelledHasChanged
		{
			get { return ((ICancellable)QuotedBooking).IsCancelledHasChanged; }
		}

		#endregion

		#region Properties

		public ZString Reference => QuotedBooking?.Quote != null
			? QuotedBooking.Quote.TH_QuoteNumber
			: QuotedBooking?.Booking?.JS_UniqueConsignRef ?? ZString.Empty;

		public ZPropertyInfo ReferenceInfo => GetZPropertyInfo(nameof(Reference));

		public static ZQuery BuildCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			shipmentQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, SQLComparisonOperator.Equal, DBNull.Value);
			shipmentQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			var ratingHeaderSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), ViewQuotedBookingSchema.VB_TH);
			ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, comparisonOperator, value);

			var ratingHeaderQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			ratingHeaderQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, SQLComparisonOperator.NotEqual, DBNull.Value);
			ratingHeaderQuery.AddSubQuery(ratingHeaderSubQuery, JoinCondition.And);

			query.AddToFilter(shipmentQuery);
			query.AddToFilter(ratingHeaderQuery, JoinCondition.Or);

			return query;
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public int VB_IsCanceled_MaxLength
		{
			get { return 1; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			VB_IsCanceled = ZBool.False;
			VB_IsConsolidated = ZBool.False;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return QuotedBooking != null ? QuotedBooking.HumanReadableName : base.HumanReadableNameCore; }
		}

		#endregion

		#region IViewQuotedBooking Members

		IJobHeader IViewQuotedBooking.Job
		{
			get { return QuotedBooking != null ? QuotedBooking.Job : null; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				var code = string.Empty;

				switch (QuotedBooking?.ObjectState)
				{
					case QuotedBookingState.AcceptedBookingWithQuote:
					case QuotedBookingState.UnacceptedBookingWithQuote:
					case QuotedBookingState.BookingOnly:
						code = Core.Constants.DocManagerCodes.Booking;
						break;
					case QuotedBookingState.QuoteOnly:
						code = Core.Constants.DocManagerCodes.OneOffQuote;
						return new RatingDocManagerInfo(QuotedBooking.Quote, code);
				}
				return new DocManagerInfo(this, code);
			}
		}

		#endregion

		#region IEDocsPluginHostDecider

		public IBusiness HostBusinessEntity => (QuotedBooking as IEDocsPluginHostDecider).HostBusinessEntity;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ViewQuotedBookingFetchStrategy(this);
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).ShouldIgnoreSuperAndSubActivityRelationships : ZBool.True; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).ActivityType : ZString.Empty; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).Client : null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).ClientHasChanges : ZBool.False; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).Contact : null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).ContactHasChanges : ZBool.False; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).Summary : ZString.Empty; }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).RelatedChildActivityPivotCollection : null; }
		}

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get { return QuotedBooking != null ? ((IRelatableActivity)QuotedBooking).RelatedParentActivityPivotCollection : null; }
		}

		#endregion

		#region ISalesRelationActivity Members

		public ISalesRelationModel SalesRelationModel
		{
			get { return QuotedBooking != null ? QuotedBooking.SalesRelationModel : null; }
		}

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (QuotedBooking != null)
			{
				return ((IImportParentRelatedActivityInfoOnNew)QuotedBooking).ImportParentInfo(parentActivity, deciderFactory);
			}

			return true;
		}

		#endregion

		#region IWorkflowProvider Members

		IWorkflowProvider ParentProvider
		{
			get { return QuotedBooking; }
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return ParentProvider != null ? ParentProvider.GetWorkflowInformationProvider() : null;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		public ProcessTaskCollection WorkflowItems
		{
			get { return ParentProvider != null ? ParentProvider.WorkflowItems : new ProcessTaskCollection(Factory); }
		}

		#endregion

		#region IWorkflowProviderCore Members

		public CargoWise.Integration.IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return ParentProvider != null ? ParentProvider.GetTemplateSelectionCriteria() : null;
		}

		public ZString WorkflowType
		{
			get { return ParentProvider != null ? ParentProvider.WorkflowType : null; }
		}

		#endregion

		#region Template record

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (IsTemplate && TemplateRecord != null)
				{
					result.Add(TemplateRecord);
				}
				return result.ToArray();
			}
		}
		bool ITemplateRecordProvider.IsTemplateRecord
		{
			get
			{
				return ((ITemplateRecordProvider)QuotedBooking)?.IsTemplateRecord ?? false;
			}
			set
			{
				if (QuotedBooking == null)
				{ throw new InvalidOperationException("QuotedBooking must be initialized for a Template Record ViewQuotedBooking"); }
				((ITemplateRecordProvider)QuotedBooking).IsTemplateRecord = value;
			}
		}

		public ZBool IsTemplate => ((ITemplateRecordProvider)this).IsTemplateRecord;

		public ZPropertyInfo IsTemplateInfo => GetZPropertyInfo(nameof(IsTemplate));

		ITemplateRecord ITemplateRecordProvider.TemplateRecord
		{
			get => TemplateRecord;
			set => TemplateRecord = (StmTemplateRecord)value;
		}

		public StmTemplateRecord TemplateRecord
		{
			get
			{
				return (StmTemplateRecord)((ITemplateRecordProvider)QuotedBooking)?.TemplateRecord;
			}
			set
			{
				if (QuotedBooking == null)
				{ throw new InvalidOperationException("QuotedBooking must be initialized for a Template Record ViewQuotedBooking"); }
				((ITemplateRecordProvider)QuotedBooking).TemplateRecord = value;
			}
		}

		void ITemplateRecordProvider.SaveToTemplateRecord()
		{
			if (QuotedBooking == null)
			{ throw new InvalidOperationException("QuotedBooking must be initialized for a Template Record ViewQuotedBooking"); }
			((ITemplateRecordProvider)QuotedBooking).SaveToTemplateRecord();
		}

		void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord sourceTemplateRecord)
		{
			if (QuotedBooking == null)
			{ throw new InvalidOperationException("QuotedBooking must be initialized for a Template Record ViewQuotedBooking"); }
			((ITemplateRecordProvider)QuotedBooking).LoadFromTemplateRecord(sourceTemplateRecord);

			if (((StmTemplateRecord)sourceTemplateRecord).IsInDatabase)
			{
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
				((INeedRow)this)?.Row.AcceptChanges();
			}
		}

		BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
		{
			if (QuotedBooking == null)
			{ throw new InvalidOperationException("QuotedBooking must be initialized for a Template Record ViewQuotedBooking"); }
			return ((ITemplateRecordProvider)QuotedBooking).InstantiateFromTemplateRecord(factory, elementType, templateRecord);
		}

		#endregion

		#region Logs

		protected override Logs GetNewLogs() => new ViewQuotedBookingLogs(this);

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			quotedBooking = new QuotedBooking(Factory);
		}

#endif

		#region IUniversalCopyValidationStrategy Member

		public string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree) =>
			QuotedBookingHelper.ValidateUniversalCopyPreconditions(configurationTree, QuotedBooking);

		#endregion
	}
}
