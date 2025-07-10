using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DocAutoDeliveryContactViewer : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocAutoDeliveryContactViewer(OrgHeader organisation) : base(organisation.Factory)
		{
			this.Organisation = organisation;
		}

		public void GenerateAutoDeliveryContacts()
		{
			DocContacts.RemoveAll();
			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(Organisation, TransportMode, "", LocalPort, ForeignPort, RelatedParty);
			DocContacts.AddRange(new DocAutoDelivery().GetDeliveryContacts(MenuItem, deliveryFilter));
		}

		#region Bound Properties

		#region MenuItem

		protected ZGuid menuItemGuid;
		[List("MenuItems")]
		public ZGuid MenuItemGuid
		{
			get { return menuItemGuid; }
			set
			{
				SetNonPersistentPropertyValue(MenuItemGuidInfo, ref menuItemGuid, value);
				if (menuItemGuid.IsValid)
				{
					MenuItem = (StmMenuItem)Factory.Load(typeof(StmMenuItem), menuItemGuid);
					DocContactTypeInfo.RefreshBinding();
					if (!IsConsign)
					{
						RelatedPartyGuid = ZGuid.Empty;
						LocalPort = "";
						ForeignPort = "";
					}
				}
			}
		}

		public ZPropertyInfo MenuItemGuidInfo
		{
			get { return GetZPropertyInfo(nameof(MenuItemGuid)); }
		}

		public virtual void ValidateMenuItemGuid()
		{
			MenuItemGuidInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MenuItemGuidInfo);
			ListValidation.ErrorIfInvalidPK(MenuItemGuidInfo);
		}

		#endregion

		#region DocContactType

		[MaxLength(3)]
		public ZString DocContactType
		{
			get { return MenuItem != null ? MenuItem.SU_ContactType : new ZString(""); }
		}

		public ZPropertyInfo DocContactTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DocContactType)); }
		}

		#endregion

		#region TransportMode

		protected ZString transportMode;
		[List("TransportMode_List")]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				if (transportMode != value)
				{
					CheckMaximumLength(TransportModeInfo, value);
					SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				}
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(nameof(TransportMode)); }
		}

		public virtual void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TransportModeInfo);
		}

		#endregion

		#region LocalPort

		protected ZString localPort;
		[List("Locations")]
		[MaxLength(5)]
		public ZString LocalPort
		{
			get { return localPort; }
			set
			{
				if (localPort != value)
				{
					CheckMaximumLength(LocalPortInfo, value);
					SetNonPersistentPropertyValue(LocalPortInfo, ref localPort, value);
				}
			}
		}

		public ZPropertyInfo LocalPortInfo
		{
			get { return GetZPropertyInfo(nameof(LocalPort)); }
		}

		public virtual void ValidateLocalPort()
		{
			LocalPortInfo.ClearAllNotifications();
		}

		#endregion

		#region ForeignPort

		protected ZString foreignPort;
		[List("Locations")]
		[MaxLength(5)]
		public ZString ForeignPort
		{
			get { return foreignPort; }
			set
			{
				if (foreignPort != value)
				{
					CheckMaximumLength(ForeignPortInfo, value);
					SetNonPersistentPropertyValue(ForeignPortInfo, ref foreignPort, value);
				}
			}
		}

		public ZPropertyInfo ForeignPortInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignPort)); }
		}

		public virtual void ValidateForeignPort()
		{
			ForeignPortInfo.ClearAllNotifications();
		}

		#endregion

		#region RelatedPartyGuid

		protected ZGuid relatedPartyGuid;
		[List("RelatedParties")]
		public ZGuid RelatedPartyGuid
		{
			get { return relatedPartyGuid; }
			set
			{
				SetNonPersistentPropertyValue(RelatedPartyGuidInfo, ref relatedPartyGuid, value);
				if (relatedPartyGuid.IsValid)
				{
					RelatedParty = (OrgHeader)Factory.Load(typeof(OrgHeader), relatedPartyGuid);
				}
			}
		}

		public ZPropertyInfo RelatedPartyGuidInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedPartyGuid)); }
		}

		public virtual void ValidateRelatedPartyGuid()
		{
			RelatedPartyGuidInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(RelatedPartyGuidInfo);
		}

		#endregion

		#region DocContactTypeIsConsign

		public ZBool DocContactTypeIsConsign
		{
			get { return IsConsign; }
		}

		public ZPropertyInfo DocContactTypeIsConsignInfo
		{
			get { return GetZPropertyInfo(nameof(DocContactTypeIsConsign)); }
		}

		#endregion

		#endregion

		#region Unbound Properties

		protected ZBool IsConsign
		{
			get { return DocContactType == ContactType.Consignee.Code || DocContactType == ContactType.Consignor.Code; }
		}

		#endregion

		#region Collections

		#region MenuItems

		protected DocAutoDeliverStmMenuItemCollection fMenuItems;
		public DocAutoDeliverStmMenuItemCollection MenuItems
		{
			get
			{
				if (fMenuItems == null)
				{
					fMenuItems = new DocAutoDeliverStmMenuItemCollection(Factory);
				}
				return fMenuItems;
			}
		}

		#endregion

		#region UNLOCOs

		protected RefUNLOCOCollection fUNLOCOs;
		public RefUNLOCOCollection UNLOCOs
		{
			get
			{
				if (fUNLOCOs == null)
				{
					fUNLOCOs = new RefUNLOCOCollection(Factory);
				}
				return fUNLOCOs;
			}
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region RelatedParties

		public OrganisationsFindBoxCollection RelatedParties
		{
			get
			{
				if (MenuItem != null)
				{
					if (MenuItem.SU_ContactType == ContactType.Consignee.Code)
					{
						return new ConsignorCollection(Factory);
					}
					else if (MenuItem.SU_ContactType == ContactType.Consignor.Code)
					{
						return new ConsigneeCollection(Factory);
					}
				}
				return new OrganisationsFindBoxCollection(Factory);
			}
		}

		#endregion

		#region DocContacts

		protected DocDeliveryContactCollection fDocContacts;
		public DocDeliveryContactCollection DocContacts
		{
			get
			{
				if (fDocContacts == null)
				{
					fDocContacts = new DocDeliveryContactCollection(Factory);
				}
				return fDocContacts;
			}
		}

		#endregion

		#endregion

		#region CodeLists

		protected CodeDescriptionPairList fTransportMode_List;
		public CodeDescriptionPairList TransportMode_List
		{
			get
			{
				if (fTransportMode_List == null)
				{
					fTransportMode_List = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);
				}
				return fTransportMode_List;
			}
		}

		#endregion

		#region Implementation

		protected OrgHeader Organisation;
		protected OrgHeader RelatedParty;
		protected StmMenuItem MenuItem;

		#endregion
	}
}
