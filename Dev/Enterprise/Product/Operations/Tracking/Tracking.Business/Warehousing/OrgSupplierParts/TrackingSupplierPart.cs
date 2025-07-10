using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class TrackingSupplierPart : NonPersistentBusinessObject, IObsoleteValidation, IWebDocumentsWithUploadSupport, IBizOChangesEmailNotification
	{
		public TrackingSupplierPart(BusinessObjectFactory factory, OrgSupplierPart part)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (part == null)
			{
				throw new ArgumentNullException(nameof(part));
			}

			fProduct = WhsProduct.GetWhsProduct(part);
		}

		public WhsProduct Product
		{
			get { return fProduct; }
		}

		readonly WhsProduct fProduct;

		public OrgSupplierPart Part
		{
			get { return Product.Parent; }
		}

		public IeDoc ProductImage
		{
			get { return fProduct.ProductImage; }
		}

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

		public override void Delete()
		{
			throw new NotSupportedException();
		}

		#region Helper Methods

		public static TrackingSupplierPart FromPKFilteredByContact(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			ZQuery filter = new ZQuery(OrgSupplierPartSchema.PK, pK);
			return FilteredByContact(factory, filter, siteUser);
		}

		public static TrackingSupplierPart FromPKFilteredByRelatedOrder(BusinessObjectFactory factory, ZGuid productPK, ZGuid orderPK, TrackingSiteUser siteUser)
		{
			TrackingSupplierPart result = null;

			TrackingOrder order = TrackingOrder.FromPKFilteredByContact(factory, orderPK, siteUser);

			if (order != null)
			{
				foreach (OrderLine line in order.OrderLines)
				{
					if (line.Product != null && line.Product.PK == productPK)
					{
						result = new TrackingSupplierPart(factory, line.Product);
						result.SiteUser = siteUser;
						break;
					}
				}
			}

			return result;
		}

		static TrackingSupplierPart FilteredByContact(BusinessObjectFactory factory, ZQuery filter, TrackingSiteUser siteUser)
		{
			TrackingSupplierPart result = null;
			if (siteUser != null && siteUser.IsLoggedIn)
			{
				filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP));
				filter.IgnoreActiveFilter = true;

				OrgSupplierPart part = factory.LoadTop1<OrgSupplierPart>(filter);
				if (part != null)
				{
					result = new TrackingSupplierPart(factory, part);
					result.SiteUser = siteUser;
				}
			}
			return result;
		}

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
			get { return Part.PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)Product.Parent).DocManagerInfo;
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
			get
			{
				return Part == null ? ZString.Empty : Part.HumanReadableName;
			}
		}

		ZString IBizOChangesEmailNotification.Number
		{
			get { return Part == null ? ZString.Empty : Part.OP_PartNum; }
		}

		OrgContact IBizOChangesEmailNotification.LoggedInContact
		{
			get { return this.LoggedInContact; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return Part == null ? ZBool.False : (ZBool)Part.IsCancelled; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get { return GlbBranch.FindControllingBranchWithFallBackToAnyCompany(((IBizOChangesEmailNotification)this).RelatedOrg); }
		}

		ZArchitecture.Environment.GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.WebWarehouseProductsNotificationEmailGroup; }
		}

		BusinessObjectFactory IBizOChangesEmailNotification.Factory
		{
			get { return Factory; }
		}

		bool IBizOChangesEmailNotification.IsInDatabase
		{
			get { return Part?.IsInDatabase ?? false; }
		}

		bool IBizOChangesEmailNotification.IsDeleted
		{
			get { return Part?.IsDeleted ?? false; }
		}

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("770c7b45-86d9-4269-bded-cb53d0d3200e", "Part Number"), Part == null ? ZString.Empty : Part.OP_PartNum);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
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
			get { return ControllerIDs.Customs.SupplierPart; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				OrgHeader result = null;
				if (Part != null)
				{
					if (Part.AllOwners.Length == 1)
					{
						result = Owner;
					}
					else
					{
						result = (LoggedInContact == null ? null : LoggedInContact.ParentOrg);
					}
				}
				return result;
			}
		}

		OrgHeader Owner
		{
			get
			{
				OrgHeader result = null;
				if (Part != null)
				{
					foreach (OrgPartRelation relation in Part.RelatedOrganisations)
					{
						if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
								relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
						{
							if (result == null)
							{
								result = relation.Organisation;
							}
							else
							{
								result = null;
								break;
							}
						}
					}
				}
				return result;
			}
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.WarehouseServices);
			GlbStaff servicesStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (servicesStaff != null)
			{
				return servicesStaff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.WebWarehouseProductsNotificationStaffRoles; }
		}

		ZArchitecture.Environment.CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.WebWarehouseProductsNotificationOptions; }
		}

		#endregion
	}
}
