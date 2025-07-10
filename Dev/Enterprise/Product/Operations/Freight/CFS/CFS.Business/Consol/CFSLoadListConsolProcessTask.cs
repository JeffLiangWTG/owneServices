using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolProcessTask : ProcessTask
	{
		public CFSLoadListConsolProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CFSLoadListConsol); }
		}

		public new CFSLoadListConsol Parent
		{
			get { return (CFSLoadListConsol)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.LoadListConsol; }
		}
	}
}
