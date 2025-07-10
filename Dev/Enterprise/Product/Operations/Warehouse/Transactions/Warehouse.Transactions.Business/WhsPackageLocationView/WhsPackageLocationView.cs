using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsPackageLocationView : AutoWhsPackageLocationView
	{
		public WhsPackageLocationView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Package))]
		public override ZGuid WPK_KP_Package
		{
			get => base.WPK_KP_Package;
			set => base.WPK_KP_Package = value;
		}

		public PkgPackage Package => Factory.Load<PkgPackage>(WPK_KP_Package);

		[RelatedBusinessObject(nameof(Warehouse))]
		public override ZGuid WPK_WW_Whs
		{
			get => base.WPK_WW_Whs;
			set => base.WPK_WW_Whs = value;
		}

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WPK_WW_Whs);

		[RelatedBusinessObject(nameof(Location))]
		public override ZGuid WPK_WL_Location
		{
			get => base.WPK_WL_Location;
			set => base.WPK_WL_Location = value;
		}

		public WhsLocation Location => Factory.Load<WhsLocation>(WPK_WL_Location);

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}
	}
}
