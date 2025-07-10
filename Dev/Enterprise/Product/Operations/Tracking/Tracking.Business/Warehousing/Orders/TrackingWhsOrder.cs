using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Tracking.Business
{
	public sealed class TrackingWhsOrder : NonPersistentBusinessObjectWithLogsAndNotes,
		IWhsOrderWrapperStrategy,
		IBizOChangesEmailNotification,
		IWebDocumentsWithUploadSupport,
		ITransactionSupport,
		IWebUserEditableNoteSupport,
		IWebUserVisibleNotesSupport,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider,
		ITemplateCopyable,
		IWrappedBizOProvider
	{
		public TrackingWhsOrder(WhsOrder order) : base(order.Factory)
		{
			this.order = order;
			RegisterEditableChildObject(order);
		}

		#region Schema

		public abstract class Schema : WhsOrder.Schema { }

		public abstract class WrapperSchema
		{
			public const string WD_TransportCoUrl = "WD_TransportCoUrl";
			public const string WD_ExternalReference = SchemaRoot + AutoWhsDocket.Schema.WD_ExternalReference;
			public const string WD_DocketID = SchemaRoot + AutoWhsDocket.Schema.WD_DocketID;
			public const string WD_TransportReference = SchemaRoot + AutoWhsDocket.Schema.WD_TransportReference;
			public const string TrackingRequiredDate = nameof(TrackingRequiredDate);
			public const string WD_DocketStatusDescription = SchemaRoot + WhsDocket.Schema.WD_DocketStatusDescription;
			public const string WD_TotalUnits = SchemaRoot + AutoWhsDocket.Schema.WD_TotalUnits;
			public const string WD_TotalWeight = SchemaRoot + AutoWhsDocket.Schema.WD_TotalWeight;
			public const string WD_TotalCubic = SchemaRoot + AutoWhsDocket.Schema.WD_TotalCubic;
			public const string WD_FinalisedDate = SchemaRoot + AutoWhsDocket.Schema.WD_FinalisedDate;
			public const string WD_CustomerReference = SchemaRoot + AutoWhsDocket.Schema.WD_CustomerReference;
			public const string WD_WW_Whs = SchemaRoot + AutoWhsDocket.Schema.WD_WW_Whs;
			public const string WD_PK = SchemaRoot + "PK";
		}

		public const string SchemaRoot = "WhsOrder.";
		public const string SchemaPath = "WhsOrder+";

		public static string GetSchemaPath(string root, bool isRoot = false)
		{
			return (isRoot ? SchemaRoot : SchemaPath) + root;
		}

		#endregion

		#region Flyweight & Wrapped Object

		readonly WhsOrder order;

		public WhsOrder WhsOrder
		{
			get { return order; }
		}

		IWhsOrderWrapperCallback WhsOrderCallback
		{
			get { return order; }
		}

		public static TrackingWhsOrder GetTrackingWhsOrder(WhsOrder order)
		{
			Argument.NotNull(order, "WhsOrder");
			var result = order.Factory.GetCachedValue("TrackingWhsOrder|" + order.PK, () => new TrackingWhsOrder(order));

			if (result.IsDeleted)
			{
				result.ResetState();
			}

			return result;
		}

		public override void Delete()
		{
			base.Delete();

			if (!WhsOrder.IsDeleted)
			{
				WhsOrder.Delete();
			}
		}

		public override bool IsInDatabase
		{
			get { return WhsOrder.IsInDatabase; }
		}

		public new ZGuid PK { get { return WhsOrder.PK; } }

		#endregion

		#region Implementation of IWrappedBizOProvider

		public BusinessObject GetWrappedBizO()
		{
			return WhsOrder;
		}

		public string GetWrappedBindTo(string bindTo)
		{
			return GetSchemaPath(bindTo);
		}

		#endregion

		#region Implementation of IWhsOrderWrapperStrategy

		WhsDocketValidation IWhsOrderWrapperStrategy.GetNewValidation()
		{
			return new TrackingWhsOrderValidation(this);
		}

		WhsDocketLookups IWhsOrderWrapperStrategy.GetNewLookups()
		{
			return new TrackingWhsOrderLookups(this);
		}

		bool IWhsOrderWrapperStrategy.DoesNotHaveJobEnteredEvent
		{
			get { return !WhsOrderCallback.EventLogExists(AutoEvents.WarehouseJobEntered.Code); }
		}

		WhsPickableDocketLine[] IWhsOrderWrapperStrategy.LinesForSelectedOrderLines()
		{
			return WhsOrder.Lines.ToArray<WhsPickableDocketLine>();
		}

		void IWhsOrderWrapperStrategy.TemplateCopyLines(WhsPickableDocket copy)
		{
			foreach (WhsPickableDocketLine line in WhsOrder.AllLines.ToArray())
			{
				var copiedLine = (WhsPickableDocketLine)line.Clone();
				copy.Lines.Add(copiedLine);
			}
		}

		void IWhsOrderWrapperStrategy.OrderOnSaving()
		{
			foreach (JobDocAddress docAddress in WhsOrder.DocAddresses.ToArray())
			{
				docAddress.E2_SuppressAddressValidationError = docAddress.E2_AddressOverride;
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			PreventSaveIfOrderAlreadyPicked();
		}

		#region PreventSaveIfOrderAlreadyPicked

		void PreventSaveIfOrderAlreadyPicked()
		{
			if (!WhsOrder.IsDeleted)
			{
				if (!WhsOrder.IsAttachedToPickButNotFinalised && WhsOrder.IsAttachedToPick && GetTrackingWhsOrderLineFromLocalCatche.Any(l => l.CriticalFieldInfos.Any(c => c.HasChanges)))
				{
					ThrowExceptionCannotEditCriticalInformationForPickedOrder();
				}
				else if (IsInDatabase && WhsOrder.HasChangesForLock)
				{
					var results = Factory.GetCachedValue(WhsPick.WhsOnFactorySaving_TakeAllLocksAtOnce, OnFactorySaving_TakeAllLocksAtOnce, CacheStalenessPolicy.StaleBeforeFactorySavingTransaction);

					IZType pickInDb = ZGuid.Empty;
					results.TryGetValue(PK, out pickInDb);
					// will only be null if developers added some weird code between caching and accessing cache in OnFactorySaving. 
					// will likelly cause most of the existing Whs tests to fail.
					if (pickInDb == null)
					{
						throw new ZCannotSaveException(
							"Prevented attempt to save data without concurency locking. Please try to save again.",
							"Concurency Error", ExceptionType.BusinessFailure);
					}
					if (pickInDb != null && !pickInDb.IsEmpty && HasCriticalChangesOnLines())
					{
						var message = ResString.GetMultilingualString("C65C9D40-7EFF-4896-907A-16DE0D8FC81E",
		@"Cannot edit critical information. Another user has picked {0}.", HumanReadableName);
						var heading = Res.GetString("8B7A912D-8785-4437-8043-0B32105755D3", "{0} is picked", HumanReadableName);

						ThrowExceptionCannotEditCriticalInformationForPickedOrder();
					}
				}
			}
		}

		void ThrowExceptionCannotEditCriticalInformationForPickedOrder()
		{
			var message = ResString.GetMultilingualString("C65C9D40-7EFF-4896-907A-16DE0D8FC81E",
								@"Cannot edit critical information. Another user has picked {0}.", HumanReadableName);

			WhsOrder.AddRowError(message);
			throw new ZSaveException(new ZDataException(new InvalidOperationException(message), null, null), Factory);  // this will make ZPage.SaveDataSourceFactory return false
		}

		Dictionary<ZGuid, IZType> OnFactorySaving_TakeAllLocksAtOnce()
		{
			return WhsPick.OnFactorySaving_TakeAllLocksAtOnce(Factory);
		}

		bool HasCriticalChangesOnLines()
		{
			IEnumerable<TrackingWhsOrderLine> orderLines;

			if (WhsOrder.IsLinesInitialised)
			{
				orderLines = Lines.Cast<TrackingWhsOrderLine>();
			}
			else
			{
				orderLines = GetTrackingWhsOrderLineFromLocalCatche;
			}

			return orderLines.Any(l => l.IsInDatabase && l.CriticalFieldInfos.Any(p => p.HasChanges));
		}

		IEnumerable<TrackingWhsOrderLine> GetTrackingWhsOrderLineFromLocalCatche
		{
			get
			{
				var memoryQuery = new ZQuery(WhsDocketLineSchema.WE_WD, PK) { FetchOnlyFromLocalCache = true };
				return Factory.Load<WhsOrderLine>(memoryQuery).Select(l => new TrackingWhsOrderLine(l));
			}
		}

		#endregion

		#region WD_TransportCoUrl

		[CargoWise.ComponentModel.MaxLength(255)]
		public ZString WD_TransportCoUrl
		{
			get
			{
				ZString result = "";

				if (!WhsOrder.WD_TransportReference.IsEmpty && WhsOrder.TransportCoDocAddress.Organisation != null)
				{
					ZString url = WhsOrder.TransportCoDocAddress.Organisation.CartageTransportWebSite;

					if (UrlValidation.IsValidUrl(url) && url.ToLower().Contains(Constants.TransportCoHotlinkOpener.CargoWiseREF.ToLower()))
					{
						result = url.ReplaceIgnoringCase(Constants.TransportCoHotlinkOpener.CargoWiseREF, WhsOrder.WD_TransportReference);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo WD_TransportCoUrlInfo
		{
			get { return GetZPropertyInfo(WrapperSchema.WD_TransportCoUrl); }
		}

		#endregion

		#region Lines

		public TrackingWhsOrderSummaryLineCollection SummaryLines
		{
			get
			{
				return new TrackingWhsOrderSummaryLineCollection(Lines, Factory);
			}
		}

		public TrackingWhsOrderLineCollection Lines
		{
			get
			{
				var lines = new TrackingWhsOrderLineCollection(this);
				lines.Load();
				lines.Sort(AutoWhsDocketLine.Schema.WE_LineNo, ListSortDirection.Ascending);
				if (WhsOrder.IsAttachedToPick)
				{
					lines.ForEach(l => l.SetReadOnlyIncludingChildren(true));
				}
				return lines;
			}
		}

		#endregion

		#region Properties

		public bool CanCancelDocket
		{
			get { return WhsOrder.CanCancel() == ZString.Empty; }
		}

		public bool CanEdit
		{
			get
			{
				return WhsOrder.WD_DocketStatus == DocketStatus.Codes.Entered ||
					WhsOrder.WD_DocketStatus == DocketStatus.Codes.New;
			}
		}

		public ZDateTime TrackingRequiredDate
		{
			get => WhsOrder.RequiredDate;
			set => WhsOrder.RequiredDate = value.IsValid && !value.IsEmpty ? new ZDateTime(value.Year, value.Month, value.Day, 23, 59, 0) : ZDateTime.Empty;
		}

		public ZPropertyInfo TrackingRequiredDateInfo
		{
			get { return WhsOrder.RequiredDateInfo; }
		}

		#endregion

		#region Helper Methods

		public static TrackingWhsOrder FromPKFilteredByContact(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, WhsDocketSchema.PK, pK, siteUser);
		}

		public static TrackingWhsOrder FromNumberFilteredByContact(BusinessObjectFactory factory, ZString number, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, WhsDocketSchema.WD_ExternalReference, number, siteUser);
		}

		static TrackingWhsOrder FromFieldFilteredByContact(BusinessObjectFactory factory, SchemaColumn column, object value, TrackingSiteUser siteUser)
		{
			TrackingWhsOrder result = null;
			if (siteUser != null && siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
			{
				result = TrackingHelper.Get(factory.LoadTop1<WhsOrder>(new ZQuery(column, value)));
			}
			else
			{
				result = TrackingHelper.Get(OrgRestrictionFilterFactory.LoadFilteredByContact<WhsOrder>(factory, column, value));
			}
			if (result != null)
			{
				result.fSiteUser = siteUser;
			}
			return result;
		}

		public CustomLabelInfoList GetAdditionalInformationFields()
		{
			WhsDocket.CustomLabelsProvider provider = new WhsDocket.CustomLabelsProvider(WhsOrder);
			CustomLabelInfoList customLabels = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, Factory);

			CustomLabelInfoList customLabelInfoList = new CustomLabelInfoList(typeof(TrackingWhsOrder), provider.ConfigOrgProvider.ConfigOrg, ResString.GetMultilingualString("6e61b4a9-47fd-49aa-b115-45944f51c501", "the client of the warehouse"), Factory);

			foreach (CustomLabelInfo field in customLabels)
			{
				if (field.IsEnabled && field.LabelName.StartsWith("WhsDocket."))
				{
					customLabelInfoList.Add(field);
				}
			}

			return customLabelInfoList;
		}

		#endregion

		#region SiteUser

		public TrackingSiteUser SiteUser
		{
			get { return fSiteUser; }
			set { fSiteUser = value; }
		}
		TrackingSiteUser fSiteUser;

		#endregion

		#region LoggedInContact

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		#endregion

		#region UserEditable & Visible Notes

		public IStmNoteParent NotesParentBO
		{
			get { return WhsOrder; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get
			{
				if (fUserEditableNoteHelper == null)
				{
					fUserEditableNoteHelper = GetNewUserEditableNoteHelper();
				}
				return fUserEditableNoteHelper;
			}
		}

		WebUserEditableNote fUserEditableNoteHelper;

		WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.HandlingInstructions);
		}

		public WebUserVisibleNotes NotesHelper
		{
			get
			{
				if (fNotesHelper == null)
				{
					fNotesHelper = new WebUserVisibleNotes(this);
				}

				return fNotesHelper;
			}
		}

		public bool ShowAgentNotes
		{
			get { return false; }
		}

		WebUserVisibleNotes fNotesHelper;

		#endregion

		#region IBizOChangesEmailNotification Members

		ZString IBizOChangesEmailNotification.Number
		{
			get { return WhsOrder.WD_DocketID; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get { return WhsOrder.Warehouse != null ? WhsOrder.Warehouse.RelatedCompanyBranch : null; }
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup; }
		}

		public ZBool IsCancelled
		{
			get { return WhsOrder.IsCancelled; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("23bb2605-489a-43e9-b4cf-360e1c4055d0", "Warehouse Order {0}", WhsOrder.WD_DocketID); }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get { return WhsOrder.Client; }
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.WarehouseOrdersNotificationOptions; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.WarehouseServices);
			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.WhsOrder; }
		}

		#region Email Reporting

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Email should be in English, no need to be translated")]
		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("6f1446d7-f156-4e12-98dd-1b0975be889a", "Warehouse"), (WhsOrder.Warehouse != null) ? WhsOrder.Warehouse.WW_WarehouseName : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("90bf6398-0294-4c82-a15d-8db2d8744161", "Order Number"), WhsOrder.WD_ExternalReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("02476047-ccd6-4fe7-b49d-3e1fecdd0ae0", "Required Date"), TrackingRequiredDate);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d4543889-7a68-4cde-9e94-30636b1cb059", "Total Units"), WhsOrder.WD_TotalUnits);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f3f180ad-ee95-4c85-ab4f-60ae2966c3b3", "Consignee"), WhsOrder.ConsigneeDocAddress.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("9286b85c-c97f-4024-aa3a-c3d0d002ceb5", "Consignee Address"), WhsOrder.ConsigneeDocAddress.AddressAsASingleLine);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1ee5fdee-2382-4b7c-802c-0b0f2cdcb30b", "Consignee Contact"), WhsOrder.ConsigneeDocAddress.E2_Contact);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("e866ae4e-a98c-4e39-965d-7d72cd435c4c", "Goods Billed To"), WhsOrder.GoodsBillToDocAddress.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("c170872c-9aa9-45a8-a334-e3a6504ad264", "Goods Billed To Address"), WhsOrder.GoodsBillToDocAddress.AddressAsASingleLine);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("148a6590-eea9-4ad2-9681-9d8684f81e46", "Goods Billed To Contact"), WhsOrder.GoodsBillToDocAddress.E2_Contact);

			AddReferencesForEmailReporting(state, WhsOrder.References);
			AddOrderLinesForEmailReporting(state, WhsOrder.Lines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		void AddReferencesForEmailReporting(DataState state, WhsDocketReferenceCollection collection)
		{
			int i = 1;
			foreach (WhsDocketReference reference in collection)
			{
				var value = GenerateReferenceDetailsForEmailReporting(reference);

				var propertyName = ResString.GetMultilingualString("f29b5830-6684-49db-908e-0c94106453c5", "Reference {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		public MultilingualString GenerateReferenceDetailsForEmailReporting(WhsDocketReference reference)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("1587fc4f-3176-40e6-b15d-843cb80c83d6", "Ref Type: {0}", reference.Lookups.ReferenceTypes.GetCodeDescriptionPairList().GetMultilingualDescriptionFromCode(reference.WX_RefType)),
				ResString.GetMultilingualString("48f2dede-a3f2-4f38-a866-b13b0e5dbb35", "Reference: {0}", reference.WX_Reference));
		}

		void AddOrderLinesForEmailReporting(DataState state, WhsOrderLineCollection collection)
		{
			int i = 1;
			foreach (WhsOrderLine line in collection)
			{
				var value = GenerateOrderLineDetailsForEmailReporting(line);

				var propertyName = ResString.GetMultilingualString("eb8c65c3-e141-4925-be32-df798452ef5c", "Order Line {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		public MultilingualString GenerateOrderLineDetailsForEmailReporting(WhsOrderLine orderLine)
		{
			var line = TrackingHelper.Get(orderLine).WhsOrderLine;

			var value = MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("0f7e1692-1ba8-496a-b02d-71c0ce16b153", "Product: {0}", line.ProductCode),
				ResString.GetMultilingualString("38fc2fda-843b-45a9-8cfd-076f31362077", "Packs: {0}", line.WE_PackQuantity),
				ResString.GetMultilingualString("b7cb582e-32f6-4ab5-b377-e8d1f3c45a0d", "Packs UQ: {0}", line.WE_F3_NKPackType),
				ResString.GetMultilingualString("e8532a45-900f-4daf-bd3a-a5414380ab5c", "Quantity: {0}", line.WE_TransactionQuantity));

			var client = line.Docket?.Client;
			if (client != null)
			{
				value = MultilingualString.Join(System.Environment.NewLine, value,
					MultilingualString.Join(": ", client.PartAttributeManager.PartAttributeName1, (NoResString)line.WE_PartAttrib1),
					MultilingualString.Join(": ", client.PartAttributeManager.PartAttributeName2, (NoResString)line.WE_PartAttrib2),
					MultilingualString.Join(": ", client.PartAttributeManager.PartAttributeName3, (NoResString)line.WE_PartAttrib3),
					MultilingualString.Join(": ", ResString.GetMultilingualString("46df433d-440f-4845-8a58-caf0df9628c6", "Serial Number"), (NoResString)line.WE_SerialNumber));
			}

			return value;
		}

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (fPropertiesForEmailReporting == null)
				{
					fPropertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return fPropertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection fPropertiesForEmailReporting;

		ZGuid IBizOChangesEmailNotification.PK
		{
			get { return WhsOrder.PK; }
		}

		BusinessObjectFactory IBizOChangesEmailNotification.Factory
		{
			get { return WhsOrder.Factory; }
		}

		bool IBizOChangesEmailNotification.IsInDatabase
		{
			get { return WhsOrder.IsInDatabase; }
		}

		bool IBizOChangesEmailNotification.IsDeleted
		{
			get { return WhsOrder.IsDeleted; }
		}

		bool IBizOChangesEmailNotification.HasChanges
		{
			get { return WhsOrder.HasChanges; }
		}

		#endregion

		#endregion

		#region Related documents

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (fDocumentHelper == null)
				{
					fDocumentHelper = new DocumentSupport(this);
				}

				return fDocumentHelper;
			}
		}
		DocumentSupport fDocumentHelper;

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				var result = new List<ZGuid>();

				foreach (var bookingPK in TransportBookingLoader.GetBookingPKs(WhsOrder))
				{
					result.Add(bookingPK);
				}

				return result;
			}
		}

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)WhsOrder).DocManagerInfo;
			}
		}

		public DocumentUploadSupport DocumentUploadHelper
		{
			get
			{
				if (documentUploadHelper == null)
				{
					documentUploadHelper = new DocumentUploadSupport(Factory);
				}
				return documentUploadHelper;
			}
		}

		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
			fDocumentHelper = null;
		}

		#endregion

		#region ITransactionSupport Members

		public ZString Reference
		{
			get { return WhsOrder.WD_DocketID; }
		}

		public TrackingInvoiceLoader InvoiceLoader
		{
			get
			{
				if (fInvoiceLoader == null)
				{
					fInvoiceLoader = new TrackingInvoiceLoader(this);
				}

				return fInvoiceLoader;
			}
		}
		TrackingInvoiceLoader fInvoiceLoader;

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
		}

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(WebSiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return WebSiteUser?.CanViewEvents ?? false; }
		}

		TrackingSiteUser WebSiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(WhsOrder)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(WhsOrder, this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new UpdateableMilestoneEventsHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.WarehouseOrderMilestoneEventUpdates.Value, WebParties);
				}
				return new List<string>();
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();
					webParties.Add(WebPartyType.Client, WhsOrder.Client);
					webParties.Add(WebPartyType.Transport, WhsOrder.GetTransportCo());
					webParties.Add(WebPartyType.Consignee, WhsOrder.Consignee);
					webParties.Add(WebPartyType.GoodsBilledTo, WhsOrder.GoodsBillTo);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region Overrides of NonPersistentBusinessObjectWithLogsAndNotes

		protected override BusinessObject LogsAndNotesTarget { get { return WhsOrder; } }

		#endregion

		#region Implementation of ITemplateCopyable

		public IBusiness TemplateCopy()
		{
			return TrackingHelper.Get(((ITemplateCopyable)WhsOrder).TemplateCopy() as WhsOrder);
		}

		#endregion
	}
}
