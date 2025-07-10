using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public sealed partial class GteDockGatePivot : AutoGteDockGatePivot
	{
		public GteDockGatePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Wl_Dock

		public WhsLocation DockLocation => Factory.Load<WhsLocation>(GDP_WL_Dock);
		public GteGate Gate => Factory.Load<GteGate>(GDP_GTE_Gate);

		[RelatedBusinessObject("DockLocation")]
		public override ZGuid GDP_WL_Dock
		{
			get => base.GDP_WL_Dock;
			set => base.GDP_WL_Dock = value;
		}

		#endregion

		#region Gate

		[RelatedBusinessObject("Gate")]
		public override ZGuid GDP_GTE_Gate
		{
			get => base.GDP_GTE_Gate;
			set => base.GDP_GTE_Gate = value;
		}

		#endregion
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GDP_Direction = "BOTH";
		}

#endif
	}
}
