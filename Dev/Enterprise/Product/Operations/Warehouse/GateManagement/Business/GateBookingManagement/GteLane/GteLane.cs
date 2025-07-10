using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public sealed partial class GteLane : AutoGteLane
	{
		public GteLane(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Gate

		public GteGate Gate => Factory.Load<GteGate>(GLN_GTE_Gate);

		[RelatedBusinessObject("Gate")]
		public override ZGuid GLN_GTE_Gate
		{
			get => base.GLN_GTE_Gate;
			set => base.GLN_GTE_Gate = value;
		}
		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GLN_Direction = "BOTH";
		}

#endif
	}
}
