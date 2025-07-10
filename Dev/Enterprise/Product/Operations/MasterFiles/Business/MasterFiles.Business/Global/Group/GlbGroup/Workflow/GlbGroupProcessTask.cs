using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupProcessTask : ProcessTask
	{
		public GlbGroupProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		protected internal override Type ParentType => typeof(GlbGroup);

		public override ControllerID ParentControllerID => ControllerIDs.GlbGroup;

		#endregion
	}
}
