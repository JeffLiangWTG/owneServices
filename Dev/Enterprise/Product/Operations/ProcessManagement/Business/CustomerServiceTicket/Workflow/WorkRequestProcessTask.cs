using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestProcessTask : ProcessTask
	{
		public WorkRequestProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID => ControllerIDs.CustomerServiceTicket;

		protected override Type ParentType => typeof(WorkRequest);

		public new WorkRequest Parent => (WorkRequest)base.Parent;

		#endregion

		public override ZString P9_Status
		{
			get => base.P9_Status;
			set
			{
				base.P9_Status = value;

				Parent?.NotifyStatusNeedsReCalculationOnFactorySaving();
			}
		}

		public override void Delete()
		{
			var parent = Parent;

			base.Delete();

			parent?.NotifyStatusNeedsReCalculationOnFactorySaving();
		}
	}
}
