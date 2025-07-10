using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceProcessTask : ProcessTask
	{
		public CusUSLVClearanceProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		protected override Type ParentType
		{
			get { return typeof(CusUSLVClearance); }
		}

		public new CusUSLVClearance Parent
		{
			get { return (CusUSLVClearance)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}
	}
}
