using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentProcessTask : ProcessTask, IHVLVConsignmentProcessTask
	{
		public HVLVConsignmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(HVLVConsignment);

		public new HVLVConsignment Parent => (HVLVConsignment)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.HVLVBookingHeader;
	}
}
