using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceProcessTask : ProcessTask
	{
		public CYDReleaseAdviceProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CYDReleaseAdvice);

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.CYDReleaseAdvice;

		#endregion
	}
}
