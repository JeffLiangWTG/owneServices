using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDAdHocService : AutoCYDAdHocService
	{
		public CYDAdHocService(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("ServiceOrder")]
		public override ZGuid YAS_YAO_ServiceOrder { get => base.YAS_YAO_ServiceOrder; set => base.YAS_YAO_ServiceOrder = value; }

		public CYDAdHocServiceOrder ServiceOrder
		{
			get => Factory.Load<CYDAdHocServiceOrder>(YAS_YAO_ServiceOrder);
		}

		[RelatedBusinessObject("YardUnitState")]
		public override ZGuid YAS_YUS_YardUnitState { get => base.YAS_YUS_YardUnitState; set => base.YAS_YUS_YardUnitState = value; }

		public CYDYardUnitState YardUnitState
		{
			get => Factory.Load<CYDYardUnitState>(YAS_YUS_YardUnitState);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("7a2e9ea8-154b-461b-b2a1-57d37536d4da", "Ad Hoc Service");
			}
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		#endregion
	}
}
