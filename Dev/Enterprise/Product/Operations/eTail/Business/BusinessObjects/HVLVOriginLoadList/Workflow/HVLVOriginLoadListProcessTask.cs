using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListProcessTask : ProcessTask
	{
		public HVLVOriginLoadListProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID => ControllerIDs.HVLVOriginLoadList;

		protected override Type ParentType => typeof(HVLVOriginLoadList);

		public new HVLVOriginLoadList Parent => (HVLVOriginLoadList)base.Parent;
	}
}
