using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Environment.Business
{
	[CodeAlive("This Business Object is used in Glow.")]
	public class WhsRFRegistry : AutoWhsRFRegistry, IWhsRFRegistry
	{
		public WhsRFRegistry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WRR_WW_Whs); }
		}

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WRR_WW_Whs { get => base.WRR_WW_Whs; set => base.WRR_WW_Whs = value; }

		#endregion

		#region PutawayArea

		public WhsArea PutawayArea
		{
			get { return Factory.Load<WhsArea>(WRR_WA_PutawayArea); }
		}

		[RelatedBusinessObject("PutawayArea")]
		public override ZGuid WRR_WA_PutawayArea { get => base.WRR_WA_PutawayArea; set => base.WRR_WA_PutawayArea = value; }

		#endregion

		#region PickingArea

		public WhsArea PickingArea
		{
			get { return Factory.Load<WhsArea>(WRR_WA_PickingArea); }
		}

		[RelatedBusinessObject("PickingArea")]
		public override ZGuid WRR_WA_PickingArea { get => base.WRR_WA_PickingArea; set => base.WRR_WA_PickingArea = value; }

		#endregion

		#region UOMPackType

		[List("Lookups.UOMPackTypes")]
		public override ZString WRR_UOMPackType
		{
			get => base.WRR_UOMPackType;
			set => base.WRR_UOMPackType = value;
		}

		public const string DefaultUOMPackType = "ANY";

		#endregion
	}
}
