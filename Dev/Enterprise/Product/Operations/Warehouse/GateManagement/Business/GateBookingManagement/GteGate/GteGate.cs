using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public sealed partial class GteGate : AutoGteGate
	{
		public GteGate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GTE_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

#endif
	}
}
