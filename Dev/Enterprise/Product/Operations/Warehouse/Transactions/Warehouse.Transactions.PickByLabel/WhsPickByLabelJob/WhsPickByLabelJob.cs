using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelJob : AutoWhsPickByLabelJob, IWhsPickByLabelJob
	{
		public WhsPickByLabelJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WTK_WL_PutawayLocation), ConcurrencyPolicy.Strict);
		}

		#region Related Business Objects

		public WhsLocation DockDoorLocation => Factory.Load<WhsLocation>(WTK_WL_DockDoor);

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WTK_WW_Warehouse);

		public WhsPickByLabelLabelCollection Labels
		{
			get
			{
				if (labels == null)
				{
					labels = new WhsPickByLabelLabelCollection(this, Factory);
					labels.Load();
				}
				return labels;
			}
		}

		WhsPickByLabelLabelCollection labels;

		#endregion

		#region Properties

		[RelatedBusinessObject("DockDoorLocation")]
		public override ZGuid WTK_WL_DockDoor
		{
			get => base.WTK_WL_DockDoor;
			set => base.WTK_WL_DockDoor = value;
		}

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WTK_WW_Warehouse
		{
			get => base.WTK_WW_Warehouse;
			set => base.WTK_WW_Warehouse = value;
		}

		#endregion
	}
}

// Add tests to PickByLabel.Testing project.
