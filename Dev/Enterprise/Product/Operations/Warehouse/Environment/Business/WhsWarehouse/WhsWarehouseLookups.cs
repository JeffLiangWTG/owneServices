using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsWarehouseLookups : AutoWhsWarehouseLookups
	{
		public WhsWarehouseLookups(AutoWhsWarehouse parent)
			: base(parent)
		{
		}

		#region RelatedCompanyBranches

		public override GlbBranchCollection RelatedCompanyBranches
		{
			get
			{
				var relatedCompanyBranches = new ZQuery(GlbBranchSchema.GB_IsActive, true);
				relatedCompanyBranches.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

				return new GlbBranchCollection(Factory, relatedCompanyBranches);
			}
		}

		#endregion

		#region Organisations

		public virtual OrgHeaderCollection Organisations
		{
			get { return Factory.GetCachedValue("WhsWarehouseLookups|Organisations", () => new OrganisationsFindBoxCollection(Factory)); }
		}

		#endregion

		#region Printers

		public IBusinessObjectCollection Printers => WhsCommonLookups.GetPrintersList(Factory);

		#endregion

		#region WarehouseTypes

		public WarehouseTypes WarehouseTypes
		{
			get { return Factory.GetCachedValue("WhsWarehouseLookups|WarehouseTypes", () => new WarehouseTypes()); }
		}

		#endregion

		#region PhoneTypes

		public PhoneTypeList PhoneTypes
		{
			get { return Factory.GetCachedValue("WhsWarehouseLookups|PhoneTypes", () => new PhoneTypeList()); }
		}

		#endregion

		#region DockDoorLocations

		public WhsLocationCollection DockDoorLocations => Factory.GetCachedValue("WhsWarehouseLookups|DockDoorLocations|" + Parent.PK, () => new WhsLocationCollection(Parent, dockDoorLocationsOnly: true));

		#endregion

		#region LocationTypes

		public WhsLocationTypeCollection LocationTypes
		{
			get { return Factory.GetCachedValue("WhsLocationTypeCollection", () => new WhsLocationTypeCollection(Factory)); }
		}

		#endregion

		#region UNDGContacts

		public OrgContactDependentCollection UNDGContacts
		{
			get
			{
				var address = Parent.WarehouseAddress;
				var organisation = address != null ? address.Header : null;

				return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "WhsWarehouseLookups|UNDGContactList-{0}",  // Key used in Factory Cache
					organisation != null ? organisation.PK : ZGuid.Empty),
					() => organisation != null ? new OrgContactDependentCollection(organisation, Factory) : new OrgContactDependentCollection(Factory));
			}
		}

		#endregion

		#region DetailedTrackingMethods

		public DetailedTrackingMethod DetailedTrackingMethods
		{
			get { return Factory.GetCachedValue("WhsWarehouseLookups|DetailedTrackingMethods", () => new DetailedTrackingMethod()); }
		}

		#endregion

		#region Parent

		new protected WhsWarehouse Parent
		{
			get { return (WhsWarehouse)base.Parent; }
		}

		#endregion
	}
}
