using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides access to tracking related shipment details
	/// </summary>
	public partial class TrackingShipment : ForwardingShipment,
		IWebDocumentsWithUploadSupport,
		ITransactionSupport,
		IWebUserVisibleNotesSupport,
		IBizOChangesEmailNotification,
		IEventReferenceProvider
	{
		#region Schema

		public abstract new class Schema : CommonShipment.Schema
		{
			public const string ReceivedDate = "ReceivedDate";
			public const string OrderReference = "OrderReference";
			public const string PickupAddressAsText = "PickupAddressAsText";
			public const string DeliveryAddressAsText = "DeliveryAddressAsText";
			public const string AvailableDate = "AvailableDate";
			public const string LoadETDWithSuppression = "LoadETDWithSuppression";
			public const string DischargeETAWithSuppression = "DischargeETAWithSuppression";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";
			public const string LastTransportDischargePort = "LastTransportDischargePort";
			public const string FirstTransportLoadPort = "FirstTransportLoadPort";
			public const string AvailableAtAddressAsText = "AvailableAtAddressAsText";
			public const string MasterShipmentNum = "MasterShipmentNum";
			public const string ShipmentType = "ShipmentType";
		}

		#endregion

		public TrackingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region FromNumber

		public static TrackingShipment FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid shipmentPK, TrackingSiteUser siteUser)
		{
			TrackingShipment result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				ZQuery filter = new ZQuery(JobShipmentSchema.PK, shipmentPK);
				if (!siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
				}
				filter.IgnoreActiveFilter = true;
				result = (TrackingShipment)factory.LoadTop1(typeof(TrackingShipment), filter);
				if (result != null)
				{
					result.SiteUser = siteUser;
				}
			}
			return result;
		}

		#endregion FromNumber

		#region Related Business Objects

		#region ContainersOnConsols

		public IEnumerable<CommonContainer> ContainersOnConsols
		{
			get
			{
				return Containers.Where(c => AllMasterConsols.Any(m => m.PK == c.JC_JK));
			}
		}

		#endregion

		#region Consols

		public new TrackingConsolManyToManyCollection Consols
		{
			get { return (TrackingConsolManyToManyCollection)base.Consols; }
		}

		protected override ConsolCollection GetNewConsolCollection()
		{
			return new TrackingConsolManyToManyCollection(this);
		}

		#endregion Consols

		#region CoLoadShipments

		[List("Lookups.CoLoadShipment_List")]
		public new CoLoadTrackingShipmentCollection CoLoadShipments
		{
			get { return (CoLoadTrackingShipmentCollection)base.CoLoadShipments; }
		}

		protected override CoLoadShipmentCollection GetNewCoLoadShipmentCollection()
		{
			return new CoLoadTrackingShipmentCollection(this, Factory);
		}

		#endregion

		#region PackLines

		[ChildEditable(true)]
		public new TrackingPackLineCollection OuterPackLines
		{
			get { return (TrackingPackLineCollection)base.OuterPackLines; }
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new TrackingPackLineCollection(this);
		}

		#endregion PackLines

		#region AttachedOrders

		[ChildEditable(true)]
		public new TrackingOrderCollection AttachedOrders
		{
			get { return (TrackingOrderCollection)base.AttachedOrders; }
		}

		protected override OrderCollection GetAttachedOrdersCollection()
		{
			return new TrackingOrderCollection(Factory, this);
		}

		#endregion AttachedOrders

		#region PossibleOrdersForAttachment_List

		public override OrderCollection PossibleOrdersForAttachment_List
		{
			get { return TrackingOrder.GetOrdersToBeAttachedForWebModule(Factory, this); }
		}

		#endregion

		#endregion Related Business Objects

		#region Property wrappers

		public new ZString ShipmentType
		{
			get { return JS_ShipmentType; }
		}

		public ZPropertyInfo ShipmentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentType); }
		}

		#region LoggedInOrganisation

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
		}

		#endregion

		#region Related documents

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (documentHelper == null)
				{
					documentHelper = new DocumentSupport(this);
				}
				return documentHelper;
			}
		}
		DocumentSupport documentHelper;

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();
				foreach (BusinessObject declaration in Declarations)
				{
					result.Add(declaration.PK);
					if (declaration is BaseJobDeclaration)
					{
						result.AddRange(new TrackingDeclaration((BaseJobDeclaration)declaration, SiteUser).DocRelatedPKs);
					}
				}
				foreach (ZGuid bookingPK in TransportBookingLoader.GetBookingPKs(this))
				{
					result.Add(bookingPK);
				}
				if (LocalConsol != null && LocalConsol.JK_AgentType == Core.Constants.AgentType.Direct)
				{
					result.Add(LocalConsol.PK);
				}
				foreach (Order order in AttachedOrders)
				{
					result.Add(order.PK);
				}
				var receive = RelatedWarehouseReceive;
				if (receive != null)
				{
					result.Add(receive.PK);
				}
				return result.Distinct().ToList();
			}
		}

		public TrackingSiteUser SiteUser
		{
			get { return fSiteUser; }
			set { fSiteUser = value; }
		}
		protected TrackingSiteUser fSiteUser;

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		#endregion

		#region VisibleNotes

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserVisibleNotes NotesHelper
		{
			get
			{
				if (notesHelper == null)
				{
					notesHelper = new WebUserVisibleNotes(this);
				}
				return notesHelper;
			}
		}

		WebUserVisibleNotes notesHelper;

		#endregion

		#region OrderReference

		public ZString OrderReference
		{
			get
			{
				ZString result = DocsAndCartage.JP_OrderItemsAsString;

				if (result.IsEmpty && AttachedOrders.Count > 0)
				{
					foreach (TrackingOrder attachedOrder in AttachedOrders)
					{
						if (!result.IsEmpty && !attachedOrder.JD_OrderNumber.IsEmpty)
						{
							result += ",";
						}
						result += attachedOrder.JD_OrderNumber;
					}
				}

				if (!result.IsEmpty)
				{
					result = result.Replace(",", ", ");
					result = result.Replace("  ", " ");
				}
				return result;
			}
		}

		public ZPropertyInfo OrderReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.OrderReference); }
		}

		#endregion

		/// <summary>
		/// returns a declaration. If more than one dec attached to a shipment, returns an import one
		/// </summary>
		/// <returns></returns>
		public BaseJobDeclaration LastDeclaration
		{
			get
			{
				BaseJobDeclaration result = null;
				foreach (BaseJobDeclaration dec in Declarations)
				{
					if (dec.JE_MessageType == Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import)
					{
						result = dec;
					}
					else if (result == null)
					{
						result = dec;
					}
				}
				return result;
			}
		}

		public BaseJobDeclaration FirstExportDeclaration
		{
			get
			{
				return Declarations.OfType<BaseJobDeclaration>().FirstOrDefault(jd => jd.JE_MessageType == Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export);
			}
		}

		public BaseJobDeclaration LastImportDeclaration
		{
			get
			{
				return Declarations.OfType<BaseJobDeclaration>().Reverse().FirstOrDefault(jd => jd.JE_MessageType == Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import);
			}
		}

		#endregion

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				webParties = new WebPartyTypeOrgPairCollection();
				webParties.Add(WebPartyType.Shipper, Consignor);
				webParties.Add(WebPartyType.Consignee, Consignee);
				if (Job != null)
				{
					webParties.Add(WebPartyType.LocalClient, Job.LocalCharges);
				}
				foreach (TrackingConsol consol in Consols)
				{
					webParties.Add(WebPartyType.SendingAgent, consol.SendingForwarder);
					webParties.Add(WebPartyType.ReceivingAgent, consol.ReceivingForwarder);
				}
				webParties.Add(WebPartyType.DeliveryAgent, DeliveryAgent);
				webParties.Add(WebPartyType.ImportBroker, ImportBroker);
				webParties.Add(WebPartyType.ExportBroker, ExportBroker);

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

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

		#region New fields

		#region RelatedShipments

		public CoLoadShipmentCollection RelatedShipments
		{
			get
			{
				if (relatedShipments == null)
				{
					relatedShipments = GetNewCoLoadShipmentCollection();
					if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
					{
						relatedShipments.Load(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
					}
				}
				return relatedShipments;
			}
		}
		CoLoadShipmentCollection relatedShipments;

		public ZString MasterShipmentNum
		{
			get
			{
				ZString result = CoLoadMasterShipment == null ? ZString.Empty : CoLoadMasterShipment.JS_UniqueConsignRef;
				return result;
			}
		}

		public ZPropertyInfo MasterShipmentNumInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.MasterShipmentNum);
			}
		}

		#endregion

		#region Transports

		public RoutingCollection RelatedTransportsInLegOrder
		{
			get
			{
				if (relatedTransportsInLegOrder == null)
				{
					relatedTransportsInLegOrder = TransportsIncludingRelated;
					relatedTransportsInLegOrder.Sort(MovementLegComparer.PortsAndDatesBased(relatedTransportsInLegOrder));
				}

				return relatedTransportsInLegOrder;
			}
		}

		RoutingCollection relatedTransportsInLegOrder;

		#endregion

		#region Pickup Address

		public ZString PickupAddressAsText
		{
			get
			{
				return (ConsignorPickupAddress == null) ? ZString.Empty : ConsignorPickupAddress.AddressAsASingleLine;
			}
		}

		public ZPropertyInfo PickupAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAddressAsText); }
		}

		#endregion

		#region DeliveryAddressAsText

		public ZString DeliveryAddressAsText
		{
			get
			{
				return (ConsigneeDeliveryAddress == null) ? ZString.Empty : ConsigneeDeliveryAddress.AddressAsASingleLine;
			}
		}

		public ZPropertyInfo DeliveryAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAddressAsText); }
		}

		#endregion

		#region Available Date

		public ZDateTime AvailableDate
		{
			get
			{
				return Core.Constants.ContainerModes.IsLCLType(JS_PackingMode)
					? DocsAndCartage.JP_LCLAvailable
					: DocsAndCartage.JP_FCLAvailable;
			}
		}

		public ZPropertyInfo AvailableDateInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableDate); }
		}

		#endregion

		#region AvailableAtAddressAsText

		public ZString AvailableAtAddressAsText
		{
			get
			{
				return ImportReleaseDepot != null ? ImportReleaseDepot.AddressAsASingleLine :
					ArrivalConsol != null && ArrivalConsol.UnpackDepotAddress != null ? ArrivalConsol.UnpackDepotAddress.AddressAsASingleLine : ZString.Empty;
			}
		}

		public ZPropertyInfo AvailableAtAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableAtAddressAsText); }
		}

		#endregion

		#region Storage Date

		public ZDateTime StorageDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Core.Constants.ContainerModes.IsLCLType(JS_PackingMode))
				{
					result = DocsAndCartage.JP_LCLStorageCommences;
					if (result.IsEmpty && ArrivalConsol != null)
					{
						result = ArrivalConsol.JK_DepotStorageDate;
					}
				}
				else
				{
					result = DocsAndCartage.JP_FCLStorageCommences;
					if (result.IsEmpty && ArrivalConsol != null)
					{
						result = ArrivalConsol.JK_CTOStorageDate;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo StorageDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.StorageDate); }
		}

		#endregion

		#region FirstTransportLoadPort

		public ZString FirstTransportLoadPort
		{
			get { return (TransportsInLegOrder.Count > 0) ? TransportsInLegOrder[0].JW_RL_NKLoadPort : ZString.Empty; }
		}

		public ZPropertyInfo FirstTransportLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.FirstTransportLoadPort); }
		}

		#endregion

		#region LastTransportDischargePort

		public ZString LastTransportDischargePort
		{
			get { return (TransportsInLegOrder.Count > 0) ? TransportsInLegOrder[TransportsInLegOrder.Count - 1].JW_RL_NKDiscPort : ZString.Empty; }
		}

		public ZPropertyInfo LastTransportDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.LastTransportDischargePort); }
		}
		#endregion

		#endregion

		#region Suppress Flight Details

		#region LoadETDWithSuppression

		public ZDateTime LoadETDWithSuppression
		{
			get
			{
				ZDateTime result = (DepartureConsol != null && DepartureConsol.Transports.Count > 0)
					? DepartureConsol.Transports.DepartureTransport.JW_ETD
					: ZDateTime.Empty;

				return Suppression.GetWebValue(result, this, SuppressFields.ETD);
			}
		}

		public ZPropertyInfo LoadETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.LoadETDWithSuppression); }
		}

		#endregion

		#region DischargeETAWithSuppression

		public ZDateTime DischargeETAWithSuppression
		{
			get
			{
				ZDateTime result = (ArrivalConsol != null && ArrivalConsol.Transports.Count > 0)
					? ArrivalConsol.Transports.ArrivalTransport.JW_ETA
					: ZDateTime.Empty;

				return Suppression.GetWebValue(result, this, SuppressFields.ETA);
			}
		}

		public ZPropertyInfo DischargeETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DischargeETAWithSuppression); }
		}

		#endregion

		#region ETDWithSuppression

		public ZDateTime ETDWithSuppression
		{
			get { return Suppression.GetWebValue(JS_E_DEP, this, SuppressFields.ETD); }
		}

		public ZPropertyInfo ETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETDWithSuppression); }
		}

		#endregion

		#region ETAWithSuppression

		public ZDateTime ETAWithSuppression
		{
			get { return Suppression.GetWebValue(JS_E_ARV, this, SuppressFields.ETA); }
		}

		public ZPropertyInfo ETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETAWithSuppression); }
		}

		#endregion

		#region Property overrides

		[RequiresSuppression]
		public override ZDateTime JS_E_DEP
		{
			get { return base.JS_E_DEP; }
			set { base.JS_E_DEP = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JS_E_ARV
		{
			get { return base.JS_E_ARV; }
			set { base.JS_E_ARV = value; }
		}

		#endregion

		#endregion

		#region ITransactionSupport Members

		public ZString Reference
		{
			get { return JS_UniqueConsignRef; }
		}

		public TrackingInvoiceLoader InvoiceLoader
		{
			get
			{
				if (invoiceLoader == null)
				{
					invoiceLoader = new TrackingInvoiceLoader(this);
				}

				return invoiceLoader;
			}
		}
		TrackingInvoiceLoader invoiceLoader;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TrackingShipmentFetchStrategy(this);
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();

				if (base.BusinessObjectsWithRelatedNotes.Length > 0)
				{
					result.AddRange(base.BusinessObjectsWithRelatedNotes);
				}

				return result.ToArray();
			}
		}

		public void EnableOnlyDeliveryConfirmationsValidationOnPreSave()
		{
			isOnlyDeliveryConfirmationsValidationEnabledOnPreSave = true;
		}
		bool isOnlyDeliveryConfirmationsValidationEnabledOnPreSave;

		protected override void RunPreSaveValidationCore()
		{
			if (isOnlyDeliveryConfirmationsValidationEnabledOnPreSave)
			{
				foreach (CommonPickupDeliveryConfirm confirm in DeliveryConfirms)
				{
					confirm.Validation.ValidateAll();
				}
			}
			else
			{
				base.RunPreSaveValidationCore();
			}
		}

		#region JS_HouseBill

		public override ZString JS_HouseBill
		{
			get
			{
				if (LocalConsol != null && LocalConsol.JK_AgentType == Core.Constants.AgentType.Direct)
				{
					return LocalConsol.JK_MasterBillNum;
				}

				return base.JS_HouseBill;
			}
			set
			{
				base.JS_HouseBill = value;
			}
		}

		#endregion

		#region IWebUserVisibleNotesSupport Members

		public bool ShowAgentNotes
		{
			get
			{
				bool showAgentNotes = LoggedInOrgIsTheAgentForThisJob;

				if (!showAgentNotes)
				{
					foreach (TrackingOrder order in AttachedOrders)
					{
						showAgentNotes = order.LoggedInOrgIsTheAgentForThisJob;
						if (showAgentNotes)
						{
							break;
						}
					}

					OrgHeader loggedInOrg = GetLoggedInOrgIncludingRelatedBizObjects();

					if (!showAgentNotes && loggedInOrg != null)
					{
						foreach (TrackingConsol consol in Consols)
						{
							showAgentNotes = consol.SendingForwarderPK == loggedInOrg.PK ||
								consol.ReceivingForwarderPK == loggedInOrg.PK;

							if (showAgentNotes)
							{
								break;
							}
						}
					}
				}

				return showAgentNotes;
			}
		}

		public bool LoggedInOrgIsTheAgentForThisJob
		{
			get
			{
				OrgHeader loggedInOrg = GetLoggedInOrgIncludingRelatedBizObjects();
				return loggedInOrg != null &&
						(DeliveryAgent != null && DeliveryAgent.PK == loggedInOrg.PK ||
						TranshipAgent != null && TranshipAgent.PK == loggedInOrg.PK);
			}
		}

		OrgHeader GetLoggedInOrgIncludingRelatedBizObjects()
		{
			OrgHeader result = LoggedInOrganisation;

			if (result == null && AttachedOrders.Count > 0)
			{
				foreach (TrackingOrder order in AttachedOrders)
				{
					if (order.LoggedInOrganisation != null)
					{
						result = order.LoggedInOrganisation;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region IBizOChangesEmailNotification Members

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				OrgHeader loggedInOrg = LoggedInContact != null ? LoggedInContact.ParentOrg : null;
				OrgHeader relatedOrg = ((IBizOChangesEmailNotification)this).RelatedOrg ?? loggedInOrg;
				RefUNLOCO closestPort = relatedOrg != null ? relatedOrg.ClosestPort : null;

				return
					GlbBranch.FindControllingBranchWithFallBackToAnyCompany(relatedOrg) ??
					GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Origin) ??
					GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, closestPort);
			}
		}

		ZArchitecture.Environment.GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.ShipmentNotificationEmailGroup; }
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.JobShipment; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				if (this.IsImport() && ConsigneeDeliveryAddress != null && !ConsigneeDeliveryAddress.E2_AddressOverride)
				{
					return ConsigneeDeliveryAddress.Organisation;
				}
				else if (ConsignorPickupAddress != null && !ConsignorPickupAddress.E2_AddressOverride)
				{
					return ConsignorPickupAddress.Organisation;
				}
				return null;
			}
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsCollection.Direction.Export,
				IsAir ? OrgStaffAssignmentsCollection.AirSea.Air : OrgStaffAssignmentsCollection.AirSea.Sea);
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

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.ShipmentNotificationStaffRoles; }
		}

		ZArchitecture.Environment.CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.ShipmentNotificationOptions; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return IsCancelled; }
		}

		#region Email Reporting

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (propertiesForEmailReporting == null)
				{
					propertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return propertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection propertiesForEmailReporting;

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			int i = 1;
			foreach (CommonPickupDeliveryConfirm confirmation in DeliveryConfirms)
			{
				var value = GenerateConfirmationDetailsForEmailReporting(confirmation);

				var propertyName = ResString.GetMultilingualString("90f4e52d-4854-493a-aeb0-cc764bf5d4d8", "Confirmation {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		protected MultilingualString GenerateConfirmationDetailsForEmailReporting(CommonPickupDeliveryConfirm confirmation)
		{
			var value = MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("e5bfe933-7876-4184-8752-876b8570a07c", "Pieces Delivered: {0}", confirmation.TotalDeliveredPackages),
				ResString.GetMultilingualString("00be8ede-8bb2-486b-9884-25d2ef882eec", "Actual Delivery: {0}", confirmation.EU_PickupDeliveryTime),
				ResString.GetMultilingualString("353eab3e-6282-4082-9ff0-db1d820251f7", "Received By: {0}", confirmation.EU_GoodsSignForBy),
				ResString.GetMultilingualString("b7e9aceb-f154-4b76-88c6-c8bf7bfd6470", "Requested By: {0}", confirmation.EU_RequestedPickupDeliveryTime),
				ResString.GetMultilingualString("9a668e94-09ac-47cf-8242-f0ab71ccff89", "Drop Mode: {0}", confirmation.EU_DropMode),
				ResString.GetMultilingualString("f76917ff-c623-4678-a84d-121cdc810ce7", "Notes: {0}", confirmation.EU_PickupDeliveryInstruction));

			if (confirmation.Divots.Count > 0)
			{
				value = MultilingualString.Join(System.Environment.NewLine, value,
					ResString.GetMultilingualString("222ceae9-4c14-4245-a64b-6c0281d2aad8", "LINES:"));

				int i = 1;
				foreach (CommonConfirmDivot divot in confirmation.Divots)
				{
					if (i > 1)
					{
						value = MultilingualString.Join("", value, (NoResString)System.Environment.NewLine);
					}
					value = MultilingualString.Join(System.Environment.NewLine, value,
						ResString.GetMultilingualString("356428eb-d1bc-4f66-8a1f-6d94fb241c18", "Line {0}", i++),
						ResString.GetMultilingualString("a911cc9f-597b-44b7-b48f-100a8c5f5541", "Packs: {0}", divot.J8_PackagesDelivered),
						ResString.GetMultilingualString("f49da73e-8f61-48f2-a38c-6f6d824ab908", "Weight: {0} {1}", divot.J8_DeliveryWeight, divot.PackLineWeightUnit),
						ResString.GetMultilingualString("b92d1807-1442-4839-8417-bff7a23b1f47", "Volume: {0} {1}", divot.J8_DeliveryVolume, divot.PackLineVolumeUnit));
				}
			}

			return value;
		}

		#endregion

		#endregion

		#region IWebDocumentsWithUploadSupport

		public new DocManagerInfo DocManagerInfo
		{
			get { return ((IDocManagerSupport)this).DocManagerInfo; }
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
			documentHelper = null;
		}

		#endregion

		#region ITemplateCopyable Members

		public override IBusiness TemplateCopy()
		{
			using (UseBookingOnlyCloningMode())
			{
				var newShipment = base.TemplateCopy() as TrackingShipment;
				if (newShipment != null)
				{
					newShipment.JS_A_BKD = ZDateTime.Now;
					newShipment.JS_IsBooking = true;
					newShipment.JS_IsForwardRegistered = false;
					newShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
					ClearLoadingMeters(newShipment);
				}

				return newShipment;
			}
		}

		void ClearLoadingMeters(TrackingShipment shipment)
		{
			foreach (TrackingPackLine packLine in shipment.OuterPackLines)
			{
				packLine.JL_LoadingMeters = 0;
			}

			shipment.JS_LoadingMeters = 0;
		}

		protected override ZBool ShouldCustomsDataBeCopiedCore
		{
			get { return false; }
		}

		#endregion

	}
}
