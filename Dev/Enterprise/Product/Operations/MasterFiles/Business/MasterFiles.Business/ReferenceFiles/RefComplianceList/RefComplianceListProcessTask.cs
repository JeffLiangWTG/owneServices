using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListProcessTask : ProcessTasks
	{
		public RefComplianceListProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		protected internal override Type ParentType
		{
			get { return typeof(RefComplianceList); }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.RefComplianceList; }
		}

		#endregion
	}
}
