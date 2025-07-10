using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceProcessTask : ProcessTask
	{
		public CYDReceiveAdviceProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CYDReceiveAdvice);

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.CYDReceiveAdvice;

		#endregion
	}
}
