using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallProcessTask : CRMProcessTask
	{
		public OrgSalesCallProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Communication; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(OrgSalesCall); }
		}

		#endregion
	}
}