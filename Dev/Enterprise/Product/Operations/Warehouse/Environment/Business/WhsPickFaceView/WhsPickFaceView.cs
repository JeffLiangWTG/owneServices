using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	[GlowInterfaceReference("IWhsPickFace")]
	public class WhsPickFaceView : AutoWhsPickFaceView, ICanDelete
	{
		public WhsPickFaceView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}

		[List("Lookups.Clients")]
		public override ZGuid WPV_OH
		{
			get => base.WPV_OH;
			set => base.WPV_OH = value;
		}

		[List("Lookups.Parts")]
		public override ZGuid WPV_OP
		{
			get => base.WPV_OP;
			set => base.WPV_OP = value;
		}

		[List("Lookups.Warehouses")]
		public override ZGuid WPV_WW_Whs
		{
			get => base.WPV_WW_Whs;
			set => base.WPV_WW_Whs = value;
		}

		[List("Lookups.Locations")]
		public override ZGuid WPV_WL
		{
			get => base.WPV_WL;
			set => base.WPV_WL = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("3CDB449F-06FB-4235-A04D-46F92CF861F1", "Pick Face");
		public OrgHeader Client => Factory.Load<OrgHeader>(WPV_OH);
		public OrgSupplierPart SupplierPart => Factory.Load<OrgSupplierPart>(WPV_OP);
		public WhsLocation Location => Factory.Load<WhsLocation>(WPV_WL);
		public ZBool IsPickFaceAssigned => !WPV_WF.IsEmpty;
		public ZBool HasStockOrPendingTransactions => WPV_TotalQuantity > 0m || WPV_Incoming > 0m;
		public ZBool ReplenishMultipleTransferCanFit => WPV_ReplenishMaximum >= WPV_ReplenishmentMultiple + WPV_TotalQuantity + WPV_Incoming;

		// Tested in PickFacesFilterControl.cs
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() =>
			new WhsPickFaceViewFetchStrategy(this);
	}
}
