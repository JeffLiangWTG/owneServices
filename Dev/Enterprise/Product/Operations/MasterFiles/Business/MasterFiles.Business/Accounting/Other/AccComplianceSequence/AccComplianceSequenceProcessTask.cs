using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceProcessTask : ProcessTasks
	{
		public AccComplianceSequenceProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AccComplianceSequence; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(AccComplianceSequence); }
		}

		#endregion
	}
}
