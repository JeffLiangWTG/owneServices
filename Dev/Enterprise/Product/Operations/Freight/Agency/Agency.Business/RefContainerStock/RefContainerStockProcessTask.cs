using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public class RefContainerStockProcessTask : ProcessTask, IRefContainerStockProcessTask
	{
		public RefContainerStockProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(RefContainerStock); }
		}

		public new RefContainerStock Parent
		{
			get { return (RefContainerStock)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AgencyContainerManager; }
		}
	}
}


