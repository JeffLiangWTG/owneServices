using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using IAsycudaManifestHeaderProcessTask = Enterprise.Integration.Customs.ZA.IAsycudaManifestHeaderProcessTask;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAsycudaManifestHeaderProcessTask : ProcessTask, IAsycudaManifestHeaderProcessTask
	{
		public ZAsycudaManifestHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override ControllerID ParentControllerID
		{
			get { return ZAControllerIDs.OutturnAndGateInOut; }
		}

		public new AsycudaManifestHeader Parent
		{
			get { return (AsycudaManifestHeader)base.Parent; }
		}

		protected override Type ParentType
		{
			get { return typeof(AsycudaManifestHeader); }
		}
	}
}
