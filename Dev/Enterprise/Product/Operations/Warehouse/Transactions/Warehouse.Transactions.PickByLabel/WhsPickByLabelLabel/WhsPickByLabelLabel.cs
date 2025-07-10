using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public partial class WhsPickByLabelLabel : AutoWhsPickByLabelLabel, IWhsPickByLabelLabel
	{
		public WhsPickByLabelLabel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public WhsPickByLabelJob PickByLabelJob => Factory.Load<WhsPickByLabelJob>(WTL_WTK_PickByLabelJob);

		public PkgPackage Package => Factory.Load<PkgPackage>(WTL_KP_Package);

		public ZGuid? DockDoorLocationPK => Pick?.DockDoorPK;

		WhsOrder Order => Package?.PackageJob?.ParentJob as WhsOrder;

		public WhsPick Pick => Order?.Pick;

		#endregion

		#region Properties

		[RelatedBusinessObject("PickByLabelJob")]
		public override ZGuid WTL_WTK_PickByLabelJob
		{
			get => base.WTL_WTK_PickByLabelJob;
			set
			{
				base.WTL_WTK_PickByLabelJob = value;
				Package?.ClearActionStrategyCacheIncludingChildren();
			}
		}

		[RelatedBusinessObject("Package")]
		public override ZGuid WTL_KP_Package
		{
			get => base.WTL_KP_Package;
			set
			{
				base.WTL_KP_Package = value;
				Package?.ClearActionStrategyCacheIncludingChildren();
			}
		}

		public bool IsPickedFromPutawayLocation
		{
			get
			{
				var pickLines = Package?.GetPickLines();
				return pickLines != null && pickLines.All(pl => pl.IsPickedFromPutawayLocation);
			}
		}

		public bool IsPutawayIntoOutboundDDL
		{
			get
			{
				var pickLines = Package?.GetPickLines();
				return pickLines != null && pickLines.All(pl => !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty && (pl.IsPicked || pl.InventoryLine.IsFinalised) || pl.IsPickFinalised);
			}
		}

		public bool IsUsingDirectedPackingConsolidation => Order?.WD_UseDirectedPackingConsolidation ?? false;

		#endregion
	}
}

// Add tests to PickByLabel.Testing project.
