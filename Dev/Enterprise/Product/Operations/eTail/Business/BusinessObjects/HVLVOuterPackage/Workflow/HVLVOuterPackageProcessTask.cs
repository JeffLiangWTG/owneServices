using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackageProcessTask : ProcessTask
	{
		public HVLVOuterPackageProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID => ControllerIDs.HVLVOuterPackage;

		protected override Type ParentType => typeof(HVLVOuterPackage);

		public new HVLVOuterPackage Parent => (HVLVOuterPackage)base.Parent;
	}
}
