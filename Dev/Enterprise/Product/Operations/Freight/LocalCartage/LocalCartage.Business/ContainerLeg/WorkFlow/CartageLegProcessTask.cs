using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type")]
	class CartageLegProcessTask : ProcessTask, Integration.ICartageLegProcessTask
	{
		public CartageLegProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CommonCartageLeg); }
		}

		public new CommonCartageLeg Parent
		{
			get { return (CommonCartageLeg)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.CartageLeg; }
		}
	}
}
