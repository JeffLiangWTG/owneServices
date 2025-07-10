using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReceive : NonPersistentBusinessObjectWithLogsAndNotes,
		IWhsReceiveWrapperStrategy,
		IBizOChangesEmailNotification,
		IWebDocumentsWithUploadSupport,
		ITransactionSupport,
		IWebUserEditableNoteSupport,
		IWebUserVisibleNotesSupport,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider,
		IWrappedBizOProvider,
		ITemplateCopyable
	{
		TrackingWhsReceive(WhsReceive receive) : base(receive.Factory)
		{
			WhsReceive = receive;
		}

		#region Schema

		public abstract class Schema : WhsReceive.Schema { }

		public abstract class WrapperSchema
		{
			public const string WD_ExternalReference = SchemaRoot + AutoWhsDocket.Schema.WD_ExternalReference;
			public const string WD_DocketID = SchemaRoot + AutoWhsDocket.Schema.WD_DocketID;
			public const string WD_BookingDate = SchemaRoot + AutoWhsDocket.Schema.WD_BookingDate;
			public const string WD_DocketStatusDescription = SchemaRoot + WhsDocket.Schema.WD_DocketStatusDescription;
			public const string WD_TotalUnits = SchemaRoot + AutoWhsDocket.Schema.WD_TotalUnits;
			public const string WD_ArrivalDate = SchemaRoot + AutoWhsDocket.Schema.WD_ArrivalDate;
			public const string WD_FinalisedDate = SchemaRoot + AutoWhsDocket.Schema.WD_FinalisedDate;
			public const string WD_TotalPallets = SchemaRoot + AutoWhsDocket.Schema.WD_TotalPallets;
			public const string WD_WW_Whs = SchemaRoot + AutoWhsDocket.Schema.WD_WW_Whs;
			public const string WD_ETA = SchemaRoot + AutoWhsDocket.Schema.WD_ETA;
			public const string WD_CustomerReference = SchemaRoot + AutoWhsDocket.Schema.WD_CustomerReference;
			public const string WD_PK = SchemaRoot + "PK";
		}

		public const string SchemaRoot = "WhsReceive.";
		public const string SchemaPath = "WhsReceive+";

		public static string GetSchemaPath(string root, bool isRoot = false)
		{
			return isRoot ? SchemaRoot : SchemaPath + root;
		}

		#endregion

		#region Flyweight & Wrapped Object

		public WhsReceive WhsReceive { get; }

		public static TrackingWhsReceive GetTrackingWhsReceive(WhsReceive receive)
		{
			Argument.NotNull(receive, "WhsReceive");
			var result = receive.Factory.GetCachedValue("TrackingWhsReceive|" + receive.PK, GetNewTrackingWhsReceive);

			if (result.IsDeleted)
			{
				result.ResetState();
			}

			return result;

			TrackingWhsReceive GetNewTrackingWhsReceive()
			{
				var newReceive = new TrackingWhsReceive(receive);
				newReceive.RegisterEditableChildObject(receive);

				return newReceive;
			}
		}

		public ZGuid WD_PK { get { return WhsReceive.PK; } }

		#region Implementation of IWrappedBizOProvider

		public BusinessObject GetWrappedBizO()
		{
			return WhsReceive;
		}

		public string GetWrappedBindTo(string bindTo)
		{
			return GetSchemaPath(bindTo);
		}

		#endregion

		#endregion

		#region Implementation of IWhsReceiveWrapperStrategy

		WhsDocketValidation IWhsReceiveWrapperStrategy.GetNewValidation()
		{
			return new TrackingWhsReceiveValidation(this);
		}

		WhsDocketLookups IWhsReceiveWrapperStrategy.GetNewLookups()
		{
			return new TrackingWhsReceiveLookups(this);
		}

		#endregion

		#region Helper Methods

		public static TrackingWhsReceive FromPKFilteredByContact(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, WhsDocketSchema.PK, pK, siteUser);
		}

		public static TrackingWhsReceive FromNumberFilteredByContact(BusinessObjectFactory factory, ZString number, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, WhsDocketSchema.WD_ExternalReference, number, siteUser);
		}

		protected static TrackingWhsReceive FromFieldFilteredByContact(BusinessObjectFactory factory, SchemaColumn column, object value, TrackingSiteUser siteUser)
		{
			var result = TrackingHelper.Get(OrgRestrictionFilterFactory.LoadFilteredByContact<WhsReceive>(factory, column, value));
			if (result != null)
			{
				result.fSiteUser = siteUser;
			}
			return result;
		}

		public CustomLabelInfoList GetAdditionalInformationFields()
		{
			var provider = new WhsDocket.CustomLabelsProvider(WhsReceive);
			var customLabels = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, Factory);

			CustomLabelInfoList customLabelInfoList = new CustomLabelInfoList(typeof(TrackingWhsReceive), provider.ConfigOrgProvider.ConfigOrg, ResString.GetMultilingualString("5620f598-f3ff-4136-9db5-7f0c05114680", "the client of the warehouse"), Factory);

			foreach (CustomLabelInfo field in customLabels)
			{
				if (field.IsEnabled && field.LabelName.StartsWith("WhsDocket.")) // Its an identifier
				{
					customLabelInfoList.Add(field);
				}
			}

			return customLabelInfoList;
		}

		#endregion

		#region Inventory

		[ChildEditable(true)]
		public TrackingWhsInventoryCollection Inventory
		{
			get
			{
				if (inventory == null)
				{
					inventory = (TrackingWhsInventoryCollection)WhsReceive.Inventory;
					inventory.Sort(WhsInventoryView.Schema.WI_LineNo);
				}
				return inventory;
			}
		}
		TrackingWhsInventoryCollection inventory;

		WhsInventoryViewCollection IWhsReceiveWrapperStrategy.GetNewWhsInventoryCollection()
		{
			return new TrackingWhsInventoryCollection(Factory, this);
		}

		#endregion

		#region Lines

		[ChildEditable(true)]
		public TrackingWhsReceiveLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new TrackingWhsReceiveLineCollection(this);
					lines.Load();
					lines.Sort(AutoWhsDocketLine.Schema.WE_LineNo);
				}
				return lines;
			}
		}

		TrackingWhsReceiveLineCollection lines;

		#endregion

		#region Properties

		public bool CanCancelDocket
		{
			get { return WhsReceive.CanCancel() == ZString.Empty; }
		}

		public bool CanEdit
		{
			get
			{
				return WhsReceive.WD_DocketStatus == DocketStatus.Codes.Entered ||
				  WhsReceive.WD_DocketStatus == DocketStatus.Codes.New;
			}
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

		#region IBizOChangesEmailNotification Members

		ZGuid IBizOChangesEmailNotification.PK
		{
			get { return WhsReceive.PK; }
		}

		BusinessObjectFactory IBizOChangesEmailNotification.Factory
		{
			get { return WhsReceive.Factory; }
		}

		bool IBizOChangesEmailNotification.IsInDatabase
		{
			get { return WhsReceive.IsInDatabase; }
		}

		bool IBizOChangesEmailNotification.IsDeleted
		{
			get { return WhsReceive.IsDeleted; }
		}

		bool IBizOChangesEmailNotification.HasChanges
		{
			get { return WhsReceive.HasChanges; }
		}

		ZString IBizOChangesEmailNotification.Number
		{
			get { return WhsReceive.WD_DocketID; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get { return WhsReceive.Warehouse != null ? WhsReceive.Warehouse.RelatedCompanyBranch : null; }
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup; }
		}

		public ZBool IsCancelled
		{
			get { return WhsReceive.IsCancelled; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("34cc3716-489a-54fa-c5d0-471f2d5166e1", "Warehouse Receive {0}", WhsReceive.WD_DocketID); }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get { return WhsReceive.Client; }
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			var staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.WarehouseServices);
			var staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
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
			get { return ControllerIDs.WhsReceive; }
		}

		#region Email Reporting

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Email should be in English, no need to be translated")]
		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("3de8234b-4819-4aeb-aa20-0e5e37a84b8f", "Warehouse"), (WhsReceive.Warehouse != null) ? WhsReceive.Warehouse.WW_WarehouseName : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d51b87e3-ffe2-4a96-ae11-8a728e09aaf7", "Receive Ref #"), WhsReceive.WD_ExternalReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("9deea177-cf04-4e09-b30f-009af26d369b", "Customer Ref #"), WhsReceive.WD_CustomerReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0bb4682d-80bf-43f2-96ff-95d0ffac303a", "Booking Date"), WhsReceive.WD_BookingDate);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("12555bc5-8574-48b3-bbab-da6a15c0d1a6", "ETA"), WhsReceive.WD_ETA);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("16c55e20-0e06-43bb-ab92-4b7bc6de8986", "Total Units"), WhsReceive.WD_TotalUnits);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("68d100de-69c3-4698-be23-c79540790b35", "Total Pallets"), WhsReceive.WD_TotalPallets);
			AddContainersForEmailReporting(state, WhsReceive.Containers);
			AddInventoryForEmailReporting(state, WhsReceive.Lines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		void AddContainersForEmailReporting(DataState state, WhsDocketContainerCollection collection)
		{
			int i = 1;
			foreach (WhsDocketContainer container in collection)
			{
				var value = GenerateContainerDetailsForEmailReporting(container);

				var propertyName = ResString.GetMultilingualString("0ec4629b-6cdb-4672-9e57-fc46998c9041", "Container {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		public MultilingualString GenerateContainerDetailsForEmailReporting(WhsDocketContainer container)
		{
			var value = MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("aa37651f-562d-4a0d-83db-1590c28842fb", "Container #: {0}", container.WC_ContainerNum),
				ResString.GetMultilingualString("13f514bb-7746-461b-9903-e26e4fdbb479", "Seal #: {0}", container.WC_SealNum));

			if (container.Lookups.RefContainers.FindByPK(container.WC_RC) is RefContainer refContainer)
			{
				value = MultilingualString.Join(System.Environment.NewLine, value,
					ResString.GetMultilingualString("7a0520db-639e-4786-b0f4-494d16c7d9c1", "Type: {0}", refContainer.RC_ContainerType));
			}

			value = MultilingualString.Join(System.Environment.NewLine, value,
				ResString.GetMultilingualString("dd8080b6-1c30-4f49-9b27-53ea359c93ce", "Palletized: {0}", BoolToYesNo(container.WC_IsPalletised)),
				ResString.GetMultilingualString("8d7e83c9-f144-445f-aac5-ca00435f3ea8", "Chargeable: {0}", BoolToYesNo(container.WC_IsChargeable)),
				ResString.GetMultilingualString("60305077-83fc-41c4-8f96-b0276a947f86", "Items: {0}", container.WC_ItemCount),
				ResString.GetMultilingualString("b5b572d3-d92f-4f84-8e6a-3967aa091191", "Pallets: {0}", container.WC_PalletCount));

			return value;
		}

		MultilingualString BoolToYesNo(bool input)
		{
			return input ? ResString.GetMultilingualString("a8318707-8acc-4c2d-8805-62b0a5b1857b", "yes") : ResString.GetMultilingualString("f56eac63-7c14-4998-900e-f45a1392ec5a", "no");
		}

		void AddInventoryForEmailReporting(DataState state, WhsReceiveLineCollection collection)
		{
			int i = 1;
			foreach (WhsReceiveLine receiveLine in collection)
			{
				var value = GenerateInventoryDetailsForEmailReporting(receiveLine);

				var propertyName = ResString.GetMultilingualString("30bb988a-507f-4e8d-8e96-8129878e8cc7", "Line {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		public MultilingualString GenerateInventoryDetailsForEmailReporting(WhsReceiveLine receiveLine)
		{
			var value = MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("9d37c3d0-89d3-48a9-a376-940cd7fa677b", "Product: {0}", receiveLine.ProductCode),
				ResString.GetMultilingualString("905db58f-0518-4105-931b-7f0c59376240", "Packs: {0}", receiveLine.WE_PackQuantity),
				ResString.GetMultilingualString("e0242544-8ba6-4fb5-8ded-6174ad85897a", "Packs UQ: {0}", receiveLine.WE_F3_NKPackType),
				ResString.GetMultilingualString("a44585ae-5bea-4a46-a240-4bc6b6ccae2a", "Quantity: {0}", receiveLine.WE_StockOnHand));

			if (receiveLine.Docket != null && receiveLine.Docket.Client != null)
			{
				value = MultilingualString.Join(System.Environment.NewLine, value,
					MultilingualString.Join(": ", receiveLine.Docket.Client.PartAttributeManager.PartAttributeName1, (NoResString)receiveLine.WE_PartAttrib1),
					MultilingualString.Join(": ", receiveLine.Docket.Client.PartAttributeManager.PartAttributeName2, (NoResString)receiveLine.WE_PartAttrib2),
					MultilingualString.Join(": ", receiveLine.Docket.Client.PartAttributeManager.PartAttributeName3, (NoResString)receiveLine.WE_PartAttrib3),
					MultilingualString.Join(": ", ResString.GetMultilingualString("02acbb17-394a-4c57-8433-1f2947e6704a", "Serial Number"), (NoResString)receiveLine.WE_SerialNumber));
			}

			return value;
		}

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
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
			get { return WhsReceive.PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				var result = new List<ZGuid>();

				foreach (var bookingPK in TransportBookingLoader.GetBookingPKs(WhsReceive))
				{
					result.Add(bookingPK);
				}

				var shipment = RelatedShipment;
				if (shipment != null)
				{
					result.Add(shipment.PK);
				}

				return result;
			}
		}

		BusinessObject RelatedShipment
		{
			get
			{
				if (relatedShipment == null && WhsReceive.WD_DocketSubType == ReceiveType.Codes.Receipt)
				{
					var query = new ZQuery();
					query.AddToFilter(WhsDocketJobPivotSchema.WV_WD_Docket, WhsReceive.PK);
					query.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, WhsReceive.WD_DocketType);

					var pivot = Factory.LoadTop1<IWhsDocketJobPivot>(query);
					return pivot != null ? Factory.Load(ObjectFactory.GetType<ICommonShipment>(), pivot.WV_ParentId) : null;
				}
				return relatedShipment;
			}
		}

		readonly BusinessObject relatedShipment;

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)WhsReceive).DocManagerInfo;
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

		#region UserEditable & Visible Notes

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
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

		protected WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
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

		#region ITransactionSupport Members

		public ZString Reference
		{
			get { return WhsReceive.WD_DocketID; }
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
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(WhsReceive)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(WhsReceive, this, true)); }
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
					return (new UpdateableMilestoneEventsHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.WarehouseReceiveMilestoneEventUpdates.Value, WebParties);
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
					webParties.Add(WebPartyType.Client, WhsReceive.Client);
					webParties.Add(WebPartyType.Transport, WhsReceive.TransportCo);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		public override void Delete()
		{
			base.Delete();

			if (!WhsReceive.IsDeleted)
			{
				WhsReceive.Delete();
			}
		}

		public IBusiness TemplateCopy()
		{
			return TrackingHelper.Get(((ITemplateCopyable)WhsReceive).TemplateCopy() as WhsReceive);
		}

		public override bool IsInDatabase
		{
			get { return WhsReceive.IsInDatabase; }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges || WhsReceive.HasChanges; }
			set { base.HasChanges = WhsReceive.HasChanges = value; }
		}

		protected override BusinessObject LogsAndNotesTarget { get { return WhsReceive; } }

		#region Cloning support

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsReceive)WhsReceive.Clone();

			return new TrackingWhsReceive(clone);
		}

		#endregion
	}
}
