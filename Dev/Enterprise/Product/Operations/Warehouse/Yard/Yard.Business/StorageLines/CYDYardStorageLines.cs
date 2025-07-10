using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Invoicing.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	[DependentBusinessObject(typeof(CYDYardUnitState), "StorageLines")]
	public class CYDYardStorageLines(BusinessObjectFactory factory, DataRow row) : AutoCYDYardStorageLines(factory, row)
	{
		#region Properties

		[RelatedBusinessObject("YardUnit")]
		public override ZGuid YSL_YUS_YardUnit
		{
			get => base.YSL_YUS_YardUnit;
			set => base.YSL_YUS_YardUnit = value;
		}

		public CYDYardUnitState YardUnit
		{
			get => Factory.Load<CYDYardUnitState>(YSL_YUS_YardUnit);
		}

		[RelatedBusinessObject("PeriodicInvoicing")]
		public override ZGuid YSL_ET_JobStorage
		{
			get => base.YSL_ET_JobStorage;
			set => base.YSL_ET_JobStorage = value;
		}

		public PeriodicInvoicing PeriodicInvoicing
		{
			get => Factory.Load<PeriodicInvoicing>(YSL_ET_JobStorage);
		}

		#endregion
	}
}
