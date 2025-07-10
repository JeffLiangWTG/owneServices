using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartProcessTask : ProcessTask
	{
		public OrgSupplierPartProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WhsConfigProduct; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(OrgSupplierPart); }
		}

		#endregion
	}
}

