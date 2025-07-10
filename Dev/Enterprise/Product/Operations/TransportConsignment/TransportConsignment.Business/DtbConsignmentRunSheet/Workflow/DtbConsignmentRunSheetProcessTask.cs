using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetProcessTask : ProcessTask
	{
		public DtbConsignmentRunSheetProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		public new DtbConsignmentRunSheet Parent
		{
			get { return (DtbConsignmentRunSheet)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.DtbConsignmentRunSheet; }
		}
	}
}
