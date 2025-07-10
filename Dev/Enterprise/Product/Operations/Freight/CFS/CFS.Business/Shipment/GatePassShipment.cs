using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipment : CFSShipment,
		Integration.CFS.IGatePassShipment,
		IDocumentSupportable,
		IRatingSupporter,
		ISendEmailSource
	{
		#region Schema

		public new class Schema : PackUnpackShipment.Schema
		{
			public const string JS_Calc_TotalManifested = "JS_Calc_TotalManifested";
			public const string JS_Calc_TotalOutturned = "JS_Calc_TotalOutturned";
			public const string JS_Calc_TotalInStock = "JS_Calc_TotalInStock";
			public const string JS_Calc_TotalDamaged = "JS_Calc_TotalDamaged";
			public const string JS_Calc_TotalPillaged = "JS_Calc_TotalPillaged";
			public const string JS_Calc_Shortlanded = "JS_Calc_Shortlanded";
			public const string JS_Calc_Surplus = "JS_Calc_Surplus";
			public const string JS_Calc_ContainerNumber = "JS_Calc_ContainerNumber";
			public const string JS_Calc_RelatedContainerNums = "JS_Calc_RelatedContainerNums";
			public const string JS_ToBeFullyDelivered = "JS_ToBeFullyDelivered";
			public const string JS_PrintNewDeliveriesOnSave = "JS_PrintNewDeliveriesOnSave";
			public const string JS_JK_UniqueConsignRef = "JS_JK_UniqueConsignRef";
		}

		#endregion

		public GatePassShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new GatePassShipment New(BusinessObjectFactory factory)
		{
			return factory.New<GatePassShipment>();
		}

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new GatePassShipmentValidation(this);
		}

		public new GatePassShipmentValidation Validation
		{
			get { return (GatePassShipmentValidation)base.Validation; }
		}

		#endregion

		#region Events

		public event EventHandler FullyDelivered;
		public event EventHandler FullDeliveryCancelled;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GatePassShipmentFetchStrategy(this);
		}

		class GatePassShipmentFetchStrategy : ShipmentFetchStrategy
		{
			public GatePassShipmentFetchStrategy(GatePassShipment shipment)
				: base(shipment)
			{
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				if (columns.Any(c =>
					c.ColumnName == $"{nameof(Job)}+{nameof(Job.JH_ProfitLossReasonCode)}" ||
					c.ColumnName == $"{nameof(Job)}+{nameof(Job.JH_TotalProfitRevenueMargin)}"))
				{
					var query = new ZQuery(JobHeaderSchema.JH_ParentID, BusinessObject.PK);
					query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					Factory.AddFetchHint(typeof(JobHeader), query);
				}
			}
		}

		#endregion

		public void ResetForNextSaveAndPrint()
		{
			outerPackLines = null;
		}

		#region Related Business Objects

		protected override ConsolCollection GetNewConsolCollection()
		{
			return new GatePassLoadListConsolManyToManyCollection(this);
		}

		protected override CoLoadShipmentCollection GetNewCoLoadShipmentCollection()
		{
			CoLoadShipmentCollection result = new CoLoadShipmentCollection(this, Factory);
			result.CountChanged += new CollectionCountChangedEventHandler(CoLoadShipments_CountChanged);
			return result;
		}

		#region OuterPackLines

		public new GatePassPackLineCollection OuterPackLines
		{
			get { return (GatePassPackLineCollection)base.OuterPackLines; }
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new GatePassPackLineCollection(this, Factory);
		}

		#endregion

		#region Inner Pack Lines

		protected override InnerPackLineCollection GetNewInnerPackLinesCollection()
		{
			return new GatePassInnerPackLineCollection(this);
		}

		#endregion

		#region JobDocsAndCartage

		public override Type DocsAndCartageType
		{
			get { return typeof(GatePassDocsAndCartage); }
		}

		public new GatePassDocsAndCartage DocsAndCartage
		{
			get { return (GatePassDocsAndCartage)base.DocsAndCartage; }
		}

		#endregion

		#endregion

		#region Business Object Overrides

		public override void OnLoaded()
		{
			base.OnLoaded();
			JS_PrintNewDeliveriesOnSave = true;
			HasChanges = false;
		}

		public void CleanUpAfterSaveAndPrint(bool succeeded)
		{
			if (succeeded && JS_IsFullyDelivered)
			{
				OnFullyDelivered(EventArgs.Empty);
			}
		}

		public void NotifyDeliveryHasBeenCancelled(ZString fullGatePassID, bool wasFullyDelivered)
		{
			if (wasFullyDelivered)
			{
				OnFullDeliveryCancelled(EventArgs.Empty);
			}

			CancelGatePassPrintEvent(fullGatePassID);
			JS_ToBeFullyDelivered = false;
		}

		public override void OnSaving()
		{
			ServiceEventUtils.SetupEvent(this, true, Events.EditedARecord);
			base.OnSaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				ResetForNextSaveAndPrint();
			}
			base.OnFactorySaved(saveSucceeded);
		}

		public int NumberOfNewDeliveries()
		{
			int result = 0;

			foreach (CommonPickupDeliveryConfirm confirm in DestinationCFSDepartures)
			{
				if (!confirm.IsInDatabase) //warning &include?
				{
					result++;
				}
			}
			return result;
		}

		#endregion

		#region Property Overrides

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString JS_GoodsDescription
		{
			get { return base.JS_GoodsDescription; }
			set { base.JS_GoodsDescription = value; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString JS_HouseBill
		{
			get { return base.JS_HouseBill; }
			set { base.JS_HouseBill = value; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString JS_RL_NKDestination
		{
			get { return base.JS_RL_NKDestination; }
			set { base.JS_RL_NKDestination = value; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString JS_RL_NKOrigin
		{
			get { return base.JS_RL_NKOrigin; }
			set { base.JS_RL_NKOrigin = value; }
		}

		#endregion

		#region Calculated Properties

		#region Unique Consign Ref

		public ZString JS_JK_UniqueConsignRef
		{
			get { return (Consols.Count > 0) ? Consols[0].JK_UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo JS_JK_UniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JK_UniqueConsignRef); }
		}

		#endregion

		#endregion

		#region Non-Persistent Properties

		#region JS_PrintNewDeliveriesOnSave

		protected ZBool fJS_PrintNewDeliveriesOnSave;
		public ZBool JS_PrintNewDeliveriesOnSave
		{
			get { return fJS_PrintNewDeliveriesOnSave; }
			set { SetNonPersistentPropertyValue(JS_PrintNewDeliveriesOnSaveInfo, ref fJS_PrintNewDeliveriesOnSave, value); }
		}

		public ZPropertyInfo JS_PrintNewDeliveriesOnSaveInfo
		{
			get { return GetZPropertyInfo(Schema.JS_PrintNewDeliveriesOnSave); }
		}

		#endregion

		#region Totals

		#region JS_Calc_RelatedContainerNums

		protected const int SecondsBetweenContainerNumsRefresh = 60;
		protected ZDateTime TimeOfLastContainerNumsRefresh;
		protected ZString fJS_Calc_RelatedContainerNums;
		public ZString JS_Calc_RelatedContainerNums
		{
			get
			{
				if (!TimeOfLastContainerNumsRefresh.IsValid ||
					TimeOfLastContainerNumsRefresh < ZDateTime.Now.AddSeconds(-SecondsBetweenContainerNumsRefresh))
				{
					TimeOfLastContainerNumsRefresh = ZDateTime.Now;
					fJS_Calc_RelatedContainerNums = CalculateRelatedContainerNums();
				}

				return fJS_Calc_RelatedContainerNums;
			}
		}

		public ZPropertyInfo JS_Calc_RelatedContainerNumsInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_RelatedContainerNums); }
		}

		#endregion

		#region JS_Calc_TotalManifested

		public ZInt JS_Calc_TotalManifested
		{
			get { return OuterPackLines.TotalManifestedPacks; }
		}

		public ZPropertyInfo JS_Calc_TotalManifestedInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalManifested); }
		}

		#endregion

		#region JS_Calc_TotalOutturned

		public ZInt JS_Calc_TotalOutturned
		{
			get { return OuterPackLines.TotalOutturned; }
		}

		public ZPropertyInfo JS_Calc_TotalOutturnedInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalOutturned); }
		}

		#endregion

		#region JS_Calc_TotalInStock

		public ZInt JS_Calc_TotalInStock
		{
			get
			{
				int inStock = 0;

				foreach (GatePassPackLine line in OuterPackLines)
				{
					inStock += line.UseOutturn ? line.JL_Outturn : line.JL_PackageCount;
					inStock -= line.PackagesConfirmed_DispatchedFromDestinationCFS;
				}

				return inStock;
			}
		}

		public ZPropertyInfo JS_Calc_TotalInStockInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalInStock); }
		}

		#endregion

		#region JS_Calc_TotalDamaged

		public ZInt JS_Calc_TotalDamaged
		{
			get { return OuterPackLines.TotalDamaged; }
		}

		public ZPropertyInfo JS_Calc_TotalDamagedInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalDamaged); }
		}

		#endregion

		#region JS_Calc_TotalPillaged

		public ZInt JS_Calc_TotalPillaged
		{
			get { return OuterPackLines.TotalPillaged; }
		}

		public ZPropertyInfo JS_Calc_TotalPillagedInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalPillaged); }
		}

		#endregion

		#region JS_Calc_Shortlanded

		public ZInt JS_Calc_Shortlanded
		{
			get { return 0; }
		}

		public ZPropertyInfo JS_Calc_ShortlandedInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_Shortlanded); }
		}

		#endregion

		#region JS_Calc_Surplus

		public ZInt JS_Calc_Surplus
		{
			get { return 0; }
		}

		public ZPropertyInfo JS_Calc_SurplusInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_Surplus); }
		}

		#endregion

		#endregion

		#region JS_ToBeFullyDelivered

		protected ZBool fJS_ToBeFullyDelivered;
		public ZBool JS_ToBeFullyDelivered
		{
			get { return fJS_ToBeFullyDelivered; }
			set
			{
				if (fJS_ToBeFullyDelivered != value)
				{
					fJS_ToBeFullyDelivered = value;
					JS_ToBeFullyDeliveredInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JS_ToBeFullyDeliveredInfo
		{
			get { return GetZPropertyInfo(Schema.JS_ToBeFullyDelivered); }
		}

		#endregion

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			result.AddRecipient(NotifyParty);
			result.AddRecipient(Consignee);

			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("dab7e5ca-54f7-4620-9103-59250e8965d4", "Shipment - {0}", this.JS_UniqueConsignRef); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.GatePass; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocGatePassShipment>(); }
		}

		#endregion

		#region Implementation

		protected void CreatePrintedEvents()
		{
			//Should Set GatePass OnBeforePrint
			//Should Add Events OnAfterPrint

			foreach (CommonPickupDeliveryConfirm confirm in DestinationCFSDepartures)
			{
				if (!confirm.IsInDatabase)
				{
					confirm.SetUniqueID();
					Logs.AddNew(Events.GatePassPrinted, confirm.FullGatePass);
				}
			}
		}

		protected void CancelGatePassPrintEvent(ZString fullGatePassID)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_Reference, fullGatePassID);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.GatePassPrinted.Code);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			StmALog[] printLogsForThisID = Logs.Find(filter);

			if (printLogsForThisID.Length > 0)
			{
				printLogsForThisID[0].Cancel();
			}
		}

		protected ZString CalculateRelatedContainerNums()
		{
			ZString result = "";

			foreach (CommonContainer container in Containers)
			{
				if (!result.Contains(container.JC_ContainerNum, StringComparison.Ordinal))
				{
					result += container.JC_ContainerNum + ", ";
				}
			}

			while (result.EndsWith(", ", StringComparison.Ordinal))
			{
				result = result.SubstringSafe(0, result.Length - 2);
			}

			return result;
		}

		protected GatePassPackLine GetFirstMatchByTypeAndDimensions(GatePassPackLineCollection list, GatePassPackLine pack)
		{
			foreach (GatePassPackLine potentialMatch in list)
			{
				if (pack.MatchingDimensions(potentialMatch))
				{
					return potentialMatch;
				}
			}

			return null;
		}

		//where is this used?
		protected virtual void OnFullyDelivered(EventArgs e)
		{
			if (FullyDelivered != null)
			{
				FullyDelivered(this, e);
			}
		}

		protected virtual void OnFullDeliveryCancelled(EventArgs e)
		{
			if (FullDeliveryCancelled != null)
			{
				FullDeliveryCancelled(this, e);
			}
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo NewDocManager()
		{
			return new DocManagerInfo(this, Constants.DocManagerCodes.GatePass);
		}

		#endregion

		#region IDocumentSupportable Memebers

		public override DocumentSupporter DocumentSupporter
		{
			get { return new GatePassShipmentDocumentSupporter(this); }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new GatePassShipmentInvoicingSupporter(this);
		}

		#endregion

		#region RefreshDeliveryBinding

		public override void RefreshDeliveryBinding()
		{
			JS_Calc_TotalInStockInfo.RefreshBinding();
		}

		#endregion

		#region Rating

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ShipmentRatingAdaptersProvider<GatePassShipment>(this); }
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new GatePassShipmentRatingAdapter(this);
		}

		#endregion

		#region Invoicing Supporter

		public class GatePassShipmentInvoicingSupporter : CFSShipmentInvoicingSupporter
		{
			public GatePassShipmentInvoicingSupporter(GatePassShipment parent)
				: base(parent)
			{
			}

			protected override SecurityCheckpoint GetAuditSecurityCore()
			{
				return Env.Security.CFSGatePassAuditBilling;
			}

			protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
			{
				return Env.Security.CFSGatePassJobInvoicing;
			}
		}

		#endregion
	}
}
