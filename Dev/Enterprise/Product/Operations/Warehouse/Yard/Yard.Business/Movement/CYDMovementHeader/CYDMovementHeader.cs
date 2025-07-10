using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDMovementHeader : AutoCYDMovementHeader
	{
		public CYDMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid YMH_WW_Yard { get => base.YMH_WW_Yard; set => base.YMH_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YMH_WW_Yard);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YMH_WW_Yard = Factory.NewWithValidTestData<WhsWarehouse>().PK;
		}
#endif

		#endregion
	}
}
