using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Business
{
	class eManifestProcessTask : ProcessTask, Integration.Customs.US.eManifest.ICusInBondHeaderProcessTask
	{
		public eManifestProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		protected override Type ParentType
		{
			get { return typeof(Trip); }
		}
	}
}
