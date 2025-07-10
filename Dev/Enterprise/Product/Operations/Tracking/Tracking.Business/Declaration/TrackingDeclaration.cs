using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides access to tracking related declarations.
	/// </summary>
	public partial class TrackingDeclaration : NonPersistentBusinessObject,
		IObsoleteValidation,
		IWebDocumentsWithUploadSupport,
		ITransactionSupport,
		IBizOChangesEmailNotification,
		IEventReferenceProvider,
		IWebUserVisibleNotesSupport
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrderReference = "OrderReference";
			public const string PickupAddressAsText = "PickupAddressAsText";
			public const string DeliveryAddressAsText = "DeliveryAddressAsText";
			public const string DateAtOriginWithSuppression = "DateAtOriginWithSuppression";
			public const string ExportDateWithSuppression = "ExportDateWithSuppression";
			public const string DateOfArrivalWithSuppression = "DateOfArrivalWithSuppression";
			public const string DateOfFirstArrivalWithSuppression = "DateOfFirstArrivalWithSuppression";
			public const string FolioWithSuppression = "FolioWithSuppression";
			public const string Declaration = "Declaration";
			public const string ShipmentType = "ShipmentType";
		}
		#endregion

		public TrackingDeclaration(BaseJobDeclaration jobDeclaration)
			: this(jobDeclaration, null)
		{
		}

		public TrackingDeclaration(BaseJobDeclaration jobDeclaration, TrackingSiteUser siteUser)
			: base(jobDeclaration.Factory)
		{
			fDeclaration = jobDeclaration;
			fSiteUser = siteUser;
			RegisterEditableChildObject(fDeclaration);
		}

		public static TrackingDeclaration FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid declarationPK, TrackingSiteUser siteUser)
		{
			TrackingDeclaration result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				ZQuery filter = new ZQuery(JobDeclarationSchema.PK, declarationPK);
				if (!siteUser.IsShipmentQuickViewUser)
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>());
				}
				filter.IgnoreActiveFilter = true;
				BaseJobDeclaration jobDeclaration = (BaseJobDeclaration)factory.LoadTop1(typeof(BaseJobDeclaration), filter);
				if (jobDeclaration != null)
				{
					result = new TrackingDeclaration(jobDeclaration, siteUser);
				}
			}
			return result;
		}

		public RoutingCollection TransportsIncludingRelated
		{
			get
			{
				return Declaration.IsStandAlone ? Declaration.TransportsIncludingRelated : Declaration.Shipment.TransportsIncludingRelated;
			}
		}

		public ZString ShipmentType
		{
			get { return Declaration != null && Declaration.Shipment != null ? Declaration.Shipment.JS_ShipmentType : ZString.Empty; }
		}

		public ZPropertyInfo ShipmentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentType); }
		}

		#region Declaration

		public BaseJobDeclaration Declaration
		{
			get
			{
				return fDeclaration;
			}
		}
		readonly BaseJobDeclaration fDeclaration;

		#endregion

		#region StorageDate

		public ZDateTime StorageDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo StorageDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.StorageDate); }
		}

		#endregion

		#region VisibleNotes

		public IStmNoteParent NotesParentBO
		{
			get { return (IStmNoteParent)Declaration.Shipment ?? Declaration; }
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

		bool IWebUserVisibleNotesSupport.ShowAgentNotes
		{
			get { return false; }
		}

		#endregion

		#region OrderReference

		public ZString OrderReference
		{
			get
			{
				ZString result = Declaration.DocsAndCartage.JP_OrderItemsAsString;

				if (result.IsEmpty && Declaration.AttachedOrders.Count > 0)
				{
					foreach (var attachedOrder in Declaration.AttachedOrders)
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

		#region Documents

		public DocumentSupport DocumentHelper
		{
			get
			{
				return fDocumentHelper ?? (fDocumentHelper = new DocumentSupport(this));
			}
		}
		DocumentSupport fDocumentHelper;

		public ZGuid DocParentPK
		{
			get { return (Declaration != null) ? Declaration.PK : ZGuid.Empty; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();
				if (Declaration != null)
				{
					foreach (BusinessObject entryHeader in Declaration.CustomsEntryHeaders)
					{
						result.Add(entryHeader.PK);
					}
					foreach (BusinessObject invoice in Declaration.Invoices)
					{
						result.Add(invoice.PK);
					}
					foreach (var bookingPK in TransportBookingLoader.GetBookingPKs(Declaration))
					{
						result.Add(bookingPK);
					}
					var landedCostingHeader = Declaration.LandedCostHeaderForDocuments;
					if (landedCostingHeader != null)
					{
						result.Add(landedCostingHeader.PK);
					}
					if (Declaration.Shipment != null)
					{
						result.Add(Declaration.Shipment.PK);
					}
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

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)Declaration).DocManagerInfo;
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
				webParties = new WebPartyTypeOrgPairCollection();

				webParties.Add(WebPartyType.Supplier, Declaration.Supplier);
				webParties.Add(WebPartyType.Forwarder, Declaration.Forwarder);
				webParties.Add(WebPartyType.Carrier, Declaration.ShippingLine);
				webParties.Add(WebPartyType.Importer, Declaration.Importer);
				if (Declaration is Customs.US.Business.JobDeclaration)
				{
					webParties.Add(WebPartyType.UltimateConsignee, ((Customs.US.Business.JobDeclaration)Declaration).ConsigneeOrgAddress);
					webParties.Add(WebPartyType.ExternalBroker, ((Customs.US.Business.JobDeclaration)Declaration).ExternalBroker);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region LoggedInOrganisation

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
		}

		#endregion

		#region PickupAddressAsText

		public ZString PickupAddressAsText
		{
			get { return (Declaration != null) ? Declaration.SupplierPickupAddress.AddressAsASingleLine : ZString.Empty; }
		}

		public ZPropertyInfo PickupAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAddressAsText); }
		}

		#endregion

		#region DeliveryAddressAsText

		public ZString DeliveryAddressAsText
		{
			get { return (Declaration != null) ? Declaration.ImporterDeliveryAddress.AddressAsASingleLine : ZString.Empty; }
		}

		public ZPropertyInfo DeliveryAddressAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAddressAsText); }
		}

		#endregion

		#region ITransactionSupport Members

		public ZString Reference
		{
			get
			{
				return (Declaration != null) ? Declaration.JE_DeclarationReference : ZString.Empty;
			}
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

		#endregion

		#region Com Invoices

		public InvoiceHeaderActiveCollection Invoices
		{
			get { return Declaration.Invoices; }
		}

		#endregion

		#region Suppress Flight Details

		#region DateAtOrigin

		public ZDateTime DateAtOriginWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_DateAtOrigin, Declaration, SuppressFields.DeclarationDateAtOrigin) : ZDateTime.Empty; }
		}

		public ZPropertyInfo DateAtOriginWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DateAtOriginWithSuppression); }
		}

		#endregion

		#region ExportDate

		public ZDateTime ExportDateWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_ExportDate, Declaration, SuppressFields.DeclarationExportDate) : ZDateTime.Empty; }
		}

		public ZPropertyInfo ExportDateWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ExportDateWithSuppression); }
		}

		#endregion

		#region DateOfArrivalWithSuppression

		public ZDateTime DateOfArrivalWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_DateAtFinalDestination, Declaration, SuppressFields.ATA) : ZDateTime.Empty; }
		}

		public ZPropertyInfo DateOfArrivalWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfArrivalWithSuppression); }
		}

		#endregion

		#region DateOfFirstArrivalWithSuppression

		public ZDateTime DateOfFirstArrivalWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_DateOfFirstArrival, Declaration, SuppressFields.ATA) : ZDateTime.Empty; }
		}

		public ZPropertyInfo DateOfFirstArrivalWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfFirstArrivalWithSuppression); }
		}

		#endregion

		#region FolioWithSuppression

		public ZString FolioWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_Folio, Declaration, SuppressFields.DeclarationFolio) : ZString.Empty; }
		}

		public ZPropertyInfo FolioWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.FolioWithSuppression); }
		}

		#endregion

		#endregion

		#region IBizOChangesEmailNotification Members

		ZGuid IBizOChangesEmailNotification.PK
		{
			get
			{
				return DocParentPK;
			}
		}

		ZString IBizOChangesEmailNotification.HumanReadableName
		{
			get
			{
				return this.HumanReadableName;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Declaration == null ? ZString.Empty : Declaration.HumanReadableName; }
		}

		ZString IBizOChangesEmailNotification.Number
		{
			get { return Declaration == null ? ZString.Empty : Declaration.JE_DeclarationReference; }
		}

		OrgContact IBizOChangesEmailNotification.LoggedInContact
		{
			get { return this.LoggedInContact; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return Declaration == null ? ZBool.False : Declaration.JE_IsCancelled; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				OrgHeader loggedInOrg = LoggedInContact != null ? LoggedInContact.ParentOrg : null;
				OrgHeader relatedOrg = ((IBizOChangesEmailNotification)this).RelatedOrg ?? loggedInOrg;
				RefUNLOCO closestPort = relatedOrg != null ? relatedOrg.ClosestPort : null;

				return
					GlbBranch.FindControllingBranchWithFallBackToAnyCompany(relatedOrg) ??
					GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Declaration.Origin) ??
					GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, closestPort);
			}
		}

		ZArchitecture.Environment.GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.WebDeclarationNotificationEmailGroup; }
		}

		BusinessObjectFactory IBizOChangesEmailNotification.Factory
		{
			get { return Factory; }
		}

		bool IBizOChangesEmailNotification.IsInDatabase
		{
			get { return Declaration != null && Declaration.IsInDatabase; }
		}

		bool IBizOChangesEmailNotification.IsDeleted
		{
			get { return Declaration != null && Declaration.IsDeleted; }
		}

		bool IBizOChangesEmailNotification.HasChanges
		{
			get { return this.HasChanges || (Declaration != null && Declaration.HasChanges); }
		}

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("beb98ca5-a063-43a7-82b4-5baf0f7be44e", "Declaration Reference"), Declaration == null ? ZString.Empty : Declaration.JE_DeclarationReference); // GetMultilingualString method requires Engligh Text as parameter
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
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

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				return Declaration == null ? null : (Declaration.IsImport ? Declaration.Importer : Declaration.Supplier);
			}
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, Declaration.IsImport ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export, Declaration.IsAir ? OrgStaffAssignmentsCollection.AirSea.Air : OrgStaffAssignmentsCollection.AirSea.Sea);
			GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
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
			get { return WebDataRegistry.Instance.WebDeclarationNotificationStaffRoles; }
		}

		ZArchitecture.Environment.CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.WebDeclarationNotificationOptions; }
		}

		#endregion
	}
}
