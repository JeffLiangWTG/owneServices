using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class eManifestJobInvoicingConsumerType : JobInvoicingConsumerType
	{
		public eManifestJobInvoicingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{ }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
