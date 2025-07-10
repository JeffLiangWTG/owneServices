using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class YardUnit : AutoYardUnit
	{
		public YardUnit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Location

		[RelatedBusinessObject("Location")]
		[List("Lookups.Locations")]
		public override ZGuid GTY_WL_Location
		{
			get { return base.GTY_WL_Location; }
			set { base.GTY_WL_Location = value; }
		}

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(GTY_WL_Location); }
		}

		#endregion

		#region Facility

		[RelatedBusinessObject("Facility")]
		[List("Lookups.Warehouses")]
		public override ZGuid GTY_WW_Facility
		{
			get { return base.GTY_WW_Facility; }
			set { base.GTY_WW_Facility = value; }
		}

		public WhsWarehouse Facility
		{
			get { return Factory.Load<WhsWarehouse>(GTY_WW_Facility); }
		}

		#endregion

		#region YardUnitMovements

		[ChildEditable]
		public YardUnitMovementCollection YardUnitMovements
		{
			get
			{
				if (yardUnitMovements == null)
				{
					yardUnitMovements = new YardUnitMovementCollection(this);
					RegisterEditableChildObject(yardUnitMovements);
				}

				return yardUnitMovements;
			}
		}

		YardUnitMovementCollection yardUnitMovements;

		#endregion

		#region GTY_Quality

		[List("Lookups.Qualities")]
		public override ZString GTY_Quality
		{
			get => base.GTY_Quality;
			set { base.GTY_Quality = value; }
		}

		#endregion
	}
}
